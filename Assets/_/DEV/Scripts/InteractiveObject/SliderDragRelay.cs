using UnityEngine;
using UnityEngine.EventSystems;

public class SliderDragRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private PhoneHandler phoneHandler;
    [SerializeField] private bool isBaitSlider; // coche cette case sur le relay du bait

    public void OnPointerDown(PointerEventData eventData) => phoneHandler.SetDragging(true, isBaitSlider);
    public void OnPointerUp(PointerEventData eventData) => phoneHandler.SetDragging(false, isBaitSlider);
}