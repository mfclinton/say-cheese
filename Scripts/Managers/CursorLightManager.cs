using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CursorLightManager : MonoBehaviour
{
    [SerializeField] private Light2D cursorLight;
    [SerializeField] private float maxIntensity = 1f;
    [SerializeField] private float fadeOutTime = 1f;
    
    public Light2D CursorLight => cursorLight;

    private PlayerController playerController;
    
    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.OnGameOver += FadeOutCursorLight;

        cursorLight.intensity = 0f;
        StartCoroutine(FadeCursorLightCoroutine(false));
    }
    
    private void Update()
    {
        UpdateCursorLightPosition();
    }
    
    private void UpdateCursorLightPosition()
    {
        cursorLight.transform.position = playerController.PointerWorldPosition;
    }
    
    private void FadeOutCursorLight(bool win)
    {
        StartCoroutine(FadeCursorLightCoroutine(true));
    }

    private IEnumerator FadeCursorLightCoroutine(bool fadeOut)
    {
        float elapsedTime = 0f;
        float startIntensity = fadeOut ? cursorLight.intensity : 0f;
        float endIntensity = fadeOut ? 0f : maxIntensity;
        
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutTime;
            cursorLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);
            yield return null;
        }
    }

    public void OnEnable()
    {
        cursorLight.enabled = true;
    }
    
    public void OnDisable()
    {
        cursorLight.enabled = false;
    }
}
