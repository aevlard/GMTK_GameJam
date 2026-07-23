using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerSample : MonoBehaviour
{
    private PlayerInput playerInput;


    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (playerInput.actions["SayHello"].triggered)
        {
            Debug.Log("Hello");
        }
    }
}
