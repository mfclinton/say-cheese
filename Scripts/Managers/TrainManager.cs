using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private AudioSource trainSound;
    
    public void PlayTrainSound()
    {
        trainSound.Play();
    }
}
