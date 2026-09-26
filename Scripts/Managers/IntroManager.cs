using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }
}
