using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI configText;
    [SerializeField] private TextMeshProUGUI speedValueText;
    [SerializeField] private TextMeshProUGUI accelerationValueText;
    [SerializeField] private TextMeshProUGUI rotationValueText;
    [SerializeField] private TextMeshProUGUI batteryValueText;
    [SerializeField] private TextMeshProUGUI penaltyValueText;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField speedInput;
    [SerializeField] private TMP_InputField accelerationInput;
    [SerializeField] private TMP_InputField rotationInput;
    [SerializeField] private TMP_InputField batteryInput;
    [SerializeField] private TMP_InputField penaltyInput;

    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backButton;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        LoadFromCurrentProfile();
        LoadConfigToUI();
        AudioManager.Instance?.PlayMenuMusic();

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveConfig);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetConfig);

        if (backButton != null)
            backButton.onClick.AddListener(BackToMenu);
        SaveSlotManager.OnProfileChanged += OnProfileChanged;
    }
    private void OnProfileChanged(SaveData data)
    {
        Debug.Log($"UIManager: Профиль изменён на {data.profileName}");
              LoadFromCurrentProfile();
        LoadConfigToUI();
    }
    private void LoadFromCurrentProfile()
    {
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            var settings = SaveSlotManager.Instance.currentSave.droneSettings;

            // Применяем к ConfigManager
            if (ConfigManager.Instance?.DroneConfig != null)
            {
                var config = ConfigManager.Instance.DroneConfig;
                config.maxSpeed = settings.maxSpeed;
                config.acceleration = settings.acceleration;
                config.rotationSpeed = settings.rotationSpeed;
                config.batteryLife = settings.batteryLife;
                config.obstaclePenalty = settings.obstaclePenalty;
                config.drag = settings.drag;
                config.angularDrag = settings.angularDrag;
                config.hoverHeight = settings.hoverHeight;
                config.hoverForce = settings.hoverForce;
                config.hoverStability = settings.hoverStability;
                config.modelTiltAmount = settings.modelTiltAmount;

                Debug.Log("Настройки загружены из профиля в ConfigManager");
            }
        }
        else
        {
            Debug.Log("Нет активного профиля для загрузки настроек");
        }
    }
    private void LoadConfigToUI()
    {
        if (ConfigManager.Instance?.DroneConfig == null) return;

        var config = ConfigManager.Instance.DroneConfig;

        if (speedInput != null) speedInput.text = config.maxSpeed.ToString();
        if (accelerationInput != null) accelerationInput.text = config.acceleration.ToString();
        if (rotationInput != null) rotationInput.text = config.rotationSpeed.ToString();
        if (batteryInput != null) batteryInput.text = config.batteryLife.ToString();
        if (penaltyInput != null) penaltyInput.text = config.obstaclePenalty.ToString();

        UpdateDisplayTexts();
        Debug.Log("UI обновлён из ConfigManager");
    }

    private void SaveConfig()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (ConfigManager.Instance?.DroneConfig == null) return;

        var config = ConfigManager.Instance.DroneConfig;

        if (float.TryParse(speedInput.text, out float speed)) config.maxSpeed = speed;
        if (float.TryParse(accelerationInput.text, out float accel)) config.acceleration = accel;
        if (float.TryParse(rotationInput.text, out float rotation)) config.rotationSpeed = rotation;
        if (float.TryParse(batteryInput.text, out float battery)) config.batteryLife = battery;
        if (float.TryParse(penaltyInput.text, out float penalty)) config.obstaclePenalty = penalty;

        UpdateDisplayTexts();

        // Сохраняем в текущий профиль
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            SaveSlotManager.Instance.SaveCurrentGame();
            Debug.Log("Настройки сохранены в профиль!");
        }
        else
        {
            Debug.LogWarning("Нет активного профиля для сохранения!");
        }
    }

    private void ResetConfig()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (ConfigManager.Instance?.DroneConfig == null) return;

        ConfigManager.Instance.DroneConfig.ResetToDefault();
        LoadConfigToUI();

        // Сохраняем сброшенные настройки в профиль
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            SaveSlotManager.Instance.SaveCurrentGame();
            Debug.Log("Настройки сброшены и сохранены в профиль!");
        }
    }

    private void UpdateDisplayTexts()
    {
        var config = ConfigManager.Instance.DroneConfig;
        if (config == null) return;

        if (speedValueText != null) speedValueText.text = $"{config.maxSpeed} m/s";
        if (accelerationValueText != null) accelerationValueText.text = $"{config.acceleration} m/s²";
        if (rotationValueText != null) rotationValueText.text = $"{config.rotationSpeed} °/s";
        if (batteryValueText != null) batteryValueText.text = $"{config.batteryLife} sec";
        if (penaltyValueText != null) penaltyValueText.text = $"{config.obstaclePenalty}";
    }

    private void BackToMenu()
    {

        AudioManager.Instance?.PlayButtonClick();
        if (SaveSlotManager.Instance?.currentSave != null)
        {
            SaveSlotManager.Instance.SaveCurrentGame();
        }
        if (GameManager.Instance != null)
            GameManager.Instance.OpenSlotSelection();
        else
            SceneManager.LoadScene(0);
    }
    private void OnDestroy()
    {
        SaveSlotManager.OnProfileChanged -= OnProfileChanged;
    }
}