using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameOverAnimationManager : MonoBehaviour
{
    [Header("Settings Cam Pan")]
    [SerializeField] private float camPanAnimationDuration = 1f;
    [SerializeField] private AnimationCurve camPanAnimationCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private float camPanZoomDuration = 1f;
    [SerializeField] private AnimationCurve camPanZoomCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    [Header("Eye Animation")]
    [SerializeField] private float eyeMovementDuration = 1f;
    [SerializeField] private AnimationCurve eyeMovementAnimationCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private Vector3 eyeGoalPosition = new Vector3(0, 0, 0);
    
    [Header("Death Animation")]
    [SerializeField] private Light2D deathLight;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private float deathAnimationDuration = 1f;
    [SerializeField] private AnimationCurve deathAnimationIntensityCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private AnimationCurve deathAnimationRangeCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private float deathLightGoalIntensity = 5f;
    [SerializeField] private float deathLightGoalRange = 10f;
    
    [SerializeField] private float deathAnimationDelay = 1f;
    [SerializeField] private AnimationCurve deathAnimationDelayCurve = AnimationCurve.EaseInOut(0,0,1,1);
    
    [Header("Computer Screen References")]
    [SerializeField] private Camera computerScreenCamera;
    [SerializeField] private RenderTexture computerScreenTexture;
    [SerializeField] private Canvas mainGameCanvas;
    
    [SerializeField] private float computerZoomDuration = 5f;
    [SerializeField] private AnimationCurve computerZoomCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private float computerZoomGoal = 20f;

    [SerializeField] private GameObject winScreenAnimationObject;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Sprite eyeButtonSprite;
    
    // Component References
    PlayerController playerController;
    Camera mainCamera;
    CameraSlider cameraSlider;
    EyeVisualizer eyeVisualizer;
    UIManager uiManager;
    GameManager gameManager;
    
    // Coroutines
    private Coroutine beatGameCoroutine;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
        mainCamera = Camera.main;
        cameraSlider = FindObjectOfType<CameraSlider>();
        eyeVisualizer = FindObjectOfType<EyeVisualizer>();
        uiManager = FindObjectOfType<UIManager>();
        
        gameManager = FindObjectOfType<GameManager>();
        gameManager.OnGameOver += OnGameOver;
        
        deathLight.gameObject.SetActive(false);
    }

    private IEnumerator BothCamPosAndZoomLerp()
    {
        Coroutine coroutine1 = StartCoroutine(LerpCameraToGameOverPosition());
        Coroutine coroutine2 = StartCoroutine(LerpCameraZoom());

        yield return coroutine1;
        yield return coroutine2;
    }

    
    private IEnumerator LerpCameraToGameOverPosition()
    {
        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = new Vector3(eyeVisualizer.transform.position.x, 0f, mainCamera.transform.position.z);
        
        float elapsedTime = 0;
        while (elapsedTime < camPanAnimationDuration)
        {
            float t = elapsedTime / camPanAnimationDuration;
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, camPanAnimationCurve.Evaluate(t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.position = endPos;
    }

    private IEnumerator LerpCameraZoom()
    {
        float startZoom = mainCamera.orthographicSize;
        float targetZoom = cameraSlider.ZoomLevels[0];
        
        float elapsedTime = 0;
        while (elapsedTime < camPanZoomDuration)
        {
            float t = elapsedTime / camPanZoomDuration;
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, targetZoom, camPanZoomCurve.Evaluate(t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.orthographicSize = targetZoom;
    }
    
    private IEnumerator LerpEyeToGameOverPosition()
    {
        Vector3 startPos = eyeVisualizer.transform.localPosition;
        
        float elapsedTime = 0;
        while (elapsedTime < eyeMovementDuration)
        {
            float t = elapsedTime / eyeMovementDuration;
            eyeVisualizer.transform.localPosition = Vector3.Lerp(startPos, eyeGoalPosition, eyeMovementAnimationCurve.Evaluate(t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        eyeVisualizer.transform.localPosition = eyeGoalPosition;
    }

    private IEnumerator DeathAnimation()
    {
        deathLight.gameObject.SetActive(true);
        
        float elapsedTime = 0;
        while (elapsedTime < deathAnimationDuration)
        {
            float t = elapsedTime / deathAnimationDuration;
            deathLight.intensity = Mathf.Lerp(0, deathLightGoalIntensity, deathAnimationIntensityCurve.Evaluate(t));
            deathLight.pointLightOuterRadius = Mathf.Lerp(0, deathLightGoalRange, deathAnimationRangeCurve.Evaluate(t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        deathLight.intensity = deathLightGoalIntensity;
        deathLight.pointLightOuterRadius = deathLightGoalRange;
        
        deathSound.Play();
        uiManager.OnGameOver(false);
        
        elapsedTime = 0;
        while (elapsedTime < deathAnimationDelay)
        {
            float t = elapsedTime / deathAnimationDuration;
            deathLight.intensity = Mathf.Lerp(deathLight.intensity, 0, deathAnimationDelayCurve.Evaluate(t));
            deathLight.pointLightOuterRadius = Mathf.Lerp(deathLight.pointLightOuterRadius, 0, deathAnimationDelayCurve.Evaluate(t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    private void DisableThings()
    {
        playerController.enabled = false;
        cameraSlider.enabled = false;
        eyeVisualizer.enabled = false;
        eyeVisualizer.LaserRenderer.enabled = false;
    }
    
    private IEnumerator FullLoseAnimation()
    {
        DisableThings();
        yield return BothCamPosAndZoomLerp();
        yield return LerpEyeToGameOverPosition();
        yield return DeathAnimation();
    }
    
    private IEnumerator ZoomComputerScreen()
    {
        float initialSize = computerScreenCamera.orthographicSize;
        
        float elapsedTime = 0;
        while (elapsedTime < computerZoomDuration)
        {
            float t = elapsedTime / computerZoomDuration;
            computerScreenCamera.orthographicSize = Mathf.Lerp(initialSize, computerZoomGoal, computerZoomCurve.Evaluate(t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        computerScreenCamera.orthographicSize = computerZoomGoal;
    }
    
    private void OnWin()
    {
        DisableThings();
        uiManager.OnGameOver(true);
    }

    public void OnBeatGame()
    {
        // Sets up Eye Icon
        nextLevelButton.image.sprite = eyeButtonSprite;
        UIManager.Instance.enableRestartLevelFunctionality = false;
        nextLevelButton.onClick.AddListener(TriggerBeatGameAnimation);
        
        // Sets up Home Button
        homeButton.gameObject.SetActive(false);
    }
    
    private void TriggerBeatGameAnimation()
    {
        if (beatGameCoroutine != null)
            return;
        
        // Triggers Audio
        AudioManager.Instance.PlayEndingSFX();
        
        beatGameCoroutine = StartCoroutine(BeatGameAnimation());
    }
    
    private IEnumerator BeatGameAnimation()
    {
        DisableThings();
        yield return BothCamPosAndZoomLerp();
        
        mainCamera.targetTexture = computerScreenTexture;
        computerScreenCamera.gameObject.SetActive(true);
        playerController.UsedCamera = computerScreenCamera;
        playerController.enabled = true;
        mainGameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        mainGameCanvas.worldCamera = mainCamera;

        // Disable canvas blocking raycasts
        mainGameCanvas.GetComponent<GraphicRaycaster>().enabled = false;
        
        Coroutine eyeMovementCoroutine = StartCoroutine(LerpEyeToGameOverPosition());
        
        winScreenAnimationObject.SetActive(true);
        yield return ZoomComputerScreen();
    }
    
    private void OnGameOver(bool isWin)
    {
        if (isWin)
            OnWin();
        else
            StartCoroutine(FullLoseAnimation());
    }
}
