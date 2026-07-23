using UnityEngine;

[CreateAssetMenu(fileName = "NewMusicConfig", menuName = "Audio/Music Config")]
public class MusicConfig : ScriptableObject
{
    [Header("Music Track")]
    public AudioClip musicClip;

    [Header("Playback Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume de cette musique")]
    public float volume = 0.7f;

    [Tooltip("Boucler la musique automatiquement")]
    public bool loop = true;

    [Header("Fade Settings")]
    [Tooltip("Durée du fade in/out (0 = pas de fade)")]
    public float fadeDuration = 1f;
    
    /// Joue cette musique immédiatement
    public void Play()
    {
        if (!Validate()) return;

        if (fadeDuration > 0f)
        {
            MusicManager.Instance.PlayMusicWithFade(musicClip, volume, fadeDuration, loop);
        }
        else
        {
            MusicManager.Instance.PlayMusic(musicClip, volume, loop);
        }
    }
    
    /// Crossfade vers cette musique
    public void PlayWithCrossfade()
    {
        if (!Validate()) return;

        MusicManager.Instance.CrossfadeToMusic(musicClip, volume, fadeDuration, loop);
    }
    
    /// Stop la musique actuelle avec fade
    public static void Stop(float customFadeDuration = -1f)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic(customFadeDuration);
        }
    }

    private bool Validate()
    {
        if (musicClip == null)
        {
            Debug.LogWarning($"MusicConfig '{name}' n'a pas de clip assigné !");
            return false;
        }

        if (MusicManager.Instance == null)
        {
            Debug.LogError("MusicManager n'existe pas dans la scène !");
            return false;
        }

        return true;
    }
}