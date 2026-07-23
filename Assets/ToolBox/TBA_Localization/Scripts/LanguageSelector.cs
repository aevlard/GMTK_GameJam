using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LanguageSelector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    
    [Header("Alternative: Buttons")]
    [SerializeField] private Button frenchButton;
    [SerializeField] private Button englishButton;
    [SerializeField] private Button dutchButton;

    private void Start()
    {
        if (languageDropdown != null)
        {
            SetupDropdown();
        }

        if (frenchButton != null)
        {
            frenchButton.onClick.AddListener(() => SetLanguage(SystemLanguage.French));
        }

        if (englishButton != null)
        {
            englishButton.onClick.AddListener(() => SetLanguage(SystemLanguage.English));
        }

        if (dutchButton != null)
        {
            dutchButton.onClick.AddListener(() => SetLanguage(SystemLanguage.Dutch));
        }
    }

    private void SetupDropdown()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError("LocalizationManager introuvable !");
            return;
        }

        List<SystemLanguage> availableLanguages = LocalizationManager.Instance.GetAvailableLanguages();
        
        languageDropdown.ClearOptions();
        
        List<string> languageNames = new List<string>();
        foreach (SystemLanguage lang in availableLanguages)
        {
            languageNames.Add(GetLanguageDisplayName(lang));
        }

        languageDropdown.AddOptions(languageNames);

        // Sélectionner la langue actuelle dans le dropdown
        SystemLanguage currentLang = LocalizationManager.Instance.GetCurrentLanguage();
        int currentIndex = availableLanguages.IndexOf(currentLang);
        if (currentIndex >= 0)
        {
            languageDropdown.value = currentIndex;
        }

        // Écouter les changements
        languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        if (LocalizationManager.Instance == null) return;

        List<SystemLanguage> availableLanguages = LocalizationManager.Instance.GetAvailableLanguages();
        
        if (index >= 0 && index < availableLanguages.Count)
        {
            SetLanguage(availableLanguages[index]);
        }
    }

    private void SetLanguage(SystemLanguage language)
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(language);
        }
    }

    private string GetLanguageDisplayName(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.French:
                return "Français";
            case SystemLanguage.English:
                return "English";
            case SystemLanguage.Dutch:
                return "Nederlands";
            case SystemLanguage.German:
                return "Deutsch";
            case SystemLanguage.Spanish:
                return "Español";
            default:
                return language.ToString();
        }
    }

    private void OnDestroy()
    {
        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }

        if (frenchButton != null)
        {
            frenchButton.onClick.RemoveAllListeners();
        }

        if (englishButton != null)
        {
            englishButton.onClick.RemoveAllListeners();
        }

        if (dutchButton != null)
        {
            dutchButton.onClick.RemoveAllListeners();
        }
    }
}