using System;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    
    public InteractiveObjectBase _interactiveObject;

    void OnMouseDown()
    {
        _interactiveObject.ResetTimer();
    }
}