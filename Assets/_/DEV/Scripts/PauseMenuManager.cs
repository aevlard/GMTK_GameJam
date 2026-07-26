using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    
    [SerializeField] private PlayerInput playerInput;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            playerInput.SwitchCurrentActionMap("ObjectView");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnCloseButtonClick()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        playerInput.SwitchCurrentActionMap("Player");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnRestartButtonClick()
    {
        SceneLoader.Instance.LoadLevel("Redouan_Scene");
    }

    public void OnSettingsButtonClick()
    {
        settingsMenu.SetActive(true);
    }

    public void OnQuitButtonClick()
    {
        Application.Quit();
    }

    public void OnQuitSettingsButtonClick()
    {
        settingsMenu.SetActive(false);
    }
}
