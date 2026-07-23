using System;
using TMPro;
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

    private Vector3 initialPosition;
    private Quaternion initialRoation;
    private Camera viewCamera;
    
    private float travelSpeed = 3f;
    private bool isBeingViewed;
    private Timer _timer;

    private void Start()
    {
        viewCamera = Camera.main;
        
        initialPosition = transform.position;
        initialRoation = transform.rotation;
        
        _timer = new Timer(60f)
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
        Debug.Log("Hello i'm" + transform.name);
    }

    public virtual void MoveToPlayer(Transform playerHand)
    {
        playerInput.SwitchCurrentActionMap("ObjectView");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        transform.position = playerHand.position;

        isBeingViewed = true;
    }
    
    public void ReturnToInitalPosition()
    {
        playerInput.SwitchCurrentActionMap("Player");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        transform.position = initialPosition;
        transform.rotation = initialRoation;

        isBeingViewed = false;
    }

    private void HandleRotation()
    {
        if (Mouse.current == null) return;

        // Au moment où on clique, on vérifie le layer touché
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log("Hit " + hit.collider.name);
    
                bool isOnCorrectLayer = ((1 << hit.collider.gameObject.layer) & interactiveObject) != 0;
                canRotate = isOnCorrectLayer && hit.transform == transform;
            }
            else
            {
                canRotate = false;
            }
        }

        if (canRotate && Mouse.current.leftButton.isPressed)
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

    private void DisplayTimer(float timerRemainingTime)
    {
        int minutes = Mathf.FloorToInt(timerRemainingTime / 60f);
        int seconds = Mathf.FloorToInt(timerRemainingTime % 60f);
        int milliseconds = Mathf.FloorToInt((timerRemainingTime * 1000f) % 1000f);

        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}