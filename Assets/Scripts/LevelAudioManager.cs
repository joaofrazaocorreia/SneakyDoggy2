using UnityEngine;

public class LevelAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource feedbackAudioSource;
    [SerializeField] private AudioClip UIbutton;
    [SerializeField] private AudioClip itemPickup;
    [SerializeField] private AudioClip levelWin;
    [SerializeField] private AudioClip levelLose;
    [SerializeField] private AudioClip timesUp;


    public void PlayFeedbackSound(AudioClip clip)
    {
        feedbackAudioSource.PlayOneShot(clip);
    }

    public void PlayUIButton()
    {
        feedbackAudioSource.pitch = Random.Range(0.9f, 1.1f);
        PlayFeedbackSound(UIbutton);
    }

    public void PlayItemPickup()
    {
        feedbackAudioSource.pitch = Random.Range(0.9f, 1.1f);
        PlayFeedbackSound(itemPickup);
    }

    public void PlayLevelWin()
    {
        feedbackAudioSource.pitch = 1f;
        PlayFeedbackSound(levelWin);
    }

    public void PlayLevelLose()
    {
        feedbackAudioSource.pitch = 1f;
        PlayFeedbackSound(levelLose);
    }

    public void PlayTimesUp()
    {
        feedbackAudioSource.pitch = 1f;
        PlayFeedbackSound(timesUp);
    }

    public void SetVolume(float volumePercent)
    {
        feedbackAudioSource.volume = Mathf.Clamp(volumePercent / 100f, 0f, 0.75f);
    }
}
