using UnityEngine;

public class OpenLinkManager : MonoBehaviour
{
    public void Open(string url)
    {
        Application.OpenURL(url);
    }
}
