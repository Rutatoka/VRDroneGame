using UnityEngine;
using UnityEngine.Events;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Language Data")]
    [SerializeField] private LanguageData languageData;

    [Header("Settings")]
    [SerializeField] private LanguageData.Language currentLanguage = LanguageData.Language.Russian;

    [Header("Events")]
    public UnityEvent<LanguageData.Language> OnLanguageChanged = new UnityEvent<LanguageData.Language>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (languageData != null)
        {
            languageData.Initialize();
        }
    }

    private void Start()
    {
        SetLanguage(currentLanguage);
    }

    public string GetText(string key)
    {
        if (languageData == null)
        {
            return $"Error: {key}";
        }

        return languageData.GetText(key, currentLanguage);
    }

    public void SetLanguage(LanguageData.Language newLanguage)
    {
        if (currentLanguage == newLanguage) return;

        currentLanguage = newLanguage;
        OnLanguageChanged?.Invoke(currentLanguage);
    }
    public void ToggleLanguage()
    {
        if (currentLanguage == LanguageData.Language.Russian)
        {
            SetLanguage(LanguageData.Language.English);
        }
        else
        {
            SetLanguage(LanguageData.Language.Russian);
        }
    }
    public void SetRussianLanguage()
    {
        SetLanguage(LanguageData.Language.Russian);
    }
    public static void StaticToggleLanguage()
    {
        if (Instance != null)
            Instance.ToggleLanguage();
    }

  
    public void SetEnglishLanguage()
    {
        SetLanguage(LanguageData.Language.English);
    }
    public LanguageData.Language GetCurrentLanguage()
    {
        return currentLanguage;
    }
}