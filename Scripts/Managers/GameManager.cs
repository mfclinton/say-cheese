using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float totalGameTime = 120f;
    
    [SerializeField] private int personCount = 10;
    [SerializeField] private int badPersonCount = 3;
    [SerializeField] private int extraBadPersonCount = 0;
    [SerializeField] private int badPersonHintCount = 3;
    [SerializeField] private int laneCount = 3;
    [SerializeField] private float timePenaltyForWrongGuess = 20f;
    [SerializeField] private float timeBonusForCorrectGuess = 5f;
    [SerializeField] private float timePoolForCombo = 8f;
    [SerializeField] private bool badPersonMustHaveAll = true;
    public int LaneCount => laneCount;

    
    [Header("Prefabs")]
    [SerializeField] private ClickEffect wrongClickEffectPrefab;
    [SerializeField] private ClickEffect rightClickEffectPrefab;
    
    // Internal variables
    private float elapsedTime;
    private int score;
    private int currentCorrectGuessCombo;
    private float timeLastCombo;
    
    // Metrics
    public float rawElapsedTime { get; private set; }
    public int correctGuesses { get; private set; }
    public int wrongGuesses { get; private set; }
    
    // Event for time elapsed in game
    public event Action OnPersonClicked;
    public event Action<float, float> OnTimeElapsed;
    public event Action<float, int> OnComboTimeUpdated;
    public event Action<int, int> OnScoreUpdated;
    public event Action<bool> OnGameOver;
    
    private PlayerController playerController;
    public static GameManager Instance { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool BadPersonMustHaveAll => badPersonMustHaveAll;
    public float ComboTimeFill => Mathf.Clamp01(1f - (Time.time - timeLastCombo) / timePoolForCombo);
    
    private void Awake()
    {
        Instance = this;
        playerController = FindObjectOfType<PlayerController>();
        
        // Metrics
        LoadLevelConfig();
        LoadMap();
    }

    private void FixedUpdate()
    {
        UpdateTime();
        OnComboTimeUpdated?.Invoke(ComboTimeFill, currentCorrectGuessCombo);

        // Trigger Sound Effect
        bool isLowBattery = (elapsedTime >= totalGameTime * 4f / 5f && elapsedTime < totalGameTime) && !IsGameOver;
        TriggerLowBatteryEvent(isLowBattery);
        
        // Time Out for Game Over
        if (elapsedTime >= totalGameTime && !IsGameOver)
        {
            Debug.Log("YOU LOSE!");
            IsGameOver = true;
            OnGameOver?.Invoke(false);
            
            // Reset after everything
            LevelManager.Instance.ResetRunStats(true);
        }
    }
    
    private void LoadLevelConfig()
    {
        if (LevelManager.Instance == null)
            return;

        LevelManager.Instance.InitializeLevel();
        LevelSettings levelSettings = LevelManager.Instance.CurrentLevelSettings;
        
        personCount = levelSettings.personCount;
        badPersonCount = levelSettings.badPersonCount;
        extraBadPersonCount = levelSettings.extraBadPersonCount;
        badPersonHintCount = levelSettings.badPersonHintCount;
        laneCount = levelSettings.laneCount;
        totalGameTime = levelSettings.totalGameTime;
        timePenaltyForWrongGuess = levelSettings.timePenaltyForWrongGuess;
        timeBonusForCorrectGuess = levelSettings.timeBonusForCorrectGuess;
        timePoolForCombo = levelSettings.timePoolForCombo;
        badPersonMustHaveAll = levelSettings.badPersonMustHaveAll;
    }
    
    private void LoadMap()
    {
        MapData mapData = MapManager.Instance.GetCurrentMap();
        if (mapData != null)
        {
            foreach(MapData map in MapManager.Instance.Maps)
                map.mapParent.SetActive(false);
            
            mapData.mapParent.SetActive(true);
        }
    }

    private void UpdateTime()
    {
        if(ComboTimeFill <= 0)
            currentCorrectGuessCombo = 0;
        
        if (!IsGameOver)
        {
            rawElapsedTime += Time.deltaTime;
            elapsedTime += Time.deltaTime;
        }
        OnTimeElapsed?.Invoke(elapsedTime, totalGameTime);
    }

    private void Start()
    {
        IsGameOver = false;
        PersonManager.Instance.InitializePeople(personCount, badPersonCount + extraBadPersonCount, badPersonHintCount, laneCount, badPersonMustHaveAll);
        OnScoreUpdated?.Invoke(score, badPersonCount);
    }

    private void HandlePersonClicked()
    {
        OnPersonClicked?.Invoke();
    }
    
    public void OnWrongTargetClicked()
    {
        wrongGuesses++;
        ProcessClick(false);
        HandlePersonClicked();
    }
    
    void ClickEffectVisual(float prevTime, float timeElapsedDelta, ClickEffect clickEffectPrefab)
    {
        string timeElapsedStr = Utils.FormatTime(-timeElapsedDelta);
        if(timeElapsedDelta < 0)
            timeElapsedStr = "+" + timeElapsedStr;
        
        Vector3 mousePos = playerController.PointerWorldPosition;
        
        ClickEffect clickEffect = Instantiate(clickEffectPrefab, mousePos, Quaternion.identity);
        clickEffect.Initialize(timeElapsedStr, prevTime, elapsedTime, totalGameTime);
    }

    public void OnCorrectTargetClicked()
    {
        score++;
        correctGuesses++;
        ProcessClick(true);
        
        HandlePersonClicked();
        OnScoreUpdated?.Invoke(score, badPersonCount);
        
        if (score == badPersonCount)
        {
            IsGameOver = true;
            OnGameOver?.Invoke(true);
        }
    }
    
    private void ProcessClick(bool isCorrect)
    {
        float prevTime = elapsedTime;
        float timeElapsedDelta = 0f;
        ClickEffect clickEffectPrefab;
        if (isCorrect)
        {
            currentCorrectGuessCombo++;
            timeLastCombo = Time.time;
            if (currentCorrectGuessCombo <= 1)
                return;
            
            timeElapsedDelta = -timeBonusForCorrectGuess * (currentCorrectGuessCombo - 1);
            clickEffectPrefab = rightClickEffectPrefab;
        }
        else
        {
            currentCorrectGuessCombo = 0;
            timeElapsedDelta = timePenaltyForWrongGuess;
            clickEffectPrefab = wrongClickEffectPrefab;
            
            CameraSlider.Instance.Shake();
            AudioManager.Instance.OnBadCapture();
        }
        
        elapsedTime = Mathf.Clamp(elapsedTime + timeElapsedDelta, 0f, totalGameTime);
        ClickEffectVisual(prevTime, timeElapsedDelta, clickEffectPrefab);
    }
    
    void TriggerLowBatteryEvent(bool play)
    {
        if (play)
        {
            bool playedBatterySound = AudioManager.Instance.PlayLowBatterySound();
            if (playedBatterySound)
                UIManager.Instance.PulseLowBatteryLife();
        }
        else
        {
            AudioManager.Instance.StopLowBatterySound();
            UIManager.Instance.StopPulseLowBatteryLife();
        }
    }
}
