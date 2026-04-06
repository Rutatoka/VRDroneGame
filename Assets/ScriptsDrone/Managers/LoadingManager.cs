using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("Settings")]
    [SerializeField] private string loadingSceneName = "LoadingScene";

    [Header("Loading Tips")]
    [TextArea]
    [SerializeField] private string[] loadingTips;

    [Header("Settings")]
    [SerializeField] private float minLoadTime = 1f;
    [SerializeField] private bool showRandomTip = true;

    // Внутренние переменные
    private string targetSceneName;
    private bool isLoading = false;

    // UI элементы (найдутся в LoadingScene)
    private Slider progressSlider;
    private TextMeshProUGUI loadingTipText;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI sceneNameText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LoadingManager инициализирован");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"=== LOADING MANAGER: Загрузка сцены {sceneName} ===");

        if (isLoading)
        {
            Debug.LogWarning("Уже идёт загрузка, игнорирую");
            return;
        }

        targetSceneName = sceneName;
        isLoading = true;

        // Загружаем сцену загрузки
        SceneManager.LoadScene(loadingSceneName);
    }

    // Этот метод вызывается из LoadingScene
    public void StartLoadingTargetScene()
    {
        Debug.Log($"=== START LOADING: Целевая сцена = {targetSceneName} ===");

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("targetSceneName пуст! Нечего загружать!");
            return;
        }

        StartCoroutine(LoadTargetSceneCoroutine());
    }

    IEnumerator LoadTargetSceneCoroutine()
    {
        // Ждём один кадр для отрисовки UI
        yield return null;

        // Находим UI элементы
        FindLoadingUI();

        // Показываем подсказку
        if (showRandomTip && loadingTipText != null && loadingTips.Length > 0)
        {
            int randomTip = Random.Range(0, loadingTips.Length);
            loadingTipText.text = loadingTips[randomTip];
        }

        // Показываем название сцены
        if (sceneNameText != null)
        {
            sceneNameText.text = GetSceneDisplayName(targetSceneName);
        }

        // Сбрасываем ползунок
        if (progressSlider != null)
            progressSlider.value = 0f;

        if (progressText != null)
            progressText.text = "0%";

        Debug.Log("Начинаем асинхронную загрузку...");

        float startTime = Time.time;

        // Асинхронная загрузка целевой сцены
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(targetSceneName);
        asyncOp.allowSceneActivation = false;

        // Ждём загрузки
        while (asyncOp.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
            UpdateProgressUI(progress);
            Debug.Log($"Прогресс загрузки: {progress * 100:F0}%");
            yield return null;
        }

        UpdateProgressUI(1f);
        Debug.Log("Загрузка завершена на 90%");

        // Минимальное время показа
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minLoadTime)
        {
            Debug.Log($"Ждём ещё {minLoadTime - elapsedTime:F1} секунд");
            yield return new WaitForSeconds(minLoadTime - elapsedTime);
        }

        // Активируем сцену
        Debug.Log("Активируем целевую сцену...");
        asyncOp.allowSceneActivation = true;

        while (!asyncOp.isDone)
        {
            yield return null;
        }

        Debug.Log("Сцена загружена!");
        isLoading = false;
    }

    void FindLoadingUI()
    {
        Debug.Log("Поиск UI элементов в LoadingScene...");

        // Ищем слайдер
        GameObject sliderObj = GameObject.Find("ProgressSlider");
        if (sliderObj != null)
        {
            progressSlider = sliderObj.GetComponent<Slider>();
            Debug.Log("ProgressSlider найден!");
        }
        else
        {
            Debug.LogWarning("ProgressSlider не найден! Проверь имя объекта в LoadingScene");
        }

        // Ищем текст процентов
        GameObject percentObj = GameObject.Find("ProgressText");
        if (percentObj != null)
        {
            progressText = percentObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("ProgressText найден!");
        }

        // Ищем текст подсказки
        GameObject tipObj = GameObject.Find("LoadingTipText");
        if (tipObj != null)
        {
            loadingTipText = tipObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("LoadingTipText найден!");
        }

        // Ищем название сцены
        GameObject sceneNameObj = GameObject.Find("SceneNameText");
        if (sceneNameObj != null)
        {
            sceneNameText = sceneNameObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("SceneNameText найден!");
        }
    }

    void UpdateProgressUI(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = progress;

            // ПРИНУДИТЕЛЬНОЕ ОБНОВЛЕНИЕ UI
            LayoutRebuilder.ForceRebuildLayoutImmediate(progressSlider.GetComponent<RectTransform>());

            Debug.Log($"Слайдер обновлён: {progress * 100:F0}%, текущее значение: {progressSlider.value}");
        }

        if (progressText != null)
        {
            progressText.text = $"{(progress * 100):F0}%";
        }
    }

    string GetSceneDisplayName(string sceneName)
    {
        switch (sceneName)
        {
            case "Menu": return "ГЛАВНОЕ МЕНЮ";
            case "SlotSelectionScene": return "ВЫБОР ПИЛОТА";
            case "TutorialScene": return "ОБУЧЕНИЕ";
            case "drone1": return "ПОЛЁТ";
            default: return sceneName.ToUpper();
        }
    }

    // Сброс состояния (на всякий случай)
    public void ResetLoading()
    {
        isLoading = false;
        targetSceneName = "";
    }
}