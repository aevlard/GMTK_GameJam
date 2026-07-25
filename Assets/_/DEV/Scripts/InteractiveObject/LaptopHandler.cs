using UnityEngine;

public class LaptopHandler : InteractiveObjectBase
{
    [SerializeField] private GameObject popUpContainer;

    public override void ResetTimer()
    {
        base.ResetTimer();
        ResetPopUp();
    }

    private void ResetPopUp()
    {
        for (int i = 0; i < popUpContainer.transform.childCount; i++)
        {
            popUpContainer.transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}
