using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ClimPadHandler : InteractiveObjectBase
{
    [SerializeField] private TMP_Text postItCodeText;
    [SerializeField] private TMP_Text userInputCodeText;
    [SerializeField] private float timeToHadPerClick = 10f;
    
    private string climCode = null;
    private string currentCode = "";
    private bool isLocked = true;

    public override void Start()
    {
        base.Start();
        GenerateCode();
        DisplayCodeToPad();
    }

    private void GenerateCode()
    {
        for (int i = 0; i < 4; i++)
        {
            int randomnbr = UnityEngine.Random.Range(0, 10);
            climCode += randomnbr;
        }
        DisplayCodeToPostIt(climCode);
    }

    private void DisplayCodeToPostIt(string s)
    {
        postItCodeText.text = s;
    }

    public void OnNumPadClicked(string s)
    {
        if (currentCode.Length == 4) return;
        currentCode += s;
        DisplayCodeToPad();
    }
    
    public void OnValidateClicked()
    {
        if (currentCode == climCode)
        {
            timerText.gameObject.SetActive(true);
            userInputCodeText.gameObject.SetActive(false);
            isLocked = false;
        }
        else
        {
            currentCode = "";
            DisplayCodeToPad();
            isLocked = true;
        }
    }
    
    public void OnCancelClicked()
    {
        string result = currentCode.Substring(0, currentCode.Length - 1);
        currentCode = result;
        DisplayCodeToPad();
    }

    public void OnReturnButtonClicked()
    {
        ResetClim();
        currentCode = "";
        isLocked = true;
        DisplayCodeToPad();
    }

    private void DisplayCodeToPad()
    {
        userInputCodeText.text = currentCode;
    }

    public void ResetClim()
    {
        timerText.gameObject.SetActive(false);
        userInputCodeText.gameObject.SetActive(true);
        currentCode = "";
        DisplayCodeToPad();
        isLocked = true;
    }

    public void OnAddtimeClick()
    {
        AddTime(timeToHadPerClick);
    }
    
    public override void ReturnToInitalPosition()
    {
        base.ReturnToInitalPosition();
        ResetClim();
    }
}
