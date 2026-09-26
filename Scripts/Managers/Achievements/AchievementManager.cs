using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AchievementData[] achievementData;
    public AchievementData[] AchievementData => achievementData;
    
    // Singleton
    public static AchievementManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void EvaluateAchievements()
    {
        foreach (AchievementData data in AchievementData)
        {
            AchievementThresholdEntry[] achievementResults = data.GetAchievementThresholdEntries(data.CalculateRunAchievementValue());
            if (achievementResults == null)
                continue;
            
            foreach (AchievementThresholdEntry achievementResult in achievementResults)
            {
                // TODO: GET ACHIEVEMENT
            }
        }
    }
}
