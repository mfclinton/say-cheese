using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class PersonManager : MonoBehaviour
{
    [Header("Randomization")]
    [SerializeField, Range(10f, 20f)] private float minTimeToReachDestination = 15f;
    public float MinTimeToReachDestination => minTimeToReachDestination;
    [SerializeField, Range(20f, 120)] private float maxTimeToReachDestination = 80;
    public float MaxTimeToReachDestination => maxTimeToReachDestination;

    [SerializeField] private float partialImposterChance = 0.1f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject personPrefab;
    
    [Header("Clothing Sets")]
    [SerializeField] ClothingSet[] clothingSets;
    [SerializeField] ClothingSet[] identifiableClothingSets;
    
    // Event for when ban list is set
    public event Action<Dictionary<ClothingSet, int>> OnBanListSet;
    
    public static PersonManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        LoadLevel();
    }

    private void LoadLevel()
    {
        if (LevelManager.Instance == null)
            return;
        
        LevelSettings levelSettings = LevelManager.Instance.CurrentLevelSettings;
        minTimeToReachDestination = levelSettings.minTimeToReachDestination;
        maxTimeToReachDestination = levelSettings.maxTimeToReachDestination;
        clothingSets = levelSettings.clothingSets;
        identifiableClothingSets = levelSettings.identifiableClothingSets;
        partialImposterChance = levelSettings.partialImposterChance;
    }

    public Person SpawnPerson(Vector3 position)
    {
        Person p = Instantiate(personPrefab, position, Quaternion.identity).GetComponent<Person>();
        return p;
    }
    
    private void SetPersonClothing(PersonVisualizer pv, Dictionary<ClothingSet, int> bannedSpriteIndexes, bool badPersonMustHaveAll)
    {
        // Initializes Random Clothing Vector
        Dictionary<ClothingSet, int> chosenIndexes = new Dictionary<ClothingSet, int>();
        for (int i = 0; i < clothingSets.Length; i++)
        {
            ClothingSet clothingSet = clothingSets[i];
            bool setToNone = clothingSet.allowedToBeNone && Random.value < clothingSet.probOfBeingNone;
            if (setToNone)
            {
                chosenIndexes.Add(clothingSet, -1);
                continue;
            }
            
            int randomIndex = Random.Range(0, clothingSet.ClothingSprites.Length);
            chosenIndexes.Add(clothingSet, randomIndex);
        }
        
        bool isPartialImpostor = Random.value < partialImposterChance;
        if (isPartialImpostor && badPersonMustHaveAll)
        {
            // Select from banned sprite indexes
            KeyValuePair<ClothingSet, int> randomBannedSpriteIndex = bannedSpriteIndexes.ElementAt(Random.Range(0, bannedSpriteIndexes.Count));
            chosenIndexes[randomBannedSpriteIndex.Key] = randomBannedSpriteIndex.Value;
        }
        
        // Validates
        List<ClothingSet> brokenRules = new List<ClothingSet>();
        foreach (KeyValuePair<ClothingSet, int> entry in bannedSpriteIndexes)
        {
            ClothingSet clothingSet = entry.Key;
            int bannedIndex = entry.Value;
            bool ruleBroken = chosenIndexes[clothingSet] == bannedIndex;
            if (ruleBroken)
                brokenRules.Add(clothingSet);
        }
        
        // Updates
        int numRulesBroken = brokenRules.Count;
        if (badPersonMustHaveAll && numRulesBroken == bannedSpriteIndexes.Count)
        {
            ClothingSet randomBrokenRule = brokenRules[Random.Range(0, brokenRules.Count)];
            FlipClothingSetIndex(randomBrokenRule);
        }
        else if (!badPersonMustHaveAll && numRulesBroken > 0)
        {
            foreach (ClothingSet clothingSet in brokenRules)
                FlipClothingSetIndex(clothingSet);
        }

        // Sets the Clothing
        for (int i = 0; i < clothingSets.Length; i++)
        {
            ClothingSet clothingSet = clothingSets[i];
            int randomIndex = chosenIndexes[clothingSet];
            for (int j = 0; j < clothingSet.ClothingRendererTypes.Length; j++)
            {
                ClothingSet.ClothingType clothingType = clothingSet.ClothingRendererTypes[j];
                Sprite sprite = randomIndex == -1 ? null : clothingSet.ClothingSprites[randomIndex].sprites[j];
                pv.SetClothingSprite(clothingType, sprite);
            }
        }

        void FlipClothingSetIndex(ClothingSet clothingSet)
        {
            int randomIndex = Random.Range(0, clothingSet.ClothingSprites.Length);
            while (randomIndex == chosenIndexes[clothingSet])
                randomIndex = Random.Range(0, clothingSet.ClothingSprites.Length);

            chosenIndexes[clothingSet] = randomIndex;
        }
    }


    public void InitializePeople(int personCount, int badPersonCount, int badPersonHintCount, int laneCount, bool badPersonMustHaveAll)
    {
        Dictionary<ClothingSet, int> bannedSpriteIndexes = GenerateBannedSpriteIndexes(badPersonHintCount);
        
        List<Person> people = new List<Person>();
        for (int i = 0; i < personCount + badPersonCount; i++)
        {
            // Creates Person
            (Vector3 spawnPos, Vector3 destinationPos) = GenerateSpawnAndDestinationPos();
            Person p = SpawnPerson(spawnPos);
            people.Add(p);
            
            // Sets Person Clothing
            PersonVisualizer pv = p.GetComponent<PersonVisualizer>();
            if (pv != null)
                SetPersonClothing(pv, bannedSpriteIndexes, badPersonMustHaveAll);
            
            // Configures Person's Movement
            int target_lane = Random.Range(0, laneCount);
            float timeToReachDestination = Random.Range(minTimeToReachDestination, maxTimeToReachDestination);
            p.Initialize(spawnPos.x, destinationPos.x, target_lane, timeToReachDestination);
        }
        
        // Transforms some people into bad people
        for (int i = 0; i < badPersonCount; i++)
            SetPersonToBad(people[i], bannedSpriteIndexes, badPersonMustHaveAll);
    }
    
    private void SetPersonToBad(Person p, Dictionary<ClothingSet, int> bannedSpriteIndexes, bool badPersonMustHaveAll)
    {
        PersonVisualizer pv = p.GetComponent<PersonVisualizer>();
        if (!badPersonMustHaveAll)
        {
            int numClothingSets = Random.Range(1, bannedSpriteIndexes.Count + 1);
            bannedSpriteIndexes = bannedSpriteIndexes.OrderBy(x => Random.value).Take(numClothingSets).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        foreach (KeyValuePair<ClothingSet, int> entry in bannedSpriteIndexes)
        {
            ClothingSet clothingSet = entry.Key;
            int spriteIndex = entry.Value;

            for (int i = 0; i < clothingSet.ClothingRendererTypes.Length; i++)
                pv.SetClothingSprite(clothingSet.ClothingRendererTypes[i], clothingSet.ClothingSprites[spriteIndex].sprites[i]);
        }
        
        p.IsTarget = true;
        p.gameObject.name = "Bad Person";
    }
    
    private (Vector3 spawnPos, Vector3 destinationPos) GenerateSpawnAndDestinationPos()
    {
        bool isLeft = Random.value < 0.5f;
        Vector3 spawnPos = isLeft ? LaneManager.Instance.spawnNodeL : LaneManager.Instance.spawnNodeR;
        Vector3 destinationPos = isLeft ? LaneManager.Instance.spawnNodeR : LaneManager.Instance.spawnNodeL;

        return (spawnPos, destinationPos);
    }
    
    private Dictionary<ClothingSet, int> GenerateBannedSpriteIndexes(int badPersonCount)
    {
        Dictionary<ClothingSet, int> bannedClothingSetDict = new Dictionary<ClothingSet, int>();
        ClothingSet[] clothingSetBanPool = Utils.GenerateUniqueNumbers(badPersonCount, identifiableClothingSets.Length).Select(i => this.identifiableClothingSets[i]).ToArray();
        foreach (ClothingSet clothingSet in clothingSetBanPool)
        {
            int randomIndex = Random.Range(0, clothingSet.ClothingSprites.Length);
            bannedClothingSetDict.Add(clothingSet, randomIndex);
        }
        
        OnBanListSet?.Invoke(bannedClothingSetDict);
        return bannedClothingSetDict;
    }
}
