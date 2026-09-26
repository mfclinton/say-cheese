public static class ClothingTypeExtensions
{
    // Adjust the method to target ClothingSet.ClothingType
    public static string ToAbbreviation(this ClothingSet.ClothingType type)
    {
        switch (type)
        {
            case ClothingSet.ClothingType.LArm:
                return "Arm";
            case ClothingSet.ClothingType.RArm:
                return "Arm";
            case ClothingSet.ClothingType.LLeg:
                return "Leg";
            case ClothingSet.ClothingType.RLeg:
                return "Leg";
            case ClothingSet.ClothingType.LFeet:
                return "Feet";
            case ClothingSet.ClothingType.RFeet:
                return "Feet";
            case ClothingSet.ClothingType.Body:
                return "Body";
            case ClothingSet.ClothingType.Head:
                return "Head";
            case ClothingSet.ClothingType.Ear:
                return "Ears";
            case ClothingSet.ClothingType.Eye:
                return "Eyes";
            case ClothingSet.ClothingType.Glasses:
                return "Glasses";
            case ClothingSet.ClothingType.Hat:
                return "Hat";
            case ClothingSet.ClothingType.Hair:
                return "Hair";
            case ClothingSet.ClothingType.Eyebrow:
                return "Eye Brow";
            case ClothingSet.ClothingType.Holdable:
                return "Hand";
            default:
                return "Unknown";
        }
    }
}