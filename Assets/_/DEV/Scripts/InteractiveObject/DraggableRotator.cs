using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DragMode
{
    Horizontal,
    Vertical,
    Circular
}

public enum RotationAxis
{
    X,
    Y,
    Z
}

public class DraggableRotator : MonoBehaviour
{
    [Header("Détection du clic")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Camera cam;

    [Header("Mode de rotation")]
    [SerializeField] private DragMode dragMode = DragMode.Horizontal;
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private float sensitivity = 0.2f;

    [Header("Limites (optionnel)")]
    [SerializeField] private bool useLimits;
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    [Header("Debug")]
    [SerializeField] private bool showCurrentAngle;

    /// <summary>
    /// Appelé à chaque changement de rotation, avec l'angle total actuel
    /// </summary>
    public event Action<float> OnRotationChanged;

    private bool isDragging;
    private float currentAngle;
    private Vector3 localAxis;
    private float previousMouseAngle;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;

        localAxis = GetAxisVector(rotationAxis);
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
                    HandleCircularDrag();
                    break;
            }
        }

        if (showCurrentAngle)
            Debug.Log(gameObject.name + " angle: " + currentAngle);
    }

    private void HandleClickDetection()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;

                    if (dragMode == DragMode.Circular)
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

    private void HandleCircularDrag()
    {
        Vector2 screenCenter = cam.WorldToScreenPoint(transform.position);
        float currentMouseAngle = GetMouseAngle(screenCenter);

        float deltaAngle = Mathf.DeltaAngle(previousMouseAngle, currentMouseAngle);
        previousMouseAngle = currentMouseAngle;

        ApplyRotation(deltaAngle * sensitivity);
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