using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    
    [Header("Settings")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private float defaultFadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private AudioClip currentClip;
    private float targetVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMusicSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeMusicSource()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.outputAudioMixerGroup = musicMixerGroup;
    }
    
    /// Joue une musique immédiatement sans fade
    public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip est null");
            return;
        }

        StopAllFades();

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = loop;
        musicSource.Play();

        currentClip = clip;
        targetVolume = volume;
    }
    
    /// Joue une musique avec fade in
    public void PlayMusicWithFade(AudioClip clip, float volume = 1f, float fadeDuration = -1f, bool loop = true)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip est null");
            return;
        }

        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

        StopAllFades();

        musicSource.clip = clip;
        musicSource.volume = 0f;
        musicSource.loop = loop;
        musicSource.Play();

        currentClip = clip;
        targetVolume = volume;

        fadeCoroutine = StartCoroutine(FadeVolume(0f, volume, fadeDuration));
    }


    /// Crossfade vers une nouvelle musique
    public void CrossfadeToMusic(AudioClip newClip, float volume = 1f, float fadeDuration = -1f, bool loop = true)
    {
        if (newClip == null)
        {
            Debug.LogWarning("AudioClip est null");
            return;
        }

        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

        // Si même clip, juste ajuster le volume
        if (currentClip == newClip)
        {
            FadeToVolume(volume, fadeDuration);
            return;
        }

        StopAllFades();
        fadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip, volume, fadeDuration, loop));
    }


    /// Fade out puis stop
    public void StopMusic(float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

        StopAllFades();
        fadeCoroutine = StartCoroutine(FadeOutAndStop(fadeDuration));
    }
    
    /// Stop immédiat sans fade
    public void StopMusicImmediate()
    {
        StopAllFades();
        musicSource.Stop();
        currentClip = null;
    }
    
    /// Pause la musique
    public void PauseMusic()
    {
        musicSource.Pause();
    }
    
    /// Reprend la musique en pause
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }
    
    /// Fade vers un nouveau volume sans changer de musique
    public void FadeToVolume(float newVolume, float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

        StopAllFades();
        targetVolume = newVolume;
        fadeCoroutine = StartCoroutine(FadeVolume(musicSource.volume, newVolume, fadeDuration));
    }
    
    /// Change le volume immédiatement
    public void SetVolume(float volume)
    {
        musicSource.volume = volume;
        targetVolume = volume;
    }
    
    /// Retourne si une musique est en train de jouer
    public bool IsPlaying()
    {
        return musicSource.isPlaying;
    }
    
    /// Retourne le clip actuellement joué
    public AudioClip GetCurrentClip()
    {
        return currentClip;
    }

    private void StopAllFades()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    private IEnumerator FadeVolume(float startVolume, float endVolume, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            musicSource.volume = Mathf.Lerp(startVolume, endVolume, t);
            yield return null;
        }

        musicSource.volume = endVolume;
    }

    private IEnumerator FadeOutAndStop(float duration)
    {
        float startVolume = musicSource.volume;
        yield return FadeVolume(startVolume, 0f, duration);
        musicSource.Stop();
        currentClip = null;
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip, float newVolume, float duration, bool loop)
    {
        float startVolume = musicSource.volume;
        float halfDuration = duration / 2f;

        // Fade out de la musique actuelle
        yield return FadeVolume(startVolume, 0f, halfDuration);

        // Changer de clip
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();
        currentClip = newClip;
        targetVolume = newVolume;

        // Fade in de la nouvelle musique
        yield return FadeVolume(0f, newVolume, halfDuration);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}