using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform achievementRowsParent;
    [SerializeField] private AchievementRow achievementRowPrefab;
    [SerializeField] private GameObject awardPrefab;
    
    [SerializeField] private GameObject achievementUI;
    
    public static AchievementsUIManager instance;

    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        InitializeAchievements();
    }
    
    private void InitializeAchievements()
    {
        var achievementData = AchievementManager.Instance.AchievementData;
        foreach (var data in achievementData)
            CreateAchievementRow(data);
    }
    
    private void CreateAchievementRow(AchievementData data)
    {
        
        string key = data.achievementCategory.ToString();
        float value = PlayerPrefs.GetFloat(key, -1);
        
        var row = Instantiate(achievementRowPrefab, achievementRowsParent);
        row.achievementName.text = data.achievementName;
        row.recordValueText.text = data.FormatValue(value);
        
        int awardIndex = data.GetAwardIndex(value);
        for (int i = 0; i < data.achievementThresholds.Length; i++)
        {
            // Get Award UI Assets
            var award = Instantiate(awardPrefab, row.awardsParent.transform);
            Image awardIcon = award.GetComponentInChildren<Image>();
            var awardThreshText = award.GetComponentInChildren<TextMeshProUGUI>();
            
            // Set Award UI Assets
            awardIcon.sprite = data.achievementThresholds[i].awardSprite;
            awardThreshText.text = data.FormatValue(data.achievementThresholds[i].value);

            // Set Award UI Visibility
            if (awardIndex < i)
            {
                awardIcon.color = new Color(1, 1, 1, 0.3f);
                awardThreshText.color = new Color(0, 0, 0, 0.3f);
            }
        }
    }
    
    public void ToggleAchievementsUI()
    {
        achievementUI.SetActive(!achievementUI.activeSelf);
    }
}
