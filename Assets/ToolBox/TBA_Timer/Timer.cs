using System;
using UnityEngine;

/// <summary>
/// Système de timer flexible et réutilisable
/// </summary>
public class Timer
{
    public float Duration { get; private set; }
    public float CurrentTime { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsFinished => CurrentTime >= Duration && !loop;
    public float Progress => Duration > 0 ? Mathf.Clamp01(CurrentTime / Duration) : 0f;
    public float RemainingTime => Mathf.Max(0, Duration - CurrentTime);

    private bool loop;
    private bool useUnscaledTime;
    private Action onComplete;
    private Action<float> onTick;

    /// <summary>
    /// Crée un nouveau timer
    /// </summary>
    /// <param name="duration">Durée du timer en secondes</param>
    public Timer(float duration)
    {
        Duration = duration;
        CurrentTime = 0f;
        IsRunning = false;
        loop = false;
        useUnscaledTime = false;
    }

    /// <summary>
    /// Démarre ou redémarre le timer
    /// </summary>
    public Timer Start()
    {
        IsRunning = true;
        CurrentTime = 0f;
        return this;
    }

    /// <summary>
    /// Met le timer en pause
    /// </summary>
    public Timer Pause()
    {
        IsRunning = false;
        return this;
    }

    /// <summary>
    /// Reprend le timer après une pause
    /// </summary>
    public Timer Resume()
    {
        IsRunning = true;
        return this;
    }

    /// <summary>
    /// Arrête et réinitialise le timer
    /// </summary>
    public Timer Stop()
    {
        IsRunning = false;
        CurrentTime = 0f;
        return this;
    }

    /// <summary>
    /// Reset le timer sans l'arrêter
    /// </summary>
    public Timer Reset()
    {
        CurrentTime = 0f;
        return this;
    }

    /// <summary>
    /// Active/désactive le mode loop
    /// </summary>
    public Timer SetLoop(bool isLooping)
    {
        loop = isLooping;
        return this;
    }

    /// <summary>
    /// Utilise Time.unscaledDeltaTime au lieu de Time.deltaTime
    /// </summary>
    public Timer SetUnscaledTime(bool unscaled)
    {
        useUnscaledTime = unscaled;
        return this;
    }

    /// <summary>
    /// Change la durée du timer
    /// </summary>
    public Timer SetDuration(float newDuration)
    {
        Duration = newDuration;
        return this;
    }

    /// <summary>
    /// Définit un callback appelé quand le timer se termine
    /// </summary>
    public Timer OnComplete(Action callback)
    {
        onComplete = callback;
        return this;
    }

    /// <summary>
    /// Définit un callback appelé à chaque frame avec le temps actuel
    /// </summary>
    public Timer OnTick(Action<float> callback)
    {
        onTick = callback;
        return this;
    }

    /// <summary>
    /// Met à jour le timer (à appeler dans Update)
    /// </summary>
    public void Tick()
    {
        if (!IsRunning) return;

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        CurrentTime += deltaTime;

        onTick?.Invoke(CurrentTime);

        if (CurrentTime >= Duration)
        {
            if (loop)
            {
                CurrentTime -= Duration;
                onComplete?.Invoke();
            }
            else
            {
                CurrentTime = Duration;
                IsRunning = false;
                onComplete?.Invoke();
            }
        }
    }

    /// <summary>
    /// Force la complétion du timer
    /// </summary>
    public Timer ForceComplete()
    {
        CurrentTime = Duration;
        IsRunning = false;
        onComplete?.Invoke();
        return this;
    }

    /// <summary>
    /// Ajoute du temps au timer
    /// </summary>
    public Timer AddTime(float time)
    {
        CurrentTime = Mathf.Min(CurrentTime + time, Duration);
        return this;
    }

    /// <summary>
    /// Crée un timer et le démarre immédiatement
    /// </summary>
    public static Timer StartNew(float duration)
    {
        return new Timer(duration).Start();
    }

    /// <summary>
    /// Crée un timer en loop qui démarre immédiatement
    /// </summary>
    public static Timer StartNewLoop(float duration)
    {
        return new Timer(duration).SetLoop(true).Start();
    }
}
