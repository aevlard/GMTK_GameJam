using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager global pour gérer automatiquement les timers
/// Permet d'éviter d'appeler Tick() manuellement
/// </summary>
public class TimerManager : MonoBehaviour
{
    private static TimerManager instance;
    private List<Timer> timers = new List<Timer>();
    private List<Timer> timersToRemove = new List<Timer>();

    public static TimerManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TimerManager");
                instance = go.AddComponent<TimerManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Update()
    {
        // Mise à jour de tous les timers
        foreach (var timer in timers)
        {
            timer.Tick();

            // Marquer pour suppression si terminé et non-loop
            if (timer.IsFinished)
            {
                timersToRemove.Add(timer);
            }
        }

        // Nettoyer les timers terminés
        foreach (var timer in timersToRemove)
        {
            timers.Remove(timer);
        }
        timersToRemove.Clear();
    }

    /// <summary>
    /// Enregistre un timer pour qu'il soit mis à jour automatiquement
    /// </summary>
    public static void Register(Timer timer)
    {
        if (!Instance.timers.Contains(timer))
        {
            Instance.timers.Add(timer);
        }
    }

    /// <summary>
    /// Désenregistre un timer
    /// </summary>
    public static void Unregister(Timer timer)
    {
        Instance.timers.Remove(timer);
    }

    /// <summary>
    /// Crée et enregistre automatiquement un nouveau timer
    /// </summary>
    public static Timer CreateTimer(float duration)
    {
        var timer = new Timer(duration);
        Register(timer);
        return timer;
    }

    /// <summary>
    /// Arrête tous les timers
    /// </summary>
    public static void StopAll()
    {
        foreach (var timer in Instance.timers)
        {
            timer.Stop();
        }
    }

    /// <summary>
    /// Nettoie tous les timers
    /// </summary>
    public static void Clear()
    {
        Instance.timers.Clear();
        Instance.timersToRemove.Clear();
    }
}
