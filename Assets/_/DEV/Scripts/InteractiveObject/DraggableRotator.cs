using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DragMode
{
    Horizontal,
    Vertical,
    Circular,
    InverseCircular,
}

public enum RotationAxis
{
    X,
    Y,
    Z
}

public class DraggableRotator : MonoBehaviour
{
    
    [Header("Détection du clic")] [SerializeField]
    private LayerMask interactableLayer;

    [SerializeField] private Camera cam;

    [Header("Mode de rotation")] [SerializeField]
    private DragMode dragMode = DragMode.Horizontal;

    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private bool invertAxis;
    [SerializeField] private float sensitivity = 0.2f;

    [Header("Cran de rotation (optionnel)")]
    [SerializeField] private float rotationStep = 15f;
    [SerializeField] private float timeToHadPerStep = 5f;
    
    public event Action<int> OnRotationStep;

    [Header("Limites (optionnel)")] [SerializeField]
    private bool useLimits;

    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    [Header("Debug")] [SerializeField] private bool showCurrentAngle;
    
    [Header("Variables")]
    [SerializeField] private InteractiveObjectBase _interactiveObject;
    
    
    public event Action<float> OnRotationChanged;

    private bool isDragging;
    private float currentAngle;
    private Vector3 localAxis;
    private float previousMouseAngle;
    private int lastStepIndex;
    
    

    private void Start()
    {
        
        if (cam == null)
            cam = Camera.main;

        localAxis = GetAxisVector(rotationAxis);

        if (invertAxis)
            localAxis = -localAxis;
    }
    private void Update()
    {
        if (Mouse.current == null) return;

        HandleClickDetection();

        if (isDragging)
        {
            switch (dragMode)
            {
                case DragMode.Horizontal:
                case DragMode.Vertical:
                    HandleLinearDrag();
                    break;
                case DragMode.Circular:
                    HandleCircularDrag(1f);
                    break;
                case DragMode.InverseCircular:
                    HandleCircularDrag(-1f);
                    break;
            }
        }

        if (showCurrentAngle)
            Debug.Log(gameObject.name + " angle: " + currentAngle);
    }

    public bool IsDragging => isDragging;

    public void SetAngle(float angle)
    {
        if (isDragging) return; // sécurité : jamais forcer pendant que le joueur drag

        float delta = angle - currentAngle;
        currentAngle = angle;
        transform.Rotate(localAxis, delta, Space.Self);

        // Resynchronise le compteur de crans pour éviter un faux déclenchement
        // d'OnRotationStep/AddTime au prochain drag du joueur
        if (rotationStep > 0f)
            lastStepIndex = Mathf.FloorToInt(currentAngle / rotationStep);

        OnRotationChanged?.Invoke(currentAngle);
    }
    private void HandleClickDetection()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
            {
                Debug.Log("Hit " + hit.collider.name);
                if (hit.transform == transform)
                {
                    isDragging = true;

                    if (dragMode == DragMode.Circular || dragMode == DragMode.InverseCircular)
                    {
                        Vector2 screenCenter = cam.WorldToScreenPoint(transform.position);
                        previousMouseAngle = GetMouseAngle(screenCenter);
                    }
                }
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void HandleLinearDrag()
    {
        Vector2 delta = Mouse.current.delta.ReadValue();

        float rawAmount = dragMode == DragMode.Horizontal
            ? -delta.x
            : delta.y;

        ApplyRotation(rawAmount * sensitivity);
    }

    private void HandleCircularDrag(float directionMultiplier)
    {
        Vector2 screenCenter = cam.WorldToScreenPoint(transform.position);
        float currentMouseAngle = GetMouseAngle(screenCenter);

        float deltaAngle = Mathf.DeltaAngle(previousMouseAngle, currentMouseAngle);
        previousMouseAngle = currentMouseAngle;

        ApplyRotation(deltaAngle * sensitivity * directionMultiplier);
    }

    private void ApplyRotation(float amount)
    {
        float appliedAmount = amount;

        if (useLimits)
        {
            float newAngle = Mathf.Clamp(currentAngle + amount, minAngle, maxAngle);
            appliedAmount = newAngle - currentAngle;
            currentAngle = newAngle;
        }
        else
        {
            currentAngle += amount;
        }

        transform.Rotate(localAxis, appliedAmount, Space.Self);
        OnRotationChanged?.Invoke(currentAngle);

        CheckRotationStep();
    }

    private void CheckRotationStep()
    {
        if (rotationStep <= 0f) return;

        int currentStepIndex = Mathf.FloorToInt(currentAngle / rotationStep);

        if (currentStepIndex != lastStepIndex)
        {
            int stepsCrossed = currentStepIndex - lastStepIndex;
            lastStepIndex = currentStepIndex;
            OnRotationStep?.Invoke(stepsCrossed);
            _interactiveObject.AddTime(timeToHadPerStep);
        }
    }

    private float GetMouseAngle(Vector2 screenCenter)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 dir = mousePos - screenCenter;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    private Vector3 GetAxisVector(RotationAxis axis)
    {
        switch (axis)
        {
            case RotationAxis.X: return Vector3.right;
            case RotationAxis.Y: return Vector3.up;
            case RotationAxis.Z: return Vector3.forward;
            default: return Vector3.up;
        }
    }

    public float GetCurrentAngle() => currentAngle;
}