 using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioVolumeHandler : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        // Charger les valeurs linéaires
        float masterValue = PlayerPrefs.GetFloat("masterVolume", 1f);
        float musicValue = PlayerPrefs.GetFloat("musicVolume", 1f);
        float sfxValue = PlayerPrefs.GetFloat("soundFXVolume", 1f);

        // Appliquer au mixer (converti en dB)
        audioMixer.SetFloat("masterVolume", LinearToDb(masterValue));
        audioMixer.SetFloat("musicVolume", LinearToDb(musicValue));
        audioMixer.SetFloat("soundFXVolume", LinearToDb(sfxValue));

        // Mettre à jour les sliders
        masterVolumeSlider.value = masterValue;
        musicVolumeSlider.value = musicValue;
        sfxVolumeSlider.value = sfxValue;
    }

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", LinearToDb(level));
        PlayerPrefs.SetFloat("masterVolume", level);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", LinearToDb(level));
        PlayerPrefs.SetFloat("musicVolume", level);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float level)
    {
        audioMixer.SetFloat("soundFXVolume", LinearToDb(level));
        PlayerPrefs.SetFloat("soundFXVolume", level);
        PlayerPrefs.Save();
    }

    private float LinearToDb(float linear)
    {
        // Empêcher log(0)
        return linear > 0f ? Mathf.Log10(linear) * 20f : -80f;
    }
}