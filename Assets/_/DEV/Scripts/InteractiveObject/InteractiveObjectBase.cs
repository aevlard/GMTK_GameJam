using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractiveObjectBase : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;
    

    private Vector3 initialPosition;
    private Quaternion initialRoation;
    
    private float travelSpeed = 3f;
    private bool isBeingViewed;
    private Timer _timer;

    private void Start()
    {
        initialPosition = transform.position;
        initialRoation = transform.rotation;
        
        _timer = new Timer(60f);
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

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            
            transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            
            transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
        }
    }

    public virtual void ResetTimer()
    {
        _timer.Reset();
    }


}