using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    public AudioSource bgmSource;
    public Slider bgmSlider;
    private float bgmMaxActualVolume = 0.03f;

    public Slider sfxSlider;
    public PlayerBallAudio playerBallAudio;

    private void Start()
    {
        bgmSource.ignoreListenerPause = true;
        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.value = 0.5f;
            SetBGMVolume(bgmSlider.value);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = 0.5f;
            SetSFXVolume(sfxSlider.value);
        }
    }

    public void SetBGMVolume(float sliderValue)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = sliderValue * bgmMaxActualVolume;
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (playerBallAudio != null)
        {
            playerBallAudio.SetSFXVolume(sliderValue);
        }
    }
}