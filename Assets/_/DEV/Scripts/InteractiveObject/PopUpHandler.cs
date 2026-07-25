using UnityEngine;
using UnityEngine.UI;

public class PopUpHandler : MonoBehaviour
{
    private Button _button;

    void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        transform.parent.gameObject.SetActive(false);
    }
}