using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string droneSceneName = "drone1";
    [SerializeField] private string slotSelectionSceneName = "SlotSelectionScene";
    [SerializeField] private string tutorialSceneName = "TutorialScene";

    private TMP_Text score_txt;
    private float levelStartTime;
    private bool isLevelActive = false;
    private int currentLevelScore = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ========== ЕДИНЫЙ МЕТОД ДЛЯ ВСЕХ ПЕРЕХОДОВ ==========
    public void LoadScene(string sceneName)
    {
        Debug.Log($"=== GameManager: Загрузка сцены {sceneName} ===");

        // Сохраняем прогресс перед переходом
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            SaveSlotManager.Instance.SaveCurrentGame();
        }

        // Всегда через LoadingManager, если он есть
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("LoadingManager не найден! Загружаем напрямую");
            SceneManager.LoadScene(sceneName);
        }
    }

    // ========== КОРОТКИЕ МЕТОДЫ-ОБЁРТКИ (для удобства) ==========
    public void GoToMenu() => LoadScene(menuSceneName);
    public void LoadDroneScene() => LoadScene(droneSceneName);
    public void OpenSlotSelection() => LoadScene(slotSelectionSceneName);
    public void OpenTutorial() => LoadScene(tutorialSceneName);

    // ========== ОСТАЛЬНЫЕ МЕТОДЫ ==========

    public void StartLevel()
    {
        isLevelActive = true;
        levelStartTime = Time.time;
        currentLevelScore = 0;
        UpdateUI();
        Debug.Log("Уровень начат!");
    }

    public void CompleteLevel(int batteryBonusScore)
    {
        if (!isLevelActive) return;
        AudioManager.Instance?.PlayVictory();
        isLevelActive = false;

        float levelTime = Time.time - levelStartTime;
        int totalLevelScore = currentLevelScore + batteryBonusScore;

        if (SaveSlotManager.Instance?.currentSave != null)
        {
            var save = SaveSlotManager.Instance.currentSave;

            if (levelTime < save.bestTime || save.bestTime == 0)
            {
                save.bestTime = levelTime;
                Debug.Log($"Новый рекорд времени! {levelTime:F1} сек");
            }

            if (totalLevelScore > save.totalScore)
            {
                save.totalScore = totalLevelScore;
                Debug.Log($"НОВЫЙ РЕКОРД ОЧКОВ! {totalLevelScore}");
            }

            SaveSlotManager.Instance.SaveCurrentGame();
        }

        Debug.Log($"Уровень пройден! Время: {levelTime:F1} сек");

        OpenSlotSelection();
    }

    public void DroneDeath(string reason)
    {
        if (!isLevelActive) return;
        isLevelActive = false;

        LevelTimerUI timer = FindObjectOfType<LevelTimerUI>();
        if (timer != null) timer.StopTimer();

        Debug.Log($"Дрон погиб! Причина: {reason}");
        OpenSlotSelection();
    }

    public void AddScore(int points)
    {
        AudioManager.Instance?.PlayCoinPickup();
        currentLevelScore += points;
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.name == droneSceneName)
            {
                isLevelActive = false;
                GoToMenu();
            }
            else if (activeScene.name == menuSceneName)
            {
                QuitGame();
            }
            else
            {
                GoToMenu();
            }
        }
    }

    public void EventChecked(string nameEvent)
    {
        GameObject wallPath = GameObject.Find("wallPath");
        if (wallPath != null)
            wallPath.SetActive(nameEvent != "EventObstacle");
    }

    public void UpdateUI()
    {
        GameObject scoreObject = GameObject.Find("scoreText");
        if (scoreObject != null)
        {
            score_txt = scoreObject.GetComponent<TMP_Text>();
            if (score_txt != null)
                score_txt.text = "Очки: " + currentLevelScore;
        }
    }

    public int GetHighScore() => SaveSlotManager.Instance?.currentSave?.totalScore ?? 0;
    public float GetCurrentLevelTime() => isLevelActive ? Time.time - levelStartTime : 0;
    public bool IsLevelActive() => isLevelActive;

    public void QuitGame()
    {
        Debug.Log("Выход из игры");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}