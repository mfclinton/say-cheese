using System;
using EasyTransition;
using UnityEngine;

public class ClickableButton : MonoBehaviour, Clickable
{
    [Header("Settings")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private SceneBuildIndex sceneIndex = SceneBuildIndex.MainMenu;
    [SerializeField] private float loadDelay = 0.5f;
    [SerializeField] private AudioSource clickAudio;
        
    private bool isClickable = true;
    public bool IsClickable => isClickable;
    
    public void OnClick()
    {
        if(!isClickable)
            return;
        
        clickAudio.Play();
        TransitionManager.Instance().Transition((int) sceneIndex, transitionSettings, loadDelay);
        isClickable = false;
    }
}
