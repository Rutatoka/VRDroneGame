using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SlotSelectorUI : MonoBehaviour
{
    [Header("Кнопки слотов")]
    public Button slot1Button;
    public Button slot2Button;
    public Button slot3Button;

    [Header("Информация о слотах")]
    public TextMeshProUGUI slot1Info;
    public TextMeshProUGUI slot2Info;
    public TextMeshProUGUI slot3Info;

    [Header("Кнопки действий")]
    public Button playButton;
    public Button[] deleteButtons; 
    public Button backButton;

    [Header("Настройки")]
    public string gameSceneName = "drone1";

    private int selectedSlot = 0;

    void Start()
    {
        Debug.Log("SlotSelectorUI Start - проверка SaveSlotManager");

        if (SaveSlotManager.Instance == null)
        {
            Debug.LogError("SaveSlotManager.Instance = null! Ищу в сцене...");
            SaveSlotManager existing = FindObjectOfType<SaveSlotManager>();
            if (existing != null)
            {
                Debug.Log("Найден существующий SaveSlotManager");
            }
            else
            {
                Debug.LogError("SaveSlotManager не найден! Создаю...");
                GameObject saveManager = new GameObject("SaveSlotManager");
                saveManager.AddComponent<SaveSlotManager>();
            }
        }
        else
        {
            Debug.Log("SaveSlotManager найден!");
        }

        SetupButtons();
        UpdateAllSlotsInfo();
    }

    void SetupButtons()
    {

        if (slot1Button != null)
        {
            slot1Button.onClick.RemoveAllListeners();
            slot1Button.onClick.AddListener(() => SelectSlot(0));
            Debug.Log("Кнопка слота 1 настроена");
        }

        if (slot2Button != null)
        {
            slot2Button.onClick.RemoveAllListeners();
            slot2Button.onClick.AddListener(() => SelectSlot(1));
            Debug.Log("Кнопка слота 2 настроена");
        }

        if (slot3Button != null)
        {
            slot3Button.onClick.RemoveAllListeners();
            slot3Button.onClick.AddListener(() => SelectSlot(2));
            Debug.Log("Кнопка слота 3 настроена");
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGame);
            Debug.Log("Кнопка Play настроена");
        }

   
        if (deleteButtons != null && deleteButtons.Length >= 3)
        {
            for (int i = 0; i < deleteButtons.Length && i < 3; i++)
            {
                int slotIndex = i; 
                if (deleteButtons[i] != null)
                {
                    deleteButtons[i].onClick.RemoveAllListeners();
                    deleteButtons[i].onClick.AddListener(() => DeleteSlot(slotIndex));
                    Debug.Log($"Кнопка Delete для слота {i + 1} настроена");
                }
            }
        }
        else
        {
            Debug.LogWarning("deleteButtons array is not properly assigned!");
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
            Debug.Log("Кнопка Back настроена");
        }

        SaveSlotManager.OnProfileChanged += OnProfileChanged;
    }

    void SelectSlot(int slotIndex)
    {
        Debug.Log($"Выбран слот {slotIndex + 1}");
        selectedSlot = slotIndex;

        if (SaveSlotManager.Instance == null)
        {
            Debug.LogError("SaveSlotManager.Instance = null!");
            return;
        }

        SaveSlotManager.Instance.SwitchToSlot(slotIndex);
        UpdateAllSlotsInfo();
    }

    void DeleteSlot(int slotIndex)
    {
        Debug.Log($"Удаление слота {slotIndex + 1}");

        if (SaveSlotManager.Instance == null)
        {
            Debug.LogError("SaveSlotManager.Instance = null!");
            return;
        }

        SaveSlotManager.Instance.DeleteSave(slotIndex);
        UpdateAllSlotsInfo();

        if (selectedSlot == slotIndex)
        {
            selectedSlot = -1;
        }
    }

    void StartGame()
    {
        if (SaveSlotManager.Instance == null)
        {
            Debug.LogError("SaveSlotManager.Instance = null!");
            return;
        }

        if (selectedSlot >= 0)
        {
            SaveSlotManager.Instance.SwitchToSlot(selectedSlot);
        }

        SaveSlotManager.Instance.SaveCurrentGame();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadDroneScene();
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    void GoBack()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMenu();
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    void UpdateAllSlotsInfo()
    {
        if (SaveSlotManager.Instance == null)
        {
            Debug.LogError("Cannot update slots: SaveSlotManager.Instance is null");
            return;
        }

        UpdateSlotInfoText(0, slot1Info);
        UpdateSlotInfoText(1, slot2Info);
        UpdateSlotInfoText(2, slot3Info);
    }

    void OnProfileChanged(SaveData data)
    {
        Debug.Log($"Профиль изменён: {data.profileName}");
        UpdateAllSlotsInfo();
    }

    void UpdateSlotInfoText(int slotIndex, TextMeshProUGUI infoText)
    {
        if (infoText == null) return;
        if (SaveSlotManager.Instance == null) return;

        if (SaveSlotManager.Instance.HasSaveInSlot(slotIndex))
        {
            SaveData data = SaveSlotManager.Instance.LoadGame(slotIndex);
            int hours = data.playTime / 3600;
            int minutes = (data.playTime % 3600) / 60;

            infoText.text = $"<b>{data.profileName}</b>\n" +
                           $" Рекорд: {data.bestTime:F1} сек\n" +
                           $" Очки: {data.totalScore}\n" +
                           $" В игре: {hours:00}:{minutes:00}";
        }
        else
        {
            infoText.text = $"<b>Слот {slotIndex + 1}</b>\n" +
                           "━━━━━━━━━━\n" +
                           "<color=#888888>Новый профиль</color>";
        }
    }

    void OnDestroy()
    {
        SaveSlotManager.OnProfileChanged -= OnProfileChanged;
    }
}