using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class MainMenuPanel
{
    public Transform panelCamPos;
    public Sprite panelIconSprite;

    public SpriteRenderer[] fadeInOnFocus;
    public TextMeshPro[] fadeInOnFocusText;
}

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float introAnimationLightTime = 1f;
    [SerializeField] private float introAnimationUITime = 1f;
    
    [SerializeField] private float switchPanelTime = 1f;
    [SerializeField] private float fadeInSpriteSpeed = 1f;
    [SerializeField] private float fadeInTextSpeed = 1f;
    [SerializeField] private float fadeInArrowImagesSpeed = 1f;
    
    [SerializeField] private AnimationCurve easeInOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Data")]
    [SerializeField] private MainMenuPanel[] panels;
    
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image leftArrowImage;
    [SerializeField] private Image rightArrowImage;
    
    [SerializeField] private AudioSource leftArrowAudio;
    [SerializeField] private AudioSource rightArrowAudio;
    
    [Header("Blinking Animation")]
    [SerializeField] private Animator blinkingAnimator;
    [SerializeField] private Vector2 blinkingAnimationTimeRange = new Vector2(10f, 30f);
    private const string blinkingAnimationTrigger = "Blink";
    
    private Camera mainCamera;
    
    Coroutine introAnimationCoroutine;
    Coroutine lerpCamToPosCoroutine;
    Coroutine fadeSpritesCoroutine;
    Coroutine fadeTextsCoroutine;
    Coroutine fadeInArrowImagesCoroutine;
    
    private int currentPanelIndex = 1;
    
    private void Awake()
    {
        mainCamera = Camera.main;
    }
    
    private void Start()
    {
        introAnimationCoroutine = StartCoroutine(IntroAnimation());
        StartCoroutine(TriggerAnimationRandomly());
        
        // Resets LevelManager's run stats
        LevelManager.Instance.ResetRunStats();
    }
    
    private IEnumerator IntroAnimation()
    {
        Light[] lights = FindObjectsOfType<Light>();
        float[] initialLightIntensities = lights.Select(l => l.intensity).ToArray();
        float[] initialLightAngles = lights.Select(l => l.spotAngle).ToArray();
        
        void SetLightIntensity(float t)
        {
            for (int i = 0; i < lights.Length; i++)
                lights[i].intensity = initialLightIntensities[i] * t;
        }
        
        void SetLightRadius(float t)
        {
            for (int i = 0; i < lights.Length; i++)
                lights[i].spotAngle = initialLightAngles[i] * t;
        }
        
        void SetAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;
            foreach (MainMenuPanel panel in panels)
            {
                foreach (SpriteRenderer sr in panel.fadeInOnFocus)
                    sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
                
                foreach (TextMeshPro text in panel.fadeInOnFocusText)
                    text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            }
        }
        
        canvasGroup.blocksRaycasts = false;
        SetLightIntensity(0f);
        SetLightRadius(0f);
        SetAlpha(0f);
        
        float elapsedTime = 0f;
        while (elapsedTime < introAnimationLightTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime/introAnimationLightTime);
            SetLightIntensity(t);
            SetLightRadius(t);
            
            yield return null;
        }
        
        elapsedTime = 0f;
        while (elapsedTime < introAnimationUITime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime/introAnimationUITime);
            SetAlpha(alpha);
            
            yield return null;
        }
        
        SwitchPanel(currentPanelIndex);
        canvasGroup.blocksRaycasts = true;
    }
    
    public void OnLeftArrowClicked()
    {
        leftArrowAudio.Play();
        SwitchPanel(currentPanelIndex - 1);
    }
    
    public void OnRightArrowClicked()
    {
        rightArrowAudio.Play();
        SwitchPanel(currentPanelIndex + 1);
    }
    
    private void SwitchPanel(int index)
    {
        if (index < 0 || index >= panels.Length)
            return;
        
        MainMenuPanel currentPanel = panels[currentPanelIndex];
        MainMenuPanel nextPanel = panels[index];
        
        bool isLeftEdge = index == 0;
        bool isRightEdge = index == panels.Length - 1;
        leftArrowImage.transform.parent.gameObject.SetActive(!isLeftEdge);
        rightArrowImage.transform.parent.gameObject.SetActive(!isRightEdge);
        
        if(fadeSpritesCoroutine != null)
            StopCoroutine(fadeSpritesCoroutine);
        fadeSpritesCoroutine = StartCoroutine(FadeSprites(false, currentPanel.fadeInOnFocus));
        
        if(fadeTextsCoroutine != null)
            StopCoroutine(fadeTextsCoroutine);
        fadeTextsCoroutine = StartCoroutine(FadeTexts(false, currentPanel.fadeInOnFocusText));
        
        if(fadeInArrowImagesCoroutine != null)
            StopCoroutine(fadeInArrowImagesCoroutine);
        fadeInArrowImagesCoroutine = StartCoroutine(FadeArrowImages(false));
        
        Vector3 endPos = nextPanel.panelCamPos.position;
        if(lerpCamToPosCoroutine != null)
            StopCoroutine(lerpCamToPosCoroutine);
        lerpCamToPosCoroutine = StartCoroutine(LerpCamToPos(endPos));
        
        currentPanelIndex = index;
    }

    private void OnPanelSettled()
    {
        bool isLeftEdge = currentPanelIndex == 0;
        bool isRightEdge = currentPanelIndex == panels.Length - 1;
        Debug.Log($"Panel Settled: {currentPanelIndex}, Left Edge: {isLeftEdge}, Right Edge: {isRightEdge}");
        
        Sprite newLeftArrowSprite = currentPanelIndex == 0 ? null : panels[currentPanelIndex - 1].panelIconSprite;
        Sprite newRightArrowSprite = currentPanelIndex == panels.Length - 1 ? null : panels[currentPanelIndex + 1].panelIconSprite;
        leftArrowImage.sprite = newLeftArrowSprite;
        rightArrowImage.sprite = newRightArrowSprite;
        
        if(fadeSpritesCoroutine != null)
            StopCoroutine(fadeSpritesCoroutine);
        fadeSpritesCoroutine = StartCoroutine(FadeSprites(true, panels[currentPanelIndex].fadeInOnFocus));
        
        if(fadeTextsCoroutine != null)
            StopCoroutine(fadeTextsCoroutine);
        fadeTextsCoroutine = StartCoroutine(FadeTexts(true, panels[currentPanelIndex].fadeInOnFocusText));
        
        if(fadeInArrowImagesCoroutine != null)
            StopCoroutine(fadeInArrowImagesCoroutine);
        fadeInArrowImagesCoroutine = StartCoroutine(FadeArrowImages(true, isLeftEdge, isRightEdge));
    }
    
    private IEnumerator LerpCamToPos(Vector3 endPos)
    {
        Vector3 startPos = mainCamera.transform.position;
        
        float elapsedTime = 0f;
        while (elapsedTime < switchPanelTime && mainCamera.transform.position != endPos)
        {
            elapsedTime += Time.deltaTime;
            float lerpFraction = elapsedTime / switchPanelTime;
            float smoothFraction = easeInOutCurve.Evaluate(lerpFraction);
        
            Vector3 newPos = Vector3.Lerp(startPos, endPos, smoothFraction);
            mainCamera.transform.position = newPos;
        
            yield return null;
        }
    
        mainCamera.transform.position = endPos;
        OnPanelSettled();
    }
    
    private IEnumerator FadeSprites(bool fadeIn, SpriteRenderer[] fadeInOnFocus)
    {
        if(fadeInOnFocus == null || fadeInOnFocus.Length == 0)
            yield break;
        
        Color spriteColor = fadeInOnFocus[0].color;
        float targetAlpha = fadeIn ? 1 : 0;
        
        while (Mathf.Abs(spriteColor.a - targetAlpha) > 0.01f)
        {
            spriteColor.a = Mathf.Lerp(spriteColor.a, targetAlpha, Time.deltaTime * fadeInSpriteSpeed);
            foreach(SpriteRenderer sr in fadeInOnFocus)
                sr.color = spriteColor;
            
            yield return null;
        }
        
        spriteColor.a = targetAlpha;
        foreach(SpriteRenderer sr in fadeInOnFocus)
            sr.color = spriteColor;
    }
    
    private IEnumerator FadeTexts(bool fadeIn, TextMeshPro[] fadeInOnFocusText)
    {
        if(fadeInOnFocusText == null || fadeInOnFocusText.Length == 0)
            yield break;
        
        Color textColor = fadeInOnFocusText[0].color;
        float targetAlpha = fadeIn ? 1 : 0;
        
        while (Mathf.Abs(textColor.a - targetAlpha) > 0.01f)
        {
            textColor.a = Mathf.Lerp(textColor.a, targetAlpha, Time.deltaTime * fadeInTextSpeed);
            foreach(TextMeshPro text in fadeInOnFocusText)
                text.color = textColor;
            
            yield return null;
        }
        
        textColor.a = targetAlpha;
        foreach(TextMeshPro text in fadeInOnFocusText)
            text.color = textColor;
    }
    
    private IEnumerator FadeArrowImages(bool fadeIn, bool ignoreLeft = false, bool ignoreRight = false)
    {
        
        Color leftArrowColor = leftArrowImage.color;
        Color rightArrowColor = rightArrowImage.color;

        float targetAlpha = fadeIn ? 1 : 0;
        
        bool StillInterpolating()
        {
            bool result = false;
            if (!ignoreLeft)
                result |= Mathf.Abs(leftArrowColor.a - targetAlpha) > 0.01f;
            if (!ignoreRight)
                result |= Mathf.Abs(rightArrowColor.a - targetAlpha) > 0.01f;
            return result;
        }
        
        while (StillInterpolating())
        {
            leftArrowColor.a = Mathf.Lerp(leftArrowColor.a, targetAlpha, Time.deltaTime * fadeInArrowImagesSpeed);
            rightArrowColor.a = Mathf.Lerp(rightArrowColor.a, targetAlpha, Time.deltaTime * fadeInArrowImagesSpeed);

            if (!ignoreLeft)
                leftArrowImage.color = leftArrowColor;
            if (!ignoreRight)
                rightArrowImage.color = rightArrowColor;
            
            yield return null;
        }
        
        leftArrowColor.a = targetAlpha;
        rightArrowColor.a = targetAlpha;
        
        if (!ignoreLeft)
            leftArrowImage.color = leftArrowColor;
        if (!ignoreRight)
            rightArrowImage.color = rightArrowColor;
    }
    
    private IEnumerator TriggerAnimationRandomly()
    {
        while (true) // Infinite loop to continuously trigger animations at random intervals
        {
            yield return new WaitForSeconds(Random.Range(blinkingAnimationTimeRange.x, blinkingAnimationTimeRange.y));
            blinkingAnimator.SetTrigger(blinkingAnimationTrigger);
        }
    }
}
