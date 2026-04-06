using UnityEngine;
using System.IO;
using System.Text;

public class SaveSlotManager : MonoBehaviour
{
    public static SaveSlotManager Instance;

    [Header("Настройки слотов")]
    public int currentSlot = 0;
    public string[] slotNames = { "Пилот 1", "Пилот 2", "Пилот 3" };

    private string saveFolderPath;
    private byte xorKey = 0xAA;
    public SaveData currentSave;
    private float totalGameTimer = 0f;

    private float autoSaveTimer = 0f;
    public float autoSaveInterval = 30f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("SaveSlotManager уже существует, уничтожаем дубликат");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        saveFolderPath = Path.Combine(Application.persistentDataPath, "DroneSaves");

        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }

        Debug.Log("SaveSlotManager инициализирован. Путь: " + saveFolderPath);

        LoadLastUsedSlot();
    }

    void LoadLastUsedSlot()
    {
        int lastSlot = PlayerPrefs.GetInt("LastUsedSlot", 0);
        SwitchToSlot(lastSlot);
    }

    public string GetSlotFilePath(int slotIndex)
    {
        return Path.Combine(saveFolderPath, $"slot_{slotIndex}.dat");
    }

    public bool HasSaveInSlot(int slotIndex)
    {
        return File.Exists(GetSlotFilePath(slotIndex));
    }

    public SaveData LoadGame(int slotIndex)
    {
        string filePath = GetSlotFilePath(slotIndex);

        if (!File.Exists(filePath))
        {
            Debug.Log($"Слот {slotIndex + 1} пуст. Создаём новое сохранение.");
            return CreateNewSave(slotIndex);
        }

        try
        {
            byte[] encrypted = File.ReadAllBytes(filePath);
            string json = Decrypt(encrypted);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"Загружен слот {slotIndex + 1}: {data.profileName}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка загрузки: {e.Message}");
            return CreateNewSave(slotIndex);
        }
    }

    public void SaveGame(int slotIndex, SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        byte[] encrypted = Encrypt(json);
        File.WriteAllBytes(GetSlotFilePath(slotIndex), encrypted);
        Debug.Log($"Сохранён слот {slotIndex + 1}");
    }

    public void SaveCurrentGame()
    {
        if (currentSave == null)
        {
            Debug.LogError("Нет активного сохранения!");
            return;
        }

        // Сохраняем ВСЕ настройки из ConfigManager
        if (ConfigManager.Instance?.DroneConfig != null)
        {
            var config = ConfigManager.Instance.DroneConfig;
            currentSave.droneSettings.maxSpeed = config.maxSpeed;
            currentSave.droneSettings.acceleration = config.acceleration;
            currentSave.droneSettings.rotationSpeed = config.rotationSpeed;
            currentSave.droneSettings.drag = config.drag;
            currentSave.droneSettings.angularDrag = config.angularDrag;
            currentSave.droneSettings.hoverHeight = config.hoverHeight;
            currentSave.droneSettings.hoverForce = config.hoverForce;
            currentSave.droneSettings.hoverStability = config.hoverStability;
            currentSave.droneSettings.batteryLife = config.batteryLife;
            currentSave.droneSettings.obstaclePenalty = config.obstaclePenalty;
            currentSave.droneSettings.modelTiltAmount = config.modelTiltAmount;

            Debug.Log("Настройки дрона сохранены в профиль");
        }

        SaveGame(currentSlot, currentSave);
    }

    public void SwitchToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 2)
        {
            Debug.LogError("Неверный номер слота!");
            return;
        }

        currentSlot = slotIndex;
        currentSave = LoadGame(slotIndex);

        ApplySaveToConfig();

        PlayerPrefs.SetInt("LastUsedSlot", slotIndex);
        PlayerPrefs.Save();

        Debug.Log($"Переключились на {slotNames[slotIndex]}");

        OnProfileChanged?.Invoke(currentSave);
    }

    void ApplySaveToConfig()
    {
        if (ConfigManager.Instance?.DroneConfig != null && currentSave != null)
        {
            var config = ConfigManager.Instance.DroneConfig;
            var settings = currentSave.droneSettings;

            config.maxSpeed = settings.maxSpeed;
            config.acceleration = settings.acceleration;
            config.rotationSpeed = settings.rotationSpeed;
            config.drag = settings.drag;
            config.angularDrag = settings.angularDrag;
            config.hoverHeight = settings.hoverHeight;
            config.hoverForce = settings.hoverForce;
            config.hoverStability = settings.hoverStability;
            config.batteryLife = settings.batteryLife;
            config.obstaclePenalty = settings.obstaclePenalty;
            config.modelTiltAmount = settings.modelTiltAmount;

            Debug.Log("Настройки дрона загружены из профиля");
        }
    }

    private SaveData CreateNewSave(int slotIndex)
    {
        SaveData newSave = new SaveData();
        newSave.profileName = slotNames[slotIndex];
        newSave.unlockedLevels.Add("Level_1");

        if (ConfigManager.Instance?.DroneConfig != null)
        {
            newSave.droneSettings.maxSpeed = ConfigManager.Instance.DroneConfig.maxSpeed;
            newSave.droneSettings.acceleration = ConfigManager.Instance.DroneConfig.acceleration;
            newSave.droneSettings.rotationSpeed = ConfigManager.Instance.DroneConfig.rotationSpeed;
            newSave.droneSettings.batteryLife = ConfigManager.Instance.DroneConfig.batteryLife;
            newSave.droneSettings.obstaclePenalty = ConfigManager.Instance.DroneConfig.obstaclePenalty;
        }

        SaveGame(slotIndex, newSave);
        return newSave;
    }

    public void DeleteSave(int slotIndex)
    {
        string filePath = GetSlotFilePath(slotIndex);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Удалён слот {slotIndex + 1}");
        }

        if (currentSlot == slotIndex)
        {
            currentSave = CreateNewSave(slotIndex);
        }
    }
    private byte[] Encrypt(string plainText)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        for (int i = 0; i < plainBytes.Length; i++)
        {
            plainBytes[i] ^= xorKey;
        }
        return plainBytes;
    }

    private string Decrypt(byte[] encryptedBytes)
    {
        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            encryptedBytes[i] ^= xorKey;
        }
        return Encoding.UTF8.GetString(encryptedBytes);
    }

    void Update()
    {
        if (currentSave != null)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval)
            {
                SaveCurrentGame();
                autoSaveTimer = 0f;
            }
            totalGameTimer += Time.deltaTime;
            if (totalGameTimer >= 1f)
            {
                int secondsToAdd = Mathf.FloorToInt(totalGameTimer);
                currentSave.playTime += secondsToAdd;
                totalGameTimer -= secondsToAdd;

                if (currentSave.playTime % 10 == 0)
                {
                    SaveCurrentGame();
                }
            }
        }
    }
    [ContextMenu("Debug/Show Current Config")]
    public void ShowCurrentConfig()
    {
        if (currentSave != null)
        {
            var s = currentSave.droneSettings;
            Debug.Log($"=== ТЕКУЩИЕ НАСТРОЙКИ В ПРОФИЛЕ ===\n" +
                      $"MaxSpeed: {s.maxSpeed}\n" +
                      $"Acceleration: {s.acceleration}\n" +
                      $"RotationSpeed: {s.rotationSpeed}\n" +
                      $"BatteryLife: {s.batteryLife}\n" +
                      $"ObstaclePenalty: {s.obstaclePenalty}\n" +
                      $"Drag: {s.drag}\n" +
                      $"HoverHeight: {s.hoverHeight}\n" +
                      $"HoverForce: {s.hoverForce}");
        }
        else
        {
            Debug.Log("currentSave is null!");
        }
    }

    public static System.Action<SaveData> OnProfileChanged;
}