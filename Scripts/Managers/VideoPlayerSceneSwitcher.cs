using UnityEngine;
using UnityEngine.Video;
using EasyTransition;
using UnityEngine.SceneManagement;

public class VideoPlayerSceneSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string videoFileName;
    
    [Header("Settings")]
    [SerializeField] private TransitionSettings transitionSettings;
    [SerializeField] private SceneBuildIndex sceneIndex = SceneBuildIndex.MainMenu;
    [SerializeField] private float loadDelay = 0f;

    void Awake()
    {
        if (videoPlayer != null)
        {
            // Load Video
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            videoPlayer.url = videoPath;
            videoPlayer.Play();
            
            // Listen for Video Finished
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        TransitionManager.Instance().Transition((int) sceneIndex, transitionSettings, loadDelay);
    }
}