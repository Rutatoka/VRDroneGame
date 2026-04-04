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

        if (ConfigManager.Instance?.DroneConfig != null)
        {
            currentSave.droneSettings.maxSpeed = ConfigManager.Instance.DroneConfig.maxSpeed;
            currentSave.droneSettings.acceleration = ConfigManager.Instance.DroneConfig.acceleration;
            currentSave.droneSettings.rotationSpeed = ConfigManager.Instance.DroneConfig.rotationSpeed;
            currentSave.droneSettings.batteryLife = ConfigManager.Instance.DroneConfig.batteryLife;
            currentSave.droneSettings.obstaclePenalty = ConfigManager.Instance.DroneConfig.obstaclePenalty;
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
            ConfigManager.Instance.DroneConfig.maxSpeed = currentSave.droneSettings.maxSpeed;
            ConfigManager.Instance.DroneConfig.acceleration = currentSave.droneSettings.acceleration;
            ConfigManager.Instance.DroneConfig.rotationSpeed = currentSave.droneSettings.rotationSpeed;
            ConfigManager.Instance.DroneConfig.batteryLife = currentSave.droneSettings.batteryLife;
            ConfigManager.Instance.DroneConfig.obstaclePenalty = currentSave.droneSettings.obstaclePenalty;
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
        }
    }

    public static System.Action<SaveData> OnProfileChanged;
}