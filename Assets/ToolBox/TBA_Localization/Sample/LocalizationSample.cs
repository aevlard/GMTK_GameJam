using System;
using TMPro;
using UnityEngine;

public class LocalizationSample : MonoBehaviour
{
    [SerializeField] private TMP_Text texts;
    [SerializeField] private string localizationKey = "L_ingame_hello";

    private void Start()
    {
        // S'abonner au changement de langue
        LocalizationManager.Instance.onLanguageChanged += UpdateText;
        UpdateText(); // Initial
    }

    private void UpdateText()
    {
        texts.text = LocalizationManager.Instance.GetText(localizationKey);
    }

    private void OnDestroy()
    {
        // Important : se désabonner pour éviter memory leak
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.onLanguageChanged -= UpdateText;
        }
    }
}
