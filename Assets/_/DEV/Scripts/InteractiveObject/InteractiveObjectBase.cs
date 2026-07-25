using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InteractiveObjectBase : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private LayerMask interactiveObject; // <-- Layer autorisé
    

    [Header("Text")]
    [SerializeField] private TMP_Text timerText;
    
    [Header("Timer")]
    [SerializeField] protected float initialDuration;

    private Vector3 initialPosition;
    private Quaternion initialRoation;
    private Camera viewCamera;
    private CinemachineCamera _objectCamera;
    public CinemachineCamera playerCamera;
    
    private float travelSpeed = 3f;
    private bool isBeingViewed;
    public Timer _timer;

    public virtual void Start()
    {
        _objectCamera = transform.GetChild(0).GetComponent<CinemachineCamera>();
        
        viewCamera = Camera.main;
        
        initialPosition = transform.position;
        initialRoation = transform.rotation;
        
        _timer = new Timer(initialDuration)
            .OnTick(currentTime =>
        {
            DisplayTimer(_timer.RemainingTime);
        });
        
        _timer.Start();

        if (viewCamera == null)
            viewCamera = Camera.main;
    }
    
    private void Update()
    {
        _timer.Tick();
        
        if (!isBeingViewed) return;

        HandleRotation();

        
    }

    public virtual void SayHello()
    {
        
    }

    public virtual void MoveToPlayer(Transform playerHand)
    {
        playerInput.SwitchCurrentActionMap("ObjectView");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (_objectCamera != null)
        {
            _objectCamera.Priority = 1;
            playerCamera.Priority = 0;
        }
        else
        {
            transform.position = playerHand.position;
        }
        
        isBeingViewed = true;
    }
    
    public virtual void ReturnToInitalPosition()
    {
        playerInput.SwitchCurrentActionMap("Player");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (_objectCamera != null)
        {
            _objectCamera.Priority = 0;
            playerCamera.Priority = 1;
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRoation;
        }
        
        isBeingViewed = false;
    }

    private void HandleRotation()
    {
        if (Mouse.current == null || _objectCamera != null) return;

        // Au moment où on clique, on vérifie le layer touché
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                bool isOnCorrectLayer = ((1 << hit.collider.gameObject.layer) & interactiveObject) != 0;
                canRotate = isOnCorrectLayer && hit.transform == transform;
            }
            else
            {
                canRotate = false;
            }
        }

        if (canRotate && Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            
            transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            
            transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
        }
    }

    private bool canRotate;

    public virtual void ResetTimer()
    {
        _timer.Reset();
    }

    protected virtual void DisplayTimer(float timerRemainingTime)
    {
        int minutes = Mathf.FloorToInt(timerRemainingTime / 60f);
        int seconds = Mathf.FloorToInt(timerRemainingTime % 60f);
        int centiseconds = Mathf.FloorToInt((timerRemainingTime * 100f) % 100f);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, centiseconds);
        }
    }

    public void AddTime(float time)
    {
        _timer.AddRemainingTime(time);
    }
}