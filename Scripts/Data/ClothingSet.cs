using UnityEngine;

[CreateAssetMenu(fileName = "ClothingSet", menuName = "ClothingSet", order = 0)]
public class ClothingSet : ScriptableObject
{
    public enum ClothingType
    {
        LArm,
        RArm,
        LLeg,
        RLeg,
        LFeet,
        RFeet,
        Body,
        Head,
        Ear,
        Eye,
        Glasses,
        Hat,
        Hair,
        Eyebrow,
        Holdable,
    }
    
    [System.Serializable]
    public struct SpritesGroup
    {
        public Sprite[] sprites;
    }
    
    [Header("Data")]
    public ClothingType[] ClothingRendererTypes;
    public SpritesGroup[] ClothingSprites;
    [Header("Randomization")]
    public bool allowedToBeNone;
    [Range(0.0f, 1.0f)] public float probOfBeingNone;
    
    private void OnValidate()
    {
        ValidateSpriteGroups();
    }

    private void ValidateSpriteGroups()
    {
        if(ClothingSprites == null || ClothingSprites.Length == 0)
            return;
        
        foreach(var spriteGroup in ClothingSprites)
        {
            // Check that nothing is null
            foreach (var sprite in spriteGroup.sprites)
            {
                if (sprite == null)
                {
                    Debug.LogError("ClothingSet: " + name + " has a null sprite");
                    return;
                }
            }
            
            if (spriteGroup.sprites.Length != ClothingRendererTypes.Length)
            {
                Debug.LogError("ClothingSet: " + name + " has a mismatch between the number of sprites and the number of sprite renderers");
                return;
            }
        }
    }
}
