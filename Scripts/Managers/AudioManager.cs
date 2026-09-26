using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource mainMusic;
    [SerializeField] private AudioSource lowCameraBattery;
    [SerializeField] private AudioSource cameraPan;
    [SerializeField] private AudioSource pointGained;
    [SerializeField] private AudioSource gameOverLose;
    [SerializeField] private AudioSource gameOverWon;
    [SerializeField] private AudioSource badCapture;
    [SerializeField] private AudioSource laserFire;

    [SerializeField] private AudioSource zoomInSFX;
    [SerializeField] private AudioSource zoomOutSFX;
    
    [SerializeField] private AudioSource endingSFX;
    
    [SerializeField] private AudioSource[] disableOnGameOver;
    
    [SerializeField] private AudioClip[] pointGainedClips;

    
    [Header("Settings")]
    [SerializeField] private float musicFadeTime = 1f;
    
    int pointGainedIndex = 0;
    
    // Internal References
    private Coroutine fadeMusicCoroutine;
    
    // Getters
    public AudioSource MainMusic => mainMusic;
    
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        
        MapData mapData = MapManager.Instance.GetCurrentMap();
        if (mapData != null)
        {
            mainMusic.clip = mapData.backgroundMusic;
            mainMusic.volume = 0f;
            mainMusic.Play();
            PlayFadeInMusic(true);

            if (LevelManager.Instance.currentLevelIndex == 0 || LevelManager.Instance.CurrentLevelSettings.isCheckpoint)
            {
                mainMusic.time = 0f;
                LevelManager.Instance.CurrentMusicStartTime = 0f;
            }
            else
            {
                mainMusic.time = LevelManager.Instance.CurrentMusicStartTime;
            }
        }
    }

    private void Start()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        pc.OnMove += OnMovingCamera;
        pc.OnZoom += OnZoom;
        
        GameManager.Instance.OnScoreUpdated += (a, b) => OnPointGained();
        GameManager.Instance.OnGameOver += OnGameOver;
        GameManager.Instance.OnPersonClicked += OnLaserFire;
    }

    public bool PlayLowBatterySound()
    {
        if(lowCameraBattery.isPlaying)
            return false;

        lowCameraBattery.Play();
        return true;
    }
    
    public void OnMovingCamera(Vector2 mov)
    {
        if (mov.magnitude < Mathf.Epsilon)
            cameraPan.Stop();
        else
            cameraPan.Play();
    }
    
    public void OnPointGained()
    {
        AudioClip sampledClip = pointGainedClips[pointGainedIndex++ % pointGainedClips.Length];
        pointGained.clip = sampledClip;
        pointGained.Play();
    }
    
    public void OnGameOver(bool won)
    {
        if (!won)
        {
            gameOverLose.gameObject.SetActive(true);
        }
        else
        {
            gameOverWon.gameObject.SetActive(true);
        }
        
        LevelManager.Instance.CurrentMusicStartTime = mainMusic.time;
        PlayFadeInMusic(false);
        
        foreach (var audioSource in disableOnGameOver)
            audioSource.Stop();
    }

    public void StopLowBatterySound()
    {
        lowCameraBattery.Stop();
    }
    
    public void OnBadCapture()
    {
        badCapture.Play();
    }
    
    public void OnComboCapture(int combo)
    {
        
    }
    
    public void OnLaserFire()
    {
        laserFire.Play();
    }
    
    public void PlayFadeInMusic(bool fadeIn)
    {
        if (fadeMusicCoroutine != null)
            StopCoroutine(fadeMusicCoroutine);
        
        fadeMusicCoroutine = StartCoroutine(FadeMusic(fadeIn));
    }
    
    private IEnumerator FadeMusic(bool fadeIn)
    {
        float startVolume = mainMusic.volume;
        float targetVolume = fadeIn ? 1f : 0f;
        
        float currentTime = 0f;
        while (currentTime < musicFadeTime)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, currentTime / musicFadeTime);
            mainMusic.volume = newVolume;
            yield return null;
        }
        
        mainMusic.volume = targetVolume;
    }
    
    public void OnZoom(float zoom)
    {
        if (zoom > 0)
        {
            zoomInSFX.Play();
        }
        else
        {
            zoomOutSFX.Play();
        }
    }
    
    public void PlayEndingSFX()
    {
        endingSFX.Play();
    }
}
