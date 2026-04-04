using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LanguageData", menuName = "Localization/Language Data")]
public class LanguageData : ScriptableObject
{
    [Serializable]
    public class LanguageEntry
    {
        public string key;
        public string russianText;
        public string englishText;
    }

    public enum Language
    {
        Russian,
        English
    }

    [SerializeField] private List<LanguageEntry> entries = new List<LanguageEntry>();

    private Dictionary<string, string> russianDictionary = new Dictionary<string, string>();
    private Dictionary<string, string> englishDictionary = new Dictionary<string, string>();

    private bool isInitialized = false;

    public void Initialize()
    {
        if (isInitialized) return;

        russianDictionary.Clear();
        englishDictionary.Clear();

        foreach (var entry in entries)
        {
            if (!string.IsNullOrEmpty(entry.key))
            {
                russianDictionary[entry.key] = entry.russianText;
                englishDictionary[entry.key] = entry.englishText;
            }
        }

        isInitialized = true;
    }

    public string GetText(string key, Language language)
    {
        if (!isInitialized) Initialize();

        switch (language)
        {
            case Language.Russian:
                return russianDictionary.TryGetValue(key, out string rusText) ? rusText : $"Missing: {key}";
            case Language.English:
                return englishDictionary.TryGetValue(key, out string engText) ? engText : $"Missing: {key}";
            default:
                return $"Missing: {key}";
        }
    }

    private void OnValidate()
    {
        isInitialized = false;
    }
}