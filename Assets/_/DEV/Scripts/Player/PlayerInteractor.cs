using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionObjectLayer;
    [SerializeField] private LayerMask interactionElementLayer;
    [SerializeField] private Transform playerHand;

    private PlayerInput _playerInput;
    private PlayerState _playerState;
    private InteractiveObjectBase currentInteractiveObject;


    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        if (_playerInput == null) return;

        if (_playerInput.actions["InteractWithObject"].WasPerformedThisFrame())
        {
            TryInteractWithObject();
        }

        if (_playerInput.actions["StopInteract"].WasPerformedThisFrame())
        {
            StopInteract();
        }
    }
    

    private void StopInteract()
    {
        _playerState.inFreeLook = true;
        _playerState.inInteraction = false;
        
        currentInteractiveObject.ReturnToInitalPosition();
        currentInteractiveObject = null;
    }

    private void TryInteractWithObject()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, transform.forward, out hit,
                interactionRange, interactionObjectLayer))
        {
            if (hit.transform.gameObject.TryGetComponent<InteractiveObjectBase>(out var interactiveObjectBase))
            {
                _playerState.inFreeLook = false;
                _playerState.inInteraction = true;
                
                currentInteractiveObject = interactiveObjectBase;
                interactiveObjectBase.SayHello();
                interactiveObjectBase.MoveToPlayer(playerHand);
            }
        }
    }
}