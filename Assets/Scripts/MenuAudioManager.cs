using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    public static MenuAudioManager instance;
    private AudioSource audioSource;
    private void Awake()
    {
        // If this is the first instance of this class, it remains loaded in the scene permanently
        // (and any others after it get disabled)
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            audioSource = GetComponent<AudioSource>();
        }
        
        // As a static class, only one instance of this script can be active in the game at the same time
        else
            gameObject.SetActive(false);
    }

    // Changes the volume of the background music
    public static void SetVolume(float volumePercent)
    {
        instance.audioSource.volume = Mathf.Clamp(volumePercent / 900f, 0f, 1f);
    }
}
