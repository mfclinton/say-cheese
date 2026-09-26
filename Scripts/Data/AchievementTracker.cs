using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI runValueText;
    [SerializeField] private TextMeshProUGUI levelValueText;
    [SerializeField] private Image awardIcon;
    [SerializeField] private Image background;
    
    [Header("Won / Loss References")]
    [SerializeField] private Sprite wonSprite;
    [SerializeField] private Sprite lossSprite;
    
    [SerializeField] private float timeToAnimate = 1f;
    
    // Animation States
    private const string ANIMATOR_BOOL = "poppedOut";
    
    // Internal References
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        SetAnimatorState(true);
    }
    
    private void SetAnimatorState(bool state)
    {
        if (animator == null)
            return;
        
        animator.SetBool(ANIMATOR_BOOL, state);
    }
    
    private void SetDataFields(string achievementName, string runValue, string levelValue, Sprite awardSprite)
    {
        label.text = achievementName;
        runValueText.text = runValue;
        
        if (levelValueText != null)
            levelValueText.text = "+" + levelValue;
        
        if (awardSprite != null)
            awardIcon.sprite = awardSprite;
        else
            awardIcon.gameObject.SetActive(false);
    }

    public void SetAchievementData(AchievementData data)
    {
        string achievementName = data.achievementName;
        float runValue = data.CalculateRunAchievementValue();
        float levelValue = data.GetLevelAchievementValue();
        Sprite awardSprite = data.GetAchievementThresholdEntry(runValue)?.awardSprite;
        
        SetDataFields(achievementName, data.FormatValue(runValue), data.FormatValue(levelValue), awardSprite);
    }

    public void SetBackground(bool won)
    {
        if (background == null)
            return;

        background.sprite = won ? wonSprite : lossSprite;
        if (!won)
            runValueText.color = Color.red;
    }
}
