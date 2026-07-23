using System;
using UnityEngine;
using UnityEngine.Audio;

public class PlaySoundSample : MonoBehaviour
{
    [SerializeField] private SoundConfig whooshSound;
    [SerializeField] private MusicConfig mainMenuMusic;


    private void Start()
    {
        mainMenuMusic.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            whooshSound.Play(transform);
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            whooshSound.Play(transform);
        }
    }
}
