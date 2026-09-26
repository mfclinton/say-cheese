using System;
using UnityEngine;

[Serializable]
public class AchievementThresholdEntry
{
    public float value;
    public Sprite awardSprite;
    public int newGroundsID;
    public string agiQuestID;
}

[CreateAssetMenu(fileName = "AchievementData", menuName = "AchievementData", order = 0)]
public class AchievementData : ScriptableObject
{
    public enum AchievementType
    {
        Min,
        Max,
    }
    
    public enum AchievementCategory
    {
        MistakesMade,
        RawTimeTaken,
        Accuracy
    }
    
    public AchievementType achievementType;
    public string achievementName;
    public AchievementCategory achievementCategory;
    public AchievementThresholdEntry[] achievementThresholds;
    
    public int GetAwardIndex(float value)
    {
        if(value == -1)
            return -1;
        
        for (int i = 0; i < achievementThresholds.Length; i++)
        {
            // Handle Different Achievement Types
            if (achievementType == AchievementType.Min)
            {
                if (value > achievementThresholds[i].value)
                    return i - 1;
            }
            else
            {
                if (value < achievementThresholds[i].value)
                    return i - 1;
            }
        }
        
        return achievementThresholds.Length - 1;
    }
    
    public AchievementThresholdEntry GetAchievementThresholdEntry(float value)
    {
        int awardIndex = GetAwardIndex(value);
        if(awardIndex == -1)
            return null;
        
        return achievementThresholds[awardIndex];
    }

    public AchievementThresholdEntry[] GetAchievementThresholdEntries(float value)
    {
        int awardIndex = GetAwardIndex(value);
        if(awardIndex == -1)
            return null;
        
        // All all awards in range of (0, awardIndex)
        AchievementThresholdEntry[] entries = new AchievementThresholdEntry[awardIndex + 1];
        for (int i = 0; i <= awardIndex; i++)
        {
            entries[i] = achievementThresholds[i];
        }
        
        return entries;
    }

    public string FormatValue(float value)
    {
        if(value == -1)
            return "N/A";
        
        if(achievementCategory == AchievementCategory.MistakesMade)
        {
            return ((int) value).ToString();
        }
        else if(achievementCategory == AchievementCategory.RawTimeTaken)
        {
            return Utils.FormatTime(value);
        }
        else if(achievementCategory == AchievementCategory.Accuracy)
        {
            return (value * 100).ToString() + "%";
        }
        
        return "N/A";
    }
    
    public bool UpdateAchievement()
    {
        string key = achievementCategory.ToString();
        float value = CalculateRunAchievementValue();
        
        // First Record
        if (!PlayerPrefs.HasKey(key) || PlayerPrefs.GetFloat(key) == -1)
        {
            PlayerPrefs.SetFloat(key, value);
            return true;
        }
        
        // Update Record
        float currentValue = PlayerPrefs.GetFloat(key);
        bool updateRecord = AchievementType.Min == achievementType ? value < currentValue : value > currentValue;
        if (updateRecord)
        {
            PlayerPrefs.SetFloat(key, value);
            return true;
        }

        return false;
    }
    
    public float CalculateRunAchievementValue()
    {
        if(achievementCategory == AchievementCategory.MistakesMade)
        {
            return LevelManager.Instance.mistakesMade;
        }
        else if(achievementCategory == AchievementCategory.RawTimeTaken)
        {
            return LevelManager.Instance.runTime;
        }
        else if(achievementCategory == AchievementCategory.Accuracy)
        {
            return LevelManager.Instance.correctGuessesMade / (LevelManager.Instance.correctGuessesMade + LevelManager.Instance.mistakesMade);
        }
        
        return -1;
    }
    
    public float GetLevelAchievementValue()
    {
        if(achievementCategory == AchievementCategory.MistakesMade)
        {
            return GameManager.Instance.wrongGuesses;
        }
        else if(achievementCategory == AchievementCategory.RawTimeTaken)
        {
            return GameManager.Instance.rawElapsedTime;
        }
        else if(achievementCategory == AchievementCategory.Accuracy)
        {
            return GameManager.Instance.correctGuesses / (GameManager.Instance.correctGuesses + GameManager.Instance.wrongGuesses);
        }
        
        return -1;
    }
}
