using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSoundConfig", menuName = "Audio/Sound Config")]
public class SoundConfig : ScriptableObject
{
    [Header("Audio Clips")]
    [Tooltip("Un seul clip pour son simple, plusieurs pour variation aléatoire")]
    public AudioClip[] clips;

    [Header("Playback Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume du son (0 = silencieux, 1 = max)")]
    public float volume = 1f;

    [Tooltip("Groupe du mixer audio (laissez vide pour Master)")]
    public AudioMixerGroup mixerGroup;

    // ─────────────────────────────────────────
    // Pitch aléatoire
    // ─────────────────────────────────────────

    [Header("Random Pitch")]
    [Tooltip("Active une variation aléatoire du pitch à chaque lecture.\nIncompatible avec Progressive Pitch (Progressive a la priorité).")]
    public bool randomizePitch = false;

    [Range(0.5f, 1.5f)]
    public float minPitch = 0.9f;

    [Range(0.5f, 1.5f)]
    public float maxPitch = 1.1f;

    // ─────────────────────────────────────────
    // Pitch progressif
    // ─────────────────────────────────────────

    [Header("Progressive Pitch")]
    [Tooltip("Le pitch monte progressivement à chaque lecture jusqu'au maximum défini.")]
    public bool progressivePitch = false;

    [Range(0.1f, 3f)]
    [Tooltip("Pitch de départ (première lecture)")]
    public float basePitch = 1f;

    [Range(0.1f, 3f)]
    [Tooltip("Pitch maximum atteint après 'Iterations To Max' lectures")]
    public float progressiveMaxPitch = 2f;

    [Min(2)]
    [Tooltip("Nombre de lectures pour passer de basePitch à progressiveMaxPitch.\n" +
             "Ex : 5 → lecture 1 = basePitch, lecture 5 = maxPitch, lecture 6+ = maxPitch.\n" +
             "Appelez ResetPitchCounter() pour repartir de zéro.")]
    public int iterationsToMax = 5;

    // ─────────────────────────────────────────
    // Sound Group
    // ─────────────────────────────────────────

    [Header("Sound Group")]
    [Tooltip("Nom du groupe d'exclusivité.\n" +
             "Si un son du même groupe est déjà en cours, ce son est mis en file d'attente\n" +
             "et joué automatiquement quand le groupe se libère.\n" +
             "Laissez vide pour ne pas utiliser de groupe.")]
    public string soundGroup = "";

    // ─────────────────────────────────────────
    // Spatial
    // ─────────────────────────────────────────

    [Header("Spatial Settings")]
    [Tooltip("Si false, le son sera en 2D (pas de spatialisation)")]
    public bool is3D = true;

    // ─────────────────────────────────────────
    // API publique
    // ─────────────────────────────────────────

    /// <summary>Joue le son à la position donnée.</summary>
    public void Play(Vector3 position)
    {
        if (!Validate()) return;
        SoundManager.Instance.Play(this, position);
    }

    /// <summary>Joue le son à la position du Transform donné.</summary>
    public void Play(Transform target) => Play(target.position);

    /// <summary>
    /// Remet le compteur de pitch progressif à zéro.
    /// Le prochain Play repartira de basePitch.
    /// </summary>
    public void ResetPitchCounter()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.ResetPitchCounter(this);
    }

    /// <summary>Retourne le nombre de lectures enregistrées pour le pitch progressif.</summary>
    public int GetPitchCount()
        => SoundManager.Instance != null ? SoundManager.Instance.GetPitchCount(this) : 0;

    // ─────────────────────────────────────────
    // Interne
    // ─────────────────────────────────────────

    /// <summary>Retourne un clip aléatoire parmi le tableau. Public pour SoundManager.</summary>
    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    private bool Validate()
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"SoundConfig '{name}' : aucun clip assigné !");
            return false;
        }

        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager introuvable dans la scène !");
            return false;
        }

        return true;
    }
}