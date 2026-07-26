using System;
using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    
    private float time;

    private void Start()
    {
        time = PlayerPrefs.GetFloat("timeLeft");
        DisplayTimer(time);
    }

    public void OnRestartButtonClick()
    {
        SceneLoader.Instance.LoadLevel("Redouan_Scene");
    }

    public void OnMenuButtonClick()
    {
        SceneLoader.Instance.LoadLevel("MainMenu_Scene");
    }

    public void OnQuitButtonClick()
    {
        Application.Quit();
    }
    
    private void DisplayTimer(float timerRemainingTime)
    {
        int minutes = Mathf.FloorToInt(timerRemainingTime / 60f);
        int seconds = Mathf.FloorToInt(timerRemainingTime % 60f);
        int centiseconds = Mathf.FloorToInt((timerRemainingTime * 100f) % 100f);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, centiseconds);
        }
    }
}
