using UnityEngine;

[RequireComponent(typeof(Person))]
public class PersonVisualizer : MonoBehaviour
{
    [Header("SpriteRenderers")]
    [SerializeField] private SpriteRenderer LArm;
    [SerializeField] private SpriteRenderer RArm;
    [SerializeField] private SpriteRenderer LLeg;
    [SerializeField] private SpriteRenderer RLeg;
    [SerializeField] private SpriteRenderer LFeet;
    [SerializeField] private SpriteRenderer RFeet;
    [SerializeField] private SpriteRenderer Body;
    [SerializeField] private SpriteRenderer Head;
    [SerializeField] private SpriteRenderer Ear;
    [SerializeField] private SpriteRenderer Eye;
    [SerializeField] private SpriteRenderer Glasses;
    [SerializeField] private SpriteRenderer Hat;
    [SerializeField] private SpriteRenderer Hair;
    [SerializeField] private SpriteRenderer Eyebrow;
    [SerializeField] private SpriteRenderer Holdable;
    
    public void SetClothingSprite(ClothingSet.ClothingType clothingType, Sprite sprite)
    {
        switch (clothingType)
        {
            case ClothingSet.ClothingType.LArm:
                LArm.sprite = sprite;
                break;
            case ClothingSet.ClothingType.RArm:
                RArm.sprite = sprite;
                break;
            case ClothingSet.ClothingType.LLeg:
                LLeg.sprite = sprite;
                break;
            case ClothingSet.ClothingType.RLeg:
                RLeg.sprite = sprite;
                break;
            case ClothingSet.ClothingType.LFeet:
                LFeet.sprite = sprite;
                break;
            case ClothingSet.ClothingType.RFeet:
                RFeet.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Body:
                Body.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Head:
                Head.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Ear:
                Ear.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Eye:
                Eye.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Glasses:
                Glasses.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Hat:
                Hat.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Hair:
                Hair.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Eyebrow:
                Eyebrow.sprite = sprite;
                break;
            case ClothingSet.ClothingType.Holdable:
                Holdable.sprite = sprite;
                break;
        }
    }

    public SpriteRenderer GetClothingSprite(ClothingSet.ClothingType clothingType)
    {
        SpriteRenderer result = null;
        switch (clothingType)
        {
            case ClothingSet.ClothingType.LArm:
                result = LArm;
                break;
            case ClothingSet.ClothingType.RArm:
                result = RArm;
                break;
            case ClothingSet.ClothingType.LLeg:
                result = LLeg;
                break;
            case ClothingSet.ClothingType.RLeg:
                result = RLeg;
                break;
            case ClothingSet.ClothingType.LFeet:
                result = LFeet;
                break;
            case ClothingSet.ClothingType.RFeet:
                result = RFeet;
                break;
            case ClothingSet.ClothingType.Body:
                result = Body;
                break;
            case ClothingSet.ClothingType.Head:
                result = Head;
                break;
            case ClothingSet.ClothingType.Ear:
                result = Ear;
                break;
            case ClothingSet.ClothingType.Eye:
                result = Eye;
                break;
            case ClothingSet.ClothingType.Glasses:
                result = Glasses;
                break;
            case ClothingSet.ClothingType.Hat:
                result = Hat;
                break;
            case ClothingSet.ClothingType.Hair:
                result = Hair;
                break;
            case ClothingSet.ClothingType.Eyebrow:
                result = Eyebrow;
                break;
            case ClothingSet.ClothingType.Holdable:
                result = Holdable;
                break;
        }

        return result;
    }
}