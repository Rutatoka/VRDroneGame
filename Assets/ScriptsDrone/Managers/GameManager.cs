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
            else
            {
                Debug.Log($"Очки за уровень: {totalLevelScore}. Рекорд: {save.totalScore} (не побит)");
            }

            SaveSlotManager.Instance.SaveCurrentGame();
        }

        Debug.Log($"Уровень пройден! Время: {levelTime:F1} сек, Очки за уровень: {totalLevelScore}");

        OpenSlotSelection();
    }

    public void DroneDeath(string reason)
    {
        if (!isLevelActive) return;

        isLevelActive = false;

        LevelTimerUI timer = FindObjectOfType<LevelTimerUI>();
        if (timer != null)
        {
            timer.StopTimer();
        }

        Debug.Log($"Дрон погиб! Причина: {reason}");

        OpenSlotSelection();
    }

    public void AddScore(int points)
    {
        AudioManager.Instance?.PlayCoinPickup();

        currentLevelScore += points;
        UpdateUI();
        Debug.Log($"Добавлено {points} очков. Всего за уровень: {currentLevelScore}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.name == droneSceneName)
            {

                if (SaveSlotManager.Instance?.currentSave != null)
                {
                    SaveSlotManager.Instance.SaveCurrentGame();
                }
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

        switch (nameEvent)
        {
            case "EventObstacle":
                if (wallPath != null) wallPath.SetActive(false);
                Debug.Log("fff");
                break;
            default:
                if (wallPath != null) wallPath.SetActive(true);
                break;
        }
    }

    public void UpdateUI()
    {
        GameObject scoreObject = GameObject.Find("scoreText");
        if (scoreObject != null)
        {
            score_txt = scoreObject.GetComponent<TMP_Text>();
            if (score_txt != null)
            {
                score_txt.text = "Очки: " + currentLevelScore;
            }
        }
    }

    public int GetHighScore()
    {
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            return SaveSlotManager.Instance.currentSave.totalScore;
        }
        return 0;
    }

    public float GetCurrentLevelTime()
    {
        if (!isLevelActive) return 0;
        return Time.time - levelStartTime;
    }

    public bool IsLevelActive() => isLevelActive;

    public void GoToMenu()
    {
        Debug.Log("Переход в меню");
        SceneManager.LoadScene(menuSceneName);
    }

    public void LoadDroneScene()
    {
        Debug.Log("Загрузка сцены с дроном");
        SceneManager.LoadScene(droneSceneName);
    }

    public void OpenSlotSelection()
    {
        Debug.Log("Открытие выбора слота");
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            SaveSlotManager.Instance.SaveCurrentGame();
        }
        SceneManager.LoadScene(slotSelectionSceneName);
    }

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