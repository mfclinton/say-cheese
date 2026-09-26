using System;
using System.Collections;
using System.Collections.Generic;
using EasyTransition;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image batteryLife;
    [SerializeField] private TextMeshProUGUI betteryLifeText;
    [SerializeField] private Image lowBatteryLifeBackfill;
    [SerializeField] private float lowBatteryPulseLength = 1f;
    [SerializeField] private AnimationCurve lowBatteryPulseCurve;

    [SerializeField] private Transform banListParent;
    [SerializeField] private GameObject banItemPrefab;
    
    [SerializeField] private Transform badGuySymbolsParent;
    [SerializeField] private Image badGuySymbolPrefab;
    [SerializeField] private float badGuySymboleDeleteDuration = .5f;

    [SerializeField] private GameObject gameOverPanel;
    
    [SerializeField] private GameObject winResultPanel;
    [SerializeField] private GameObject loseResultPanel;
    
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private bool isMainMenu = false;

    [SerializeField] private GameObject[] tutorialPanels;
    [SerializeField] private GameObject instructionsSplashScreen;
    public GameObject InstructionsSplashScreen => instructionsSplashScreen;
    
    [SerializeField] private TextMeshProUGUI levelText;
    
    [SerializeField] private GameObject AnyText;
    [SerializeField] private GameObject AllText;
    
    [SerializeField] private ScriptableRendererFeature glitchEffect;
    
    [SerializeField] private Toggle showTutorialToggle;
    [SerializeField] private Animator introAnimator;

    [SerializeField] private GameObject comboParent;
    [SerializeField] private Image comboFill;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private float comboFadeDuration = 1f;
    
    [SerializeField] private Animator anyAllChangedBannerAnimator;
    
    [Header("Transition Settings")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private float loadDelay = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource radioEnabledAudio;
    [SerializeField] private AudioSource radioDisabledAudio;
    [SerializeField] private AudioSource[] pausePlayOnPause;

    [Header("Checkpoint References")]
    [SerializeField] private Image checkpointFlag;
    [SerializeField] private Image gameOverCheckpointIndicatorWin;
    [SerializeField] private Image gameOverCheckpointIndicatorLose;
    [SerializeField] private Image settingsCheckpointIndicator;
    
    // Player Prefs
    const string showTutorialPlayerPref = "ShowTutorial";
    
    // Animation Constants
    private const string anyAllBannerOpenedParam = "show";
    
    // Coroutines
    private Coroutine pulseLowBatteryCoroutine;
    private Coroutine fadeComboCoroutine;
    private Coroutine hintAnimationCoroutine;
    
    // Internal Variables
    private bool lastShowComboState;
    private Dictionary<ClothingSet.ClothingType, Image> clothingTypeToHintImage;
    
    // Public Variables
    public bool enableRestartLevelFunctionality = true;

    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if(glitchEffect != null)
            glitchEffect.SetActive(false);

        if (showTutorialToggle != null)
        {
            showTutorialToggle.isOn = PlayerPrefs.GetInt(showTutorialPlayerPref, 1) == 1;
            showTutorialToggle.onValueChanged.AddListener(OnShowTutorialToggle);
        }

        if (!isMainMenu)
        {
            GameManager.Instance.OnTimeElapsed += UpdateBatteryLife;
            GameManager.Instance.OnComboTimeUpdated += OnComboTimeFill;
            PersonManager.Instance.OnBanListSet += OnBanListSet;
            GameManager.Instance.OnScoreUpdated += UpdateBadGuySymbols;
            
            comboParent.SetActive(false);

            if (LevelManager.Instance != null)
            {
                bool isFirstLevel = LevelManager.Instance.currentLevelIndex == 0;
                bool showTutorial = PlayerPrefs.GetInt(showTutorialPlayerPref, 1) == 1;
                bool showInstructions = isFirstLevel && showTutorial;
                instructionsSplashScreen.SetActive(showInstructions);
                HandlePause(showInstructions);
                introAnimator.gameObject.SetActive(isFirstLevel);
                
                // Set Any/All Changed Banner
                if (!isFirstLevel)
                {
                    var curLevelSettings = LevelManager.Instance.CurrentLevelSettings;
                    var prevLevelSettings = LevelManager.Instance.Levels[LevelManager.Instance.currentLevelIndex - 1];
                    if (curLevelSettings.badPersonMustHaveAll != prevLevelSettings.badPersonMustHaveAll)
                        SetAnyAllBanner();
                }
                
                levelText.text = "Level " + (LevelManager.Instance.currentLevelIndex + 1);
            }

            if (GameManager.Instance != null)
            {
                bool badPersonMustHaveAll = GameManager.Instance.BadPersonMustHaveAll;
                AnyText.SetActive(!badPersonMustHaveAll);
                AllText.SetActive(badPersonMustHaveAll);
            }
        }
        
        SetCheckpointFlag(LevelManager.Instance.CurrentLevelSettings.isCheckpoint);
        if (settingsCheckpointIndicator != null)
            settingsCheckpointIndicator.gameObject.SetActive(LevelManager.Instance.GetLastCheckpointLevelIndex() != -1);
    }
    
    private void HandlePause(bool paused)
    {
        if (paused)
        {
            Time.timeScale = 0f;
            GameObject.FindWithTag("FadeOut")?.SetActive(false);
            
            foreach (AudioSource audioSource in pausePlayOnPause)
                audioSource.Pause();
        }
        else
        {
            Time.timeScale = 1f;
            foreach (AudioSource audioSource in pausePlayOnPause)
                audioSource.UnPause();
        }
    }

    private void UpdateBatteryLife(float elapsedTime, float totalGameTime)
    {
        elapsedTime = Mathf.Clamp(elapsedTime, 0f, totalGameTime);
        float timeLeft = totalGameTime - elapsedTime;
        
        float batteryLifePercentage = 1f - elapsedTime / totalGameTime;

        string batteryLifeText = Utils.FormatTime(timeLeft);
        // if(elapsedTime >= totalGameTime)
        //     batteryLifeText = "Game Over";
        
        batteryLife.fillAmount = Mathf.Lerp(batteryLife.fillAmount, batteryLifePercentage, Time.deltaTime * 2f);
        betteryLifeText.text = batteryLifeText;
    }
    
    private void UpdateComboTimeFill(float comboTimeFill, float totalGameTime)
    {
        float batteryLifePercentage = comboTimeFill / totalGameTime;
        batteryLife.fillAmount = Mathf.Lerp(batteryLife.fillAmount, batteryLifePercentage, Time.deltaTime * 2f);
    }

    private void OnBanListSet(Dictionary<ClothingSet, int> bannedSpritesIndexes)
    {
        foreach (Transform child in banListParent)
            Destroy(child.gameObject);
        
        clothingTypeToHintImage = new Dictionary<ClothingSet.ClothingType, Image>();
        foreach (KeyValuePair<ClothingSet, int> bannedSpriteIndex in bannedSpritesIndexes)
        {
            ClothingSet.ClothingType clothingType = bannedSpriteIndex.Key.ClothingRendererTypes[0];
            
            GameObject banItem = Instantiate(banItemPrefab, banListParent);
            Image banItemImage = banItem.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            banItemImage.sprite = bannedSpriteIndex.Key.ClothingSprites[bannedSpriteIndex.Value].sprites[0];
            banItem.GetComponentInChildren<TextMeshProUGUI>().text = clothingType.ToAbbreviation();
            
            clothingTypeToHintImage.Add(clothingType, banItemImage);
        }
        
        // Recalcualte Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(banListParent.GetComponent<RectTransform>());
    }
    
    private IEnumerator DelayedFadeDestroyBadGuySymbol(Image badGuySymbol)
    {
        Color startingColor = badGuySymbol.color;
        Color targetColor = Color.red;
        targetColor.a = 0f;
        
        float elapsedTime = 0f;
        while (elapsedTime < badGuySymboleDeleteDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / badGuySymboleDeleteDuration;
            badGuySymbol.color = Color.Lerp(startingColor, targetColor, t);
            yield return null;
        }
        
        Destroy(badGuySymbol.gameObject);
    }

    private void UpdateBadGuySymbols(int score, int badGuyCount)
    {
        int badGuysLeft = badGuyCount - score;
        int numChildren = badGuySymbolsParent.childCount;
        int childrenDelta = badGuysLeft - numChildren;
        
        if(childrenDelta > 0)
        {
            for (int i = 0; i < childrenDelta; i++)
            {
                Image badGuySymbol = Instantiate(badGuySymbolPrefab, badGuySymbolsParent);
            }
        }
        else if(childrenDelta < 0)
        {
            int lastIndex = numChildren - 1;
            for (int i = lastIndex; i > lastIndex + childrenDelta; i--)
            {
                Image badGuySymbol = badGuySymbolsParent.GetChild(i).GetComponent<Image>();
                if(badGuySymbol.color.a < 1f)
                    continue;
                
                StartCoroutine(DelayedFadeDestroyBadGuySymbol(badGuySymbol));
            }
        }
    }

    public void OnGameOver(bool won)
    {
        gameOverPanel.SetActive(true);
        
        if (won)
        {
            winResultPanel.SetActive(true);
            loseResultPanel.SetActive(false);
            gameOverCheckpointIndicatorWin.gameObject.SetActive(LevelManager.Instance.CurrentLevelSettings.isCheckpoint);
        }
        else
        {
            winResultPanel.SetActive(false);
            loseResultPanel.SetActive(true);
            gameOverCheckpointIndicatorLose.gameObject.SetActive(LevelManager.Instance.GetLastCheckpointLevelIndex() != -1);
        }
        
        if(glitchEffect != null)
            glitchEffect.SetActive(!won);
    }
    
    private string FormatTime(float time)
    {
        return Mathf.FloorToInt(time / 60f) + "m " + Mathf.FloorToInt(time % 60f) + "s";
    }

    private string FormatScore(int correctGuesses, int wrongGuesses)
    {
        int totalGuesses = correctGuesses + wrongGuesses;
        return Mathf.RoundToInt((float)correctGuesses / totalGuesses * 100f) + "%";
    }
    
    public void GoToMainMenu()
    {
        HandlePause(false);
        TransitionManager.Instance().Transition((int) SceneBuildIndex.MainMenu, transitionSettings, loadDelay);
    }
    
    public void NextLevel()
    {
        if (!enableRestartLevelFunctionality)
            return;
        
        LoadLevel();
    }

    public void RestartLevel()
    {
        if (!enableRestartLevelFunctionality)
            return;
        
        LoadLevel();
    }
    
    private void LoadLevel()
    {
        HandlePause(false);
        TransitionManager.Instance().Transition((int) SceneBuildIndex.Game, transitionSettings, loadDelay);
    }
    
    public void RestartLevelSettingButton()
    {
        if (!enableRestartLevelFunctionality)
            return;

        HandlePause(false);

        LevelManager.Instance.ResetRunStats(true);
        if (0 < LevelManager.Instance.GetLastCheckpointLevelIndex())
            LevelManager.Instance.UpdateRunStats();

        TransitionManager.Instance().Transition((int) SceneBuildIndex.Game, transitionSettings, loadDelay);
    }

    public void ToggleSettingsMenu()
    {
        settingsMenu.SetActive(!settingsMenu.activeSelf);
        if (!instructionsSplashScreen.activeSelf)
            Time.timeScale = settingsMenu.activeSelf ? 0f : 1f;
        
        foreach (AudioSource audioSource in pausePlayOnPause)
        {
            if (audioSource == AudioManager.Instance.MainMusic)
            {
                audioSource.GetComponent<AudioLowPassFilter>().enabled = settingsMenu.activeSelf;
                continue;
            }
            
            if (settingsMenu.activeSelf)
                audioSource.Pause();
            else
                audioSource.UnPause();
        }
    }
    
    public void ToggleAllTutorialPanels()
    {
        foreach (GameObject tutorialPanel in tutorialPanels)
        {
            tutorialPanel.SetActive(!tutorialPanel.activeSelf);
        }
    }
    
    public void SetInstructionsSplashScreen(bool enabled)
    {
        instructionsSplashScreen.SetActive(enabled);
        HandlePause(enabled);
    }
    
    public void OnShowTutorialToggle(bool showTutorial)
    {
        // if (showTutorial)
        // {
        //     radioEnabledAudio.Play();
        // }
        // else
        // {
        //     radioDisabledAudio.Play();
        // }
        
        PlayerPrefs.SetInt(showTutorialPlayerPref, showTutorial ? 1 : 0);
        print(showTutorial);
    }
    
    public void PulseLowBatteryLife()
    {
        if (pulseLowBatteryCoroutine != null)
            return;

        pulseLowBatteryCoroutine = StartCoroutine(PulseLowBattery());
    }
    
    public void StopPulseLowBatteryLife()
    {
        if (pulseLowBatteryCoroutine != null)
            StopCoroutine(pulseLowBatteryCoroutine);
        
        lowBatteryLifeBackfill.color = new Color(lowBatteryLifeBackfill.color.r, lowBatteryLifeBackfill.color.g, lowBatteryLifeBackfill.color.b, 0f);
    }
    
    private IEnumerator PulseLowBattery()
    {
        float elapsedTime = 0f;
        Color c = lowBatteryLifeBackfill.color;
        while (elapsedTime < lowBatteryPulseLength)
        {
            elapsedTime += Time.deltaTime;
            float t = lowBatteryPulseCurve.Evaluate(elapsedTime);
            lowBatteryLifeBackfill.color = new Color(c.r, c.g, c.b, t);
            yield return null;
        }
        
        pulseLowBatteryCoroutine = null;
    }
    
    private void OnComboTimeFill(float timeFillT, int currentCorrectGuessCombo)
    {
        bool showCombo = 0 < currentCorrectGuessCombo;
        if (lastShowComboState != showCombo)
        {
            if(fadeComboCoroutine != null)
                StopCoroutine(fadeComboCoroutine);
            fadeComboCoroutine = StartCoroutine(FadeComboFill(showCombo));
            
        }
        lastShowComboState = showCombo;
        
        comboFill.fillAmount = timeFillT;
        comboText.text = "x " + currentCorrectGuessCombo + " Combo";
    }
    
    private IEnumerator FadeComboFill(bool fadeIn)
    {
        if (fadeIn)
            comboParent.SetActive(true);
        
        float elapsedTime = 0f;
        Color textColor = comboText.color;
        Color fillColor = comboFill.color;
        while (elapsedTime < comboFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / comboFadeDuration;
            if (fadeIn)
            {
                comboText.color = Color.Lerp(textColor, new Color(textColor.r, textColor.g, textColor.b, 1f), t);
                comboFill.color = Color.Lerp(fillColor, new Color(fillColor.r, fillColor.g, fillColor.b, 1f), t);
            }
            else
            {
                comboText.color = Color.Lerp(textColor, new Color(textColor.r, textColor.g, textColor.b, 0f), t);
                comboFill.color = Color.Lerp(fillColor, new Color(fillColor.r, fillColor.g, fillColor.b, 0f), t);
            }
            yield return null;
        }
        
        if(!fadeIn)
            comboParent.SetActive(false);
        
        fadeComboCoroutine = null;
    }

    public void SetCheckpointFlag(bool set)
    {
        if (checkpointFlag != null)
            checkpointFlag.gameObject.SetActive(set);
    }
    
    public void SetAnyAllBanner()
    {
        anyAllChangedBannerAnimator.SetTrigger(anyAllBannerOpenedParam);
    }
    
    public void ProcessPersonClicked(PersonVisualizer pv)
    {
        bool isAll = GameManager.Instance.BadPersonMustHaveAll;
        bool IsTarget = pv.GetComponent<Person>().IsTarget;
        
        LinkedList<(Image bgImage, Color targetColor)> results = new LinkedList<(Image bgImage, Color targetColor)>();
        foreach (KeyValuePair<ClothingSet.ClothingType, Image> hintImageData in clothingTypeToHintImage)
        {
            ClothingSet.ClothingType clothingType = hintImageData.Key;
            Image hintImage = hintImageData.Value;
            Sprite clothingSprite = pv.GetClothingSprite(clothingType).sprite;
            
            bool isCorrect = clothingSprite == hintImage.sprite;
            Image bgImage = hintImage.transform.parent.GetComponent<Image>();

            Color targetColor = Color.white;
            if (isCorrect)
                targetColor = Color.green;
            else if (!IsTarget)
                targetColor = Color.red;
            
            results.AddLast((bgImage, targetColor));
        }
        
        if (hintAnimationCoroutine != null)
            StopCoroutine(hintAnimationCoroutine);
        hintAnimationCoroutine = StartCoroutine(PersonClickedHintAnimation(results));
    }
    
    private IEnumerator PersonClickedHintAnimation(LinkedList<(Image bgImage, Color targetColor)> hintBgColorTargets)
    {
        float elapsedTime = 0f;
        float duration = 1f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            float t = elapsedTime / duration;
            foreach ((Image bgImage, Color targetColor) in hintBgColorTargets)
                bgImage.color = Color.Lerp(bgImage.color, targetColor, t);
            yield return null;
        }
        
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            float t = elapsedTime / duration;
            foreach ((Image bgImage, Color targetColor) in hintBgColorTargets)
                bgImage.color = Color.Lerp(bgImage.color, Color.white, t);
            yield return null;
        }
        
        hintAnimationCoroutine = null;
    }

    public void OnDestroy()
    {
        if(glitchEffect != null)
            glitchEffect.SetActive(false);
    }
}
