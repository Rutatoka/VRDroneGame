using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string localizationKey;
    private Text uiText;
    private TextMeshProUGUI tmpText;
    private bool isInitialized = false;

    private void Awake()
    {
        FindTextComponent();
    }

    private void FindTextComponent()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText == null)
        {
            uiText = GetComponent<Text>();
        }    
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged.AddListener(OnLanguageChanged);
            isInitialized = true;
            UpdateText();
        }
        else
        {
            Invoke(nameof(TrySubscribe), 0.1f);
        }
    }

    private void TrySubscribe()
    {
        if (LocalizationManager.Instance != null && !isInitialized)
        {
            LocalizationManager.Instance.OnLanguageChanged.AddListener(OnLanguageChanged);
            isInitialized = true;
            UpdateText();
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null && isInitialized)
        {
            LocalizationManager.Instance.OnLanguageChanged.RemoveListener(OnLanguageChanged);
            isInitialized = false;
        }
    }

    private void OnLanguageChanged(LanguageData.Language language)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        string localizedText = LocalizationManager.Instance.GetText(localizationKey);
        LanguageData.Language currentLanguage = LocalizationManager.Instance.GetCurrentLanguage();
        if (tmpText != null)
        {
            tmpText.text = localizedText;
        }
        else if (uiText != null)
        {
            uiText.text = localizedText;
        }
    }

    public void SetKey(string newKey)
    {
        localizationKey = newKey;
        if (gameObject.activeInHierarchy && LocalizationManager.Instance != null)
        {
            UpdateText();
        }
    }

    public string GetKey()
    {
        return localizationKey;
    }
}