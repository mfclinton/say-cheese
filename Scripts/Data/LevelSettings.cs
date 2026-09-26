using UnityEngine;

[CreateAssetMenu(fileName = "LevelSettings", menuName = "LevelSettings", order = 1)]
public class LevelSettings : ScriptableObject
{
    public enum MapType
    {
        Base,
        TimesSquare,
        Subways,
    }
    
    // GameManager settings
    public float totalGameTime = 120f;
    public int personCount = 10;
    public int badPersonCount = 3;
    public int extraBadPersonCount = 0;
    public int badPersonHintCount = 3;
    public int laneCount = 3;
    public float slowDownFactor = 0.5f;
    public float timePenaltyForWrongGuess = 15f;
    public float timeBonusForCorrectGuess = 5f;
    public float timePoolForCombo = 8f;
    public bool badPersonMustHaveAll = true;
    public MapType mapType;
    public bool isCheckpoint = false;
    public float partialImposterChance = 0.1f;

    // PersonManager settings
    [Range(10f, 20f)] public float minTimeToReachDestination = 15f;
    [Range(20f, 120f)] public float maxTimeToReachDestination = 80f;
    public ClothingSet[] clothingSets;
    public ClothingSet[] identifiableClothingSets;
}