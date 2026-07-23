using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [Header("Localization Key")]
    [SerializeField] private string localizationKey;

    [Header("Optional Formatting")]
    [Tooltip("Si activé, permet d'utiliser {0}, {1} etc. pour formater le texte")]
    [SerializeField] private bool useFormatting = false;

    private TextMeshProUGUI textComponent;
    private object[] formatArgs;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        UpdateText();
        
        // S'abonner aux changements de langue
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.onLanguageChanged += UpdateText;
        }
    }

    private void OnDestroy()
    {
        // Se désabonner pour éviter les memory leaks
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.onLanguageChanged -= UpdateText;
        }
    }

    /// <summary>
    /// Met à jour le texte avec la langue actuelle
    /// </summary>
    public void UpdateText()
    {
        if (textComponent == null || LocalizationManager.Instance == null)
            return;

        if (string.IsNullOrEmpty(localizationKey))
        {
            Debug.LogWarning($"Clé de localisation vide sur {gameObject.name}");
            return;
        }

        string localizedText = LocalizationManager.Instance.GetText(localizationKey);

        // Appliquer le formatage si activé et des arguments sont fournis
        if (useFormatting && formatArgs != null && formatArgs.Length > 0)
        {
            try
            {
                textComponent.text = string.Format(localizedText, formatArgs);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur de formatage pour '{localizationKey}': {e.Message}");
                textComponent.text = localizedText;
            }
        }
        else
        {
            textComponent.text = localizedText;
        }
    }

    /// <summary>
    /// Change la clé de localisation et met à jour le texte
    /// </summary>
    public void SetKey(string newKey)
    {
        localizationKey = newKey;
        UpdateText();
    }

    /// <summary>
    /// Retourne la clé actuelle
    /// </summary>
    public string GetKey()
    {
        return localizationKey;
    }

    /// <summary>
    /// Définit les arguments de formatage (pour textes avec {0}, {1}, etc.)
    /// Exemple: "Score: {0}" avec SetFormatArgs(100) donnera "Score: 100"
    /// </summary>
    public void SetFormatArgs(params object[] args)
    {
        formatArgs = args;
        useFormatting = true;
        UpdateText();
    }

    /// <summary>
    /// Réinitialise le formatage
    /// </summary>
    public void ClearFormatArgs()
    {
        formatArgs = null;
        useFormatting = false;
        UpdateText();
    }

#if UNITY_EDITOR
    // Preview dans l'éditeur
    private void OnValidate()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        // Afficher la clé dans l'éditeur si le jeu ne tourne pas
        if (!Application.isPlaying && !string.IsNullOrEmpty(localizationKey))
        {
            textComponent.text = $"[{localizationKey}]";
        }
    }
#endif
}