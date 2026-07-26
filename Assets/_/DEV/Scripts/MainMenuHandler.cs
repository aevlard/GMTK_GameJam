using TMPro.EditorUtilities;
using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject rulesPanel;
    
    
    public void OnPlayButtonClicked()
    {
        SceneLoader.Instance.LoadLevel("Redouan_Scene");
    }

    public void OnSettingsButtonClicked()
    {
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        rulesPanel.SetActive(false);
    }

    public void OnCreditsButtonClicked()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
        rulesPanel.SetActive(false);
    }

    public void OnRulesButtonClicked()
    {
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        rulesPanel.SetActive(true);
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    public void OnCloseButtonClicked(GameObject panelToClose)
    {
        panelToClose.SetActive(false);
    }
}
