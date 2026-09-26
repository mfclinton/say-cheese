using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<LevelSettings> levels;
    public List<LevelSettings> Levels => levels;
    
    // Internal References
    public int currentLevelIndex { get; private set; }
    public LevelSettings CurrentLevelSettings => levels[currentLevelIndex];
    
    // Run Stats
    public int correctGuessesMade { get; private set; }
    public int mistakesMade { get; private set; }
    public float runTime { get; private set; }
    
    // Music Helper
    public float CurrentMusicStartTime { get; set; }
    
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton
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
    
    public void InitializeLevel()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += HandleGameOver;
    }

    private void HandleGameOver(bool won)
    {
        UpdateRunStats();
        
        if (won)
        {
            if(currentLevelIndex == levels.Count - 1)
            {
                SetRunStats();

                // End Screen
                GameOverAnimationManager gameOverAnimationManager = FindObjectOfType<GameOverAnimationManager>();
                if (gameOverAnimationManager != null)
                    gameOverAnimationManager.OnBeatGame();
                
                // Evaluate Achievements
                AchievementManager.Instance.EvaluateAchievements();
            }
            
            if(currentLevelIndex < levels.Count - 1)
                currentLevelIndex++;
        }
    }


    public void ResetRunStats(bool loadCheckpoint = false)
    {
        int checkpointLevelIndex = GetLastCheckpointLevelIndex();
        if (loadCheckpoint && 0 < checkpointLevelIndex)
        {
            currentLevelIndex = checkpointLevelIndex;
        }
        else
        {
            mistakesMade = 0;
            runTime = 0;
            currentLevelIndex = 0;
            correctGuessesMade = 0;
        }
    }
    
    public void UpdateRunStats()
    {
        float elapsedTime =  GameManager.Instance.rawElapsedTime;
        runTime += elapsedTime;

        mistakesMade += GameManager.Instance.wrongGuesses;
        correctGuessesMade += GameManager.Instance.correctGuesses;
    }

    public void SetRunStats()
    {
        string mistakesMadeKey = AchievementData.AchievementCategory.MistakesMade.ToString();
        string rawTimeTaken = AchievementData.AchievementCategory.RawTimeTaken.ToString();
        string accuracy = AchievementData.AchievementCategory.Accuracy.ToString();


        if (!PlayerPrefs.HasKey(mistakesMadeKey) || PlayerPrefs.GetFloat(mistakesMadeKey) == -1 || mistakesMade < PlayerPrefs.GetFloat(mistakesMadeKey))
        {
            PlayerPrefs.SetFloat(mistakesMadeKey, mistakesMade);
        }

        if (!PlayerPrefs.HasKey(rawTimeTaken) || PlayerPrefs.GetFloat(rawTimeTaken) == -1 || runTime < PlayerPrefs.GetFloat(rawTimeTaken))
        {
            PlayerPrefs.SetFloat(rawTimeTaken, runTime);
        }
        
        float accuracyValue = (correctGuessesMade / (correctGuessesMade + mistakesMade));
        if (!PlayerPrefs.HasKey(accuracy) || PlayerPrefs.GetFloat(accuracy) == -1 || accuracyValue > PlayerPrefs.GetFloat(accuracy))
        {
            PlayerPrefs.SetFloat(accuracy, accuracyValue);
        }
    }

    public int GetLastCheckpointLevelIndex()
    {
        for (int i = currentLevelIndex; 0 <= i; i--)
        {
            if (levels[i].isCheckpoint)
                return i;
        }
        
        return -1;
    }
}