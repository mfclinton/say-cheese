using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Image[] volumeImages;
    [SerializeField] private string VolumePrefKey;
    
    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 0.5f);
        volumeSlider.value = savedVolume;

        HandleSliderValueChanged(savedVolume); // Update mixer and images based on saved volume
    }
    
    private void HandleSliderValueChanged(float value)
    {
        audioMixer.SetFloat(VolumePrefKey, Mathf.Log10(value + Mathf.Epsilon) * 20);
        UpdateVolumeImages(value);

        PlayerPrefs.SetFloat(VolumePrefKey, value);
    }

    private void UpdateVolumeImages(float volume)
    {
        // Update visibility of images based on volume thresholds
        volumeImages[0].enabled = volume == 0;
        volumeImages[1].enabled = volume > 0;
        volumeImages[2].enabled = volume >= 0.33f;
        volumeImages[3].enabled = volume >= 0.66f;
    }

    void OnEnable()
    {
        volumeSlider.onValueChanged.AddListener(HandleSliderValueChanged);
    }

    void OnDisable()
    {
        volumeSlider.onValueChanged.RemoveListener(HandleSliderValueChanged);
    }
}