using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Pooling")]
    [SerializeField] private AudioSource soundFxPrefab;
    [SerializeField] private int poolSize = 10;

    private Queue<AudioSource> audioSourcePool;
    private List<AudioSourceTimer> activeTimers;

    // Progressive pitch : compteur de lectures par SoundConfig
    private readonly Dictionary<SoundConfig, int> pitchCounters = new();

    // Groupe : nombre de sons actifs par groupe + file d'attente
    private readonly Dictionary<string, int> activeGroupCount = new();
    private readonly Dictionary<string, Queue<PendingGroupSound>> pendingGroupSounds = new();

    // ─────────────────────────────────────────
    // Inner classes
    // ─────────────────────────────────────────

    private class AudioSourceTimer
    {
        public AudioSource source;
        public float endTime;
        public float originalPitch;
        public string group; // null si pas de groupe

        public AudioSourceTimer(AudioSource source, float duration, string group = null)
        {
            this.source    = source;
            this.endTime   = Time.time + duration;
            this.originalPitch = source.pitch;
            this.group     = group;
        }
    }

    private class PendingGroupSound
    {
        public AudioClip       clip;
        public Vector3         position;
        public float           volume;
        public float           pitch;
        public AudioMixerGroup mixerGroup;
        public string          group;
    }

    // ─────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        audioSourcePool = new Queue<AudioSource>();
        activeTimers    = new List<AudioSourceTimer>();

        for (int i = 0; i < poolSize; i++)
            CreateNewAudioSource();
    }

    private AudioSource CreateNewAudioSource()
    {
        AudioSource src = Instantiate(soundFxPrefab, transform);
        src.gameObject.SetActive(false);
        audioSourcePool.Enqueue(src);
        return src;
    }

    private AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count == 0)
        {
            Debug.LogWarning("SoundManager : pool vide, création d'une nouvelle AudioSource.");
            CreateNewAudioSource();
        }

        AudioSource src = audioSourcePool.Dequeue();
        src.gameObject.SetActive(true);
        return src;
    }

    private void ReturnToPool(AudioSource source, float originalPitch)
    {
        source.Stop();
        source.clip  = null;
        source.pitch = originalPitch;
        source.gameObject.SetActive(false);
        audioSourcePool.Enqueue(source);
    }

    private void Update()
    {
        float now = Time.time;

        for (int i = activeTimers.Count - 1; i >= 0; i--)
        {
            AudioSourceTimer timer = activeTimers[i];

            if (now < timer.endTime) continue;

            string group = timer.group;
            ReturnToPool(timer.source, timer.originalPitch);
            activeTimers.RemoveAt(i);

            if (string.IsNullOrEmpty(group)) continue;

            // Libère une place dans le groupe
            activeGroupCount[group] = Mathf.Max(0, GetGroupCount(group) - 1);

            // Si le groupe est libre, joue le prochain son en attente
            if (activeGroupCount[group] == 0
                && pendingGroupSounds.TryGetValue(group, out Queue<PendingGroupSound> queue)
                && queue.Count > 0)
            {
                PendingGroupSound next = queue.Dequeue();
                PlaySoundInternal(next.clip, next.position, next.volume, next.pitch, next.mixerGroup, next.group);
            }
        }
    }

    // ─────────────────────────────────────────
    // Point d'entrée principal (SoundConfig)
    // ─────────────────────────────────────────

    /// <summary>Joue un SoundConfig en tenant compte de toutes ses options.</summary>
    public void Play(SoundConfig config, Vector3 position)
    {
        if (config == null)
        {
            Debug.LogWarning("SoundManager.Play : SoundConfig est null.");
            return;
        }

        AudioClip clip = config.GetRandomClip();
        if (clip == null) return;

        float  pitch = ResolvePitch(config);
        string group = string.IsNullOrEmpty(config.soundGroup) ? null : config.soundGroup;

        // Si le groupe est occupé → mise en attente
        if (group != null && GetGroupCount(group) > 0)
        {
            if (!pendingGroupSounds.ContainsKey(group))
                pendingGroupSounds[group] = new Queue<PendingGroupSound>();

            pendingGroupSounds[group].Enqueue(new PendingGroupSound
            {
                clip       = clip,
                position   = position,
                volume     = config.volume,
                pitch      = pitch,
                mixerGroup = config.mixerGroup,
                group      = group
            });
            return;
        }

        PlaySoundInternal(clip, position, config.volume, pitch, config.mixerGroup, group);
    }

    public void Play(SoundConfig config, Transform transform)
        => Play(config, transform.position);

    // ─────────────────────────────────────────
    // Progressive Pitch
    // ─────────────────────────────────────────

    private float ResolvePitch(SoundConfig config)
    {
        if (config.progressivePitch)  return GetProgressivePitch(config);
        if (config.randomizePitch)    return Random.Range(config.minPitch, config.maxPitch);
        return 1f;
    }

    private float GetProgressivePitch(SoundConfig config)
    {
        if (!pitchCounters.TryGetValue(config, out int count))
            count = 0;

        // Interpole basePitch → progressiveMaxPitch sur (iterationsToMax - 1) incréments
        int   steps = Mathf.Max(1, config.iterationsToMax - 1);
        float t     = Mathf.Clamp01((float)count / steps);
        float pitch = Mathf.Lerp(config.basePitch, config.progressiveMaxPitch, t);

        // Avance le compteur (bloqué au max)
        pitchCounters[config] = Mathf.Min(count + 1, steps);

        return pitch;
    }

    /// <summary>Remet le compteur de pitch progressif à zéro pour ce SoundConfig.</summary>
    public void ResetPitchCounter(SoundConfig config)
    {
        if (config != null)
            pitchCounters[config] = 0;
    }

    /// <summary>Retourne le nombre de lectures actuelles pour le pitch progressif.</summary>
    public int GetPitchCount(SoundConfig config)
        => config != null && pitchCounters.TryGetValue(config, out int c) ? c : 0;

    // ─────────────────────────────────────────
    // Lecture interne
    // ─────────────────────────────────────────

    private void PlaySoundInternal(AudioClip clip, Vector3 position, float volume,
        float pitch, AudioMixerGroup mixerGroup, string group)
    {
        AudioSource src = GetAudioSource();
        src.transform.position   = position;
        src.clip                 = clip;
        src.volume               = volume;
        src.pitch                = pitch;
        src.outputAudioMixerGroup = mixerGroup;
        src.Play();

        float duration = clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
        activeTimers.Add(new AudioSourceTimer(src, duration, group));

        if (!string.IsNullOrEmpty(group))
        {
            if (!activeGroupCount.ContainsKey(group))
                activeGroupCount[group] = 0;
            activeGroupCount[group]++;
        }
    }

    // ─────────────────────────────────────────
    // API legacy (rétrocompatibilité)
    // ─────────────────────────────────────────

    public void PlaySoundFX(AudioClip clip, Vector3 position, float volume = 1f, AudioMixerGroup mixerGroup = null)
    {
        if (clip == null) { Debug.LogWarning("AudioClip est null"); return; }
        PlaySoundInternal(clip, position, volume, 1f, mixerGroup, null);
    }

    public void PlaySoundFX(AudioClip clip, Transform spawnTransform, float volume = 1f, AudioMixerGroup mixerGroup = null)
        => PlaySoundFX(clip, spawnTransform.position, volume, mixerGroup);

    public void PlayRandomSoundFX(AudioClip[] clips, Vector3 position, float volume = 1f, AudioMixerGroup mixerGroup = null)
    {
        if (clips == null || clips.Length == 0) { Debug.LogWarning("Tableau de clips vide ou null"); return; }
        PlaySoundFX(clips[Random.Range(0, clips.Length)], position, volume, mixerGroup);
    }

    public void PlayRandomSoundFX(AudioClip[] clips, Transform spawnTransform, float volume = 1f, AudioMixerGroup mixerGroup = null)
        => PlayRandomSoundFX(clips, spawnTransform.position, volume, mixerGroup);

    public void PlaySoundFXWithPitch(AudioClip clip, Vector3 position, float volume = 1f,
        float minPitch = 0.9f, float maxPitch = 1.1f, AudioMixerGroup mixerGroup = null)
    {
        if (clip == null) { Debug.LogWarning("AudioClip est null"); return; }
        PlaySoundInternal(clip, position, volume, Random.Range(minPitch, maxPitch), mixerGroup, null);
    }

    public void PlaySoundFXWithPitch(AudioClip clip, Transform spawnTransform, float volume = 1f,
        float minPitch = 0.9f, float maxPitch = 1.1f, AudioMixerGroup mixerGroup = null)
        => PlaySoundFXWithPitch(clip, spawnTransform.position, volume, minPitch, maxPitch, mixerGroup);

    // ─────────────────────────────────────────
    // Utilitaires groupes
    // ─────────────────────────────────────────

    /// <summary>Vide la file d'attente d'un groupe sans stopper les sons actifs.</summary>
    public void ClearGroupQueue(string group)
    {
        if (pendingGroupSounds.TryGetValue(group, out var q))
            q.Clear();
    }

    /// <summary>Stoppe immédiatement tous les sons actifs d'un groupe ET vide sa file.</summary>
    public void StopGroup(string group)
    {
        ClearGroupQueue(group);

        for (int i = activeTimers.Count - 1; i >= 0; i--)
        {
            if (activeTimers[i].group == group)
            {
                ReturnToPool(activeTimers[i].source, activeTimers[i].originalPitch);
                activeTimers.RemoveAt(i);
            }
        }

        activeGroupCount[group] = 0;
    }

    /// <summary>Retourne le nombre de sons en attente pour un groupe.</summary>
    public int GetPendingGroupCount(string group)
        => pendingGroupSounds.TryGetValue(group, out var q) ? q.Count : 0;

    // ─────────────────────────────────────────
    // Utilitaires généraux
    // ─────────────────────────────────────────

    /// <summary>Stoppe tous les sons et vide toutes les files d'attente.</summary>
    public void StopAllSounds()
    {
        foreach (var timer in activeTimers)
            ReturnToPool(timer.source, timer.originalPitch);

        activeTimers.Clear();
        activeGroupCount.Clear();
        pendingGroupSounds.Clear();
    }

    public int GetActiveSoundCount() => activeTimers.Count;

    private int GetGroupCount(string group)
        => activeGroupCount.TryGetValue(group, out int c) ? c : 0;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}