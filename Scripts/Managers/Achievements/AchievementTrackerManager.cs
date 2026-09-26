using System;
using UnityEngine;

public class AchievementTrackerManager : MonoBehaviour
{
    [SerializeField] private AchievementTracker[] trackers;
    [SerializeField] private AchievementTracker[] computerScreenTrackers;

    private void Awake()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.OnGameOver += HandleGameOver;
    }

    private void HandleGameOver(bool playerWon)
    {
        AchievementData[] achievementData = AchievementManager.Instance.AchievementData;
        
        for (int i = 0; i < trackers.Length; i++)
        {
            AchievementTracker tracker = trackers[i];
            tracker.SetBackground(playerWon);
            tracker.SetAchievementData(achievementData[i]);
        }
        
        SetComputerScreenTrackers();
    }
    
    public void SetComputerScreenTrackers()
    {
        for (int i = 0; i < computerScreenTrackers.Length; i++)
        {
            AchievementTracker tracker = computerScreenTrackers[i];
            tracker.SetAchievementData(AchievementManager.Instance.AchievementData[i]);
        }
    }
}
