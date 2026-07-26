using System;
using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void OnCloseButtonClick()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnRestartButtonClick()
    {
        SceneLoader.Instance.LoadLevel("Redouane_Scene");
    }

    private void OnSettingsButtonClick()
    {
        settingsMenu.SetActive(true);
    }

    private void OnQuitButtonClick()
    {
        Application.Quit();
    }

    private void OnQuitSettingsButtonClick()
    {
        settingsMenu.SetActive(false);
    }
}
