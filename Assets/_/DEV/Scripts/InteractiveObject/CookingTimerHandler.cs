using UnityEngine;

public class CookingTimerHandler : InteractiveObjectBase
{
    [Header("Knob")]
    [SerializeField] private DraggableRotator knobRotator;

    [Header("Config cadran")]
    [Tooltip("Rotation totale (en degrés) correspondant au temps initial complet")]
    [SerializeField] private float degreesForFullDuration = 360f;

    protected override void DisplayTimer(float timerRemainingTime)
    {
        base.DisplayTimer(timerRemainingTime);

        if (knobRotator == null || initialDuration <= 0f) return;
        if (knobRotator.IsDragging) return; // le joueur a la main, on ne touche à rien

        float ratio = Mathf.Clamp01(timerRemainingTime / initialDuration);
        float targetAngle = ratio * degreesForFullDuration;

        knobRotator.SetAngle(targetAngle);
    }
}