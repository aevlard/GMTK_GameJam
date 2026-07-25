using UnityEngine;

public class AlarmClockHandler : InteractiveObjectBase
{
    
    [Header("Aiguilles")]
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;

    [Header("Config cadran")]
    [Tooltip("Combien de secondes réelles représentent un tour complet de la grande aiguille")]
    [SerializeField] private float secondsPerFullMinuteRotation = 60f;

    [Tooltip("Axe local sur lequel tournent les aiguilles (souvent Z pour un cadran face caméra)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;

    protected override void DisplayTimer(float timerRemainingTime)
    {
        base.DisplayTimer(timerRemainingTime);
        
        float symbolicMinutes = timerRemainingTime / secondsPerFullMinuteRotation;
        
        float minuteAngle = (symbolicMinutes % 1f) * 360f;
        
        float hourAngle = (symbolicMinutes % 12f) * (360f / 12f);

        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(rotationAxis.normalized * -minuteAngle);

        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(rotationAxis.normalized * -hourAngle);
    }
    
}
