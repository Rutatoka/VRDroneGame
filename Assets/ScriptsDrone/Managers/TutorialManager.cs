using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image illustrationImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button skipButton;

    [Header("Tutorial Pages")]
    [SerializeField] private TutorialPage[] tutorialPages;

    [Header("Navigation")]
    [SerializeField] private string nextSceneName = "drone1";
    [SerializeField] private string previousSceneName = "SlotSelectionScene";

    [Header("Settings")]
    [SerializeField] private bool showOnlyOnce = true;

    private int currentPageIndex = 0;

    void Start()
    {
        // Проверяем, нужно ли показывать обучение
        if (showOnlyOnce && HasCompletedTutorial())
        {
            SkipTutorial();
            return;
        }

        SetupButtons();
        ShowPage(0);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    void SetupButtons()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextPage);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(PreviousPage);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipTutorial);
        }
    }

    void ShowPage(int index)
    {
        currentPageIndex = index;

        if (tutorialPages == null || tutorialPages.Length == 0)
        {
            Debug.LogError("No tutorial pages assigned!");
            return;
        }

        if (currentPageIndex < 0 || currentPageIndex >= tutorialPages.Length)
            return;

        TutorialPage page = tutorialPages[currentPageIndex];

        if (titleText != null)
            titleText.text = page.title;

        if (descriptionText != null)
            descriptionText.text = page.description;

        if (illustrationImage != null && page.illustration != null)
            illustrationImage.sprite = page.illustration;

        // Обновляем состояние кнопок
        UpdateButtons();
        AnimatePageTransition();
        Debug.Log($"Showing tutorial page {currentPageIndex + 1}/{tutorialPages.Length}: {page.title}");
    }
    private void AnimatePageTransition()
    {
        if (tutorialPanel != null)
        {
            // Простая анимация с использованием корутины
            StartCoroutine(AnimatePanel());
        }
    }

    private IEnumerator AnimatePanel()
    {
        Vector3 originalScale = tutorialPanel.transform.localScale;
        tutorialPanel.transform.localScale = Vector3.one * 0.95f;

        float elapsed = 0;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Плавное возвращение к нормальному масштабу
            tutorialPanel.transform.localScale = Vector3.Lerp(Vector3.one * 0.95f, originalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        tutorialPanel.transform.localScale = originalScale;
    }
    void UpdateButtons()
    {
        // Кнопка "Назад" активна только не на первом слайде
        if (backButton != null)
            backButton.gameObject.SetActive(currentPageIndex > 0);

        // Кнопка "Далее" или "Начать"
        if (nextButton != null)
        {
            bool isLastPage = currentPageIndex >= tutorialPages.Length - 1;

            // Меняем текст кнопки
            TextMeshProUGUI buttonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isLastPage ? "НАЧАТЬ" : "ДАЛЕЕ";
            }

            // Меняем цвет кнопки на последней странице
            Image buttonImage = nextButton.GetComponent<Image>();
            if (buttonImage != null && isLastPage)
            {
                buttonImage.color = Color.green;
            }
            else if (buttonImage != null)
            {
                buttonImage.color = Color.white;
            }
        }
    }

    public void NextPage()
    {
        // Если есть AudioManager - добавь звук
        // AudioManager.Instance?.PlayButtonClick();

        bool isLastPage = currentPageIndex >= tutorialPages.Length - 1;

        if (isLastPage)
        {
            CompleteTutorial();
        }
        else
        {
            ShowPage(currentPageIndex + 1);
        }
    }

    public void PreviousPage()
    {
        // AudioManager.Instance?.PlayButtonClick();

        if (currentPageIndex > 0)
        {
            ShowPage(currentPageIndex - 1);
        }
    }

    public void SkipTutorial()
    {
        // AudioManager.Instance?.PlayButtonClick();
        Debug.Log("Tutorial skipped!");
        MarkTutorialAsCompleted();
        LoadNextScene();
    }

    void CompleteTutorial()
    {
        Debug.Log("Tutorial completed!");
        MarkTutorialAsCompleted();
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // Сохраняем прогресс перед переходом
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            SaveSlotManager.Instance.SaveCurrentGame();
        }

        Debug.Log($"Загрузка следующей сцены: {nextSceneName} через LoadingManager");

        // ВСЕГДА используем LoadingManager
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }

    void MarkTutorialAsCompleted()
    {
        // Сохраняем в PlayerPrefs (простое решение)
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        // ИЛИ сохраняем в профиль игрока
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            if (!SaveSlotManager.Instance.currentSave.unlockedLevels.Contains("TutorialCompleted"))
            {
                SaveSlotManager.Instance.currentSave.unlockedLevels.Add("TutorialCompleted");
                SaveSlotManager.Instance.SaveCurrentGame();
            }
        }
    }

    bool HasCompletedTutorial()
    {
        // Проверяем в PlayerPrefs
        if (PlayerPrefs.HasKey("TutorialCompleted"))
            return true;

        // ИЛИ проверяем в профиле
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            return SaveSlotManager.Instance.currentSave.unlockedLevels.Contains("TutorialCompleted");
        }

        return false;
    }

    public void GoBack()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (LoadingManager.Instance != null)
            LoadingManager.Instance.LoadScene("SlotSelectionScene");
        else
            SceneManager.LoadScene("SlotSelectionScene");
    }
}

[System.Serializable]
public class TutorialPage
{
    public string title;
    [TextArea(3, 5)]
    public string description;
    public Sprite illustration;
}