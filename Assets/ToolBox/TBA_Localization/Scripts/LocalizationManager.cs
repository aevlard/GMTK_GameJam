using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Localization Settings")]
    [SerializeField] private TextAsset localizationCSV;
    [SerializeField] private SystemLanguage defaultLanguage = SystemLanguage.English;

    private Dictionary<string, Dictionary<SystemLanguage, string>> localizationData;
    private SystemLanguage currentLanguage;
    
    public event Action onLanguageChanged;

    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        LoadLocalizationData();
        LoadSavedLanguage();
    }

    private void LoadLocalizationData()
    {
        localizationData = new Dictionary<string, Dictionary<SystemLanguage, string>>();

        if (localizationCSV == null)
        {
            Debug.LogError("Localization CSV non assigné !");
            return;
        }

        string[] lines = localizationCSV.text.Split('\n');

        if (lines.Length < 2)
        {
            Debug.LogError("Le CSV de localisation est vide ou invalide !");
            return;
        }

        // Parse la première ligne pour obtenir les langues
        string[] headers = lines[0].Split(',');
        List<SystemLanguage> languages = new List<SystemLanguage>();

        for (int i = 1; i < headers.Length; i++)
        {
            string langName = headers[i].Trim();
            SystemLanguage lang = ParseLanguage(langName);
            languages.Add(lang);
        }

        // Parse les données
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if (values.Length < 2) continue;

            string key = values[0].Trim();
            Dictionary<SystemLanguage, string> translations = new Dictionary<SystemLanguage, string>();

            for (int j = 1; j < values.Length && j - 1 < languages.Count; j++)
            {
                translations[languages[j - 1]] = values[j].Trim();
            }

            localizationData[key] = translations;
        }

        Debug.Log($"Localisation chargée : {localizationData.Count} clés trouvées");
    }

    private SystemLanguage ParseLanguage(string langName)
    {
        switch (langName.ToLower())
        {
            case "french":
            case "français":
                return SystemLanguage.French;
            case "english":
            case "anglais":
                return SystemLanguage.English;
            case "dutch":
            case "néerlandais":
                return SystemLanguage.Dutch;
            case "german":
            case "allemand":
                return SystemLanguage.German;
            case "spanish":
            case "espagnol":
                return SystemLanguage.Spanish;
            default:
                Debug.LogWarning($"Langue inconnue : {langName}, utilisation de English par défaut");
                return SystemLanguage.English;
        }
    }

    private void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
        {
            int savedLang = PlayerPrefs.GetInt(LANGUAGE_PREF_KEY);
            currentLanguage = (SystemLanguage)savedLang;
        }
        else
        {
            // Utiliser la langue du système ou la langue par défaut
            currentLanguage = Application.systemLanguage;
            
            // Vérifier si la langue du système est supportée
            if (!IsLanguageSupported(currentLanguage))
            {
                currentLanguage = defaultLanguage;
            }
        }

        Debug.Log($"Langue chargée : {currentLanguage}");
    }

    /// <summary>
    /// Change la langue actuelle
    /// </summary>
    public void SetLanguage(SystemLanguage language)
    {
        if (!IsLanguageSupported(language))
        {
            Debug.LogWarning($"Langue {language} non supportée, utilisation de {defaultLanguage}");
            language = defaultLanguage;
        }

        currentLanguage = language;
        PlayerPrefs.SetInt(LANGUAGE_PREF_KEY, (int)language);
        PlayerPrefs.Save();

        // Notifier tous les textes localisés
        onLanguageChanged?.Invoke();

        Debug.Log($"Langue changée : {currentLanguage}");
    }

    /// <summary>
    /// Retourne le texte traduit pour une clé donnée
    /// </summary>
    public string GetText(string key)
    {
        if (localizationData == null || !localizationData.ContainsKey(key))
        {
            Debug.LogWarning($"Clé de localisation '{key}' introuvable !");
            return $"[{key}]";
        }

        if (localizationData[key].ContainsKey(currentLanguage))
        {
            return localizationData[key][currentLanguage];
        }

        // Fallback sur la langue par défaut
        if (localizationData[key].ContainsKey(defaultLanguage))
        {
            Debug.LogWarning($"Traduction pour '{key}' en {currentLanguage} introuvable, utilisation de {defaultLanguage}");
            return localizationData[key][defaultLanguage];
        }

        Debug.LogError($"Aucune traduction trouvée pour '{key}'");
        return $"[{key}]";
    }

    /// <summary>
    /// Retourne la langue actuellement active
    /// </summary>
    public SystemLanguage GetCurrentLanguage()
    {
        return currentLanguage;
    }

    /// <summary>
    /// Vérifie si une langue est supportée
    /// </summary>
    public bool IsLanguageSupported(SystemLanguage language)
    {
        if (localizationData == null || localizationData.Count == 0)
            return false;

        // Vérifier si au moins une clé contient cette langue
        foreach (var translations in localizationData.Values)
        {
            if (translations.ContainsKey(language))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Retourne toutes les langues disponibles
    /// </summary>
    public List<SystemLanguage> GetAvailableLanguages()
    {
        HashSet<SystemLanguage> languages = new HashSet<SystemLanguage>();

        if (localizationData != null)
        {
            foreach (var translations in localizationData.Values)
            {
                foreach (var lang in translations.Keys)
                {
                    languages.Add(lang);
                }
            }
        }

        return new List<SystemLanguage>(languages);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}