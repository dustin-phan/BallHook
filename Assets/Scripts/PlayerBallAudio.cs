using UnityEngine;

public class PlayerBallAudio : MonoBehaviour
{
    [Header("Audio Sources on this same object")]
    public AudioSource hookSource;
    public AudioSource detachSource;
    public AudioSource rollSource;

    [Header("Max actual volumes")]
    public float hookMaxVolume = 0.5f;
    public float detachMaxVolume = 0.5f;
    public float rollMaxVolume = .15f;

    public void SetSFXVolume(float sliderValue)
    {
        if (hookSource != null)
            hookSource.volume = sliderValue * hookMaxVolume;

        if (detachSource != null)
            detachSource.volume = sliderValue * detachMaxVolume;

        if (rollSource != null)
            rollSource.volume = sliderValue * rollMaxVolume;
    }
    public void PauseSFX()
    {
        if (hookSource != null && hookSource.isPlaying)
            hookSource.Pause();

        if (detachSource != null && detachSource.isPlaying)
            detachSource.Pause();

        if (rollSource != null && rollSource.isPlaying)
            rollSource.Pause();
    }
    public void ResumeSFX()
    {
        if (hookSource != null)
            hookSource.UnPause();

        if (detachSource != null)
            detachSource.UnPause();

        if (rollSource != null)
            rollSource.UnPause();

    }
    public void StopSFX()
    {
        if (hookSource != null)
            hookSource.Stop();

        if (detachSource != null)
            detachSource.Stop();

        if (rollSource != null)
            rollSource.Stop();
    }
}