using TMPro;
using UnityEngine;

public class LevelTimerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private GameObject timerPanel;

    [Header("Format Settings")]
    [SerializeField] private string timerFormat = "{0:00}:{1:00}:{2:000}"; 
    [SerializeField] private string bestTimeFormat = "Лучшее время: {0:00}:{1:00}:{2:000}";

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color recordColor = Color.yellow;

    private float currentTime = 0f;
    private bool isTimerRunning = false;
    private float bestTime = 0f;
    private bool isNewRecord = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            if (SaveSlotManager.Instance?.currentSave != null)
            {
                bestTime = SaveSlotManager.Instance.currentSave.bestTime;
                UpdateBestTimeDisplay();
            }
            GameManager.Instance.StartLevel();
            StartTimer();
        }

    }

    private void Update()
    {
        if (isTimerRunning && GameManager.Instance != null && GameManager.Instance.IsLevelActive())
        {
        
            currentTime = GameManager.Instance.GetCurrentLevelTime();
            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        currentTime = 0f;
        isNewRecord = false;
      
        if (timerPanel != null) timerPanel.SetActive(true);

        Debug.Log("Таймер запущен!");
    }

    public void StopTimer()
    {
        isTimerRunning = false;

        if (bestTime == 0 || currentTime < bestTime)
        {
            isNewRecord = true;
            bestTime = currentTime;

            if (SaveSlotManager.Instance?.currentSave != null)
            {
                SaveSlotManager.Instance.currentSave.bestTime = bestTime;
                SaveSlotManager.Instance.SaveCurrentGame();
            }

            Debug.Log($"НОВЫЙ РЕКОРД! {FormatTime(currentTime)}");
        }

        UpdateBestTimeDisplay();
        Debug.Log($"Таймер остановлен. Время: {FormatTime(currentTime)}");
    }

    private void UpdateTimerDisplay()
    {
       
            string formattedTime = FormatTime(currentTime);
      
            timerText.text = formattedTime;

            if (isNewRecord && isTimerRunning == false)
            {
                timerText.color = recordColor;
            }
            else
            {
                timerText.color = normalColor;
            }
       
    }

    private void UpdateBestTimeDisplay()
    {
       
            if (bestTime > 0)
            {
                string formattedBestTime = FormatTime(bestTime);
                bestTimeText.text = string.Format(bestTimeFormat,
                    Mathf.FloorToInt(bestTime / 60),
                    Mathf.FloorToInt(bestTime % 60),
                    Mathf.FloorToInt((bestTime % 1) * 1000));
            }
            else
            {
                bestTimeText.text = "Лучшее время: --:--:---";
            }
        
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time % 1) * 1000);

        return string.Format(timerFormat, minutes, seconds, milliseconds);
    }

    public float GetCurrentTime() => currentTime;
    public bool IsNewRecord() => isNewRecord;
    public float GetBestTime() => bestTime;

    public void ShowFinalTime()
    {
        if (timerText != null)
        {
            string finalText = FormatTime(currentTime);
            if (isNewRecord)
            {
                timerText.text = $"НОВЫЙ РЕКОРД!\n{finalText}";
                timerText.color = recordColor;
            }
            else
            {
                timerText.text = $"Время: {finalText}";
            }
        }
    }
}