using UnityEngine;

public class LevelAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource feedbackAudioSource;
    [SerializeField] private AudioClip UIbutton;
    [SerializeField] private AudioClip itemPickup;
    [SerializeField] private AudioClip levelWin;
    [SerializeField] private AudioClip levelLose;
    [SerializeField] private AudioClip timesUp;


    // Plays a given sound from the level's feedback audioSource
    public void PlayFeedbackSound(AudioClip clip)
    {
        feedbackAudioSource.PlayOneShot(clip);
    }

    // Plays the UI buttons' sound from the level's feedback audioSource
    public void PlayUIButton()
    {
        feedbackAudioSource.pitch = Random.Range(0.9f, 1.1f);
        PlayFeedbackSound(UIbutton);
    }

    // Plays the item pickup sound from the level's feedback audioSource
    public void PlayItemPickup()
    {
        feedbackAudioSource.pitch = Random.Range(0.9f, 1.1f);
        PlayFeedbackSound(itemPickup);
    }

    // Plays the winning fanfare from the level's feedback audioSource
    public void PlayLevelWin()
    {
        feedbackAudioSource.pitch = 1f;
        PlayFeedbackSound(levelWin);
    }

    // Plays the losing fanfare from the level's feedback audioSource
    public void PlayLevelLose()
    {
        feedbackAudioSource.pitch = 1f;
        PlayFeedbackSound(levelLose);
    }

    // Plays the "time's up" fanfare sound from the level's feedback audioSource
    public void PlayTimesUp()
    {
        feedbackAudioSource.pitch = 1f;
        PlayFeedbackSound(timesUp);
    }

    // Changes the volume of the level's feedback audioSource
    public void SetVolume(float volumePercent)
    {
        feedbackAudioSource.volume = Mathf.Clamp(volumePercent / 100f, 0f, 0.75f);
    }
}
