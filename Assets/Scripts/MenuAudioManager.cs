using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    public static bool active = false;
    private void Awake()
    {
        if (!active)
        {
            DontDestroyOnLoad(gameObject);
            active = true;
        }
        
        else
            gameObject.SetActive(false);
    }
}
