using System;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    
    private InteractiveObjectBase _interactiveObject;

    private void Start()
    {
        _interactiveObject = GetComponentInParent<InteractiveObjectBase>();
    }

    void OnMouseDown()
    {
        _interactiveObject.ResetTimer();
    }
}