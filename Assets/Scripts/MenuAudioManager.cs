using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
    public static MenuAudioManager instance;
    private AudioSource audioSource;
    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            audioSource = GetComponent<AudioSource>();
        }
        
        else
            gameObject.SetActive(false);
    }

    public static void SetVolume(float volumePercent)
    {
        instance.audioSource.volume = Mathf.Clamp(volumePercent / 900f, 0f, 1f);
    }
}
