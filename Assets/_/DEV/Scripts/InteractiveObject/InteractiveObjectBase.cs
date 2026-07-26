using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InteractiveObjectBase : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private SoundConfig _ticSound;
    [SerializeField] private SoundConfig _alarmSound;
    
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameManager gameManager;

    [Header("Interval Event")]
    [Tooltip("X = ratio de temps restant (1 = début, 0 = fin). Y = intervalle en secondes entre chaque tic.")]
    [SerializeField] private AnimationCurve tickIntervalCurve = AnimationCurve.Linear(0f, 0.05f, 1f, 1f);

    [SerializeField] private float minTickInterval = 0.05f; // garde-fou anti-spam infini

    public event Action OnIntervalTick;

    private float intervalTimer;
    
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private LayerMask interactiveObject; // <-- Layer autorisé
    

    [Header("Text")]
    [SerializeField] protected TMP_Text timerText;
    
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

        if (viewCamera == null)
            viewCamera = Camera.main;
    }
    
    private void Update()
    {
        if (_timer != null)
        {
            _timer.Tick();
        }
        
        if (!isBeingViewed) return;

        HandleRotation();
    }

    public virtual void StartItem()
    {
        _timer = new Timer(initialDuration)
            .OnTick(currentTime =>
            {
                DisplayTimer(_timer.RemainingTime);
                HandleIntervalTick();
            })
            .OnComplete(() =>
            {
                gameManager.IncreaseNbrOfMisstakes();
                _timer.Reset();
                _timer.Start();
                if (_alarmSound == null)
                {
                    Debug.LogWarning("SoundDesign");
                }
                else
                {
                    _alarmSound.Play(transform.position);
                }
            });
        _timer.Start();
    }

    private void HandleIntervalTick()
    {
        if (initialDuration <= 0f) return;

        float ratio = Mathf.Clamp01(_timer.RemainingTime / initialDuration);
        float currentInterval = Mathf.Max(tickIntervalCurve.Evaluate(ratio), minTickInterval);

        intervalTimer += Time.deltaTime;

        if (intervalTimer >= currentInterval)
        {
            intervalTimer = 0f;
            OnIntervalTick?.Invoke();

            if (_ticSound == null)
            {
                Debug.LogWarning("SoundDesign");
            }
            else
            {
                _ticSound.Play(transform.position);
            }
        }
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