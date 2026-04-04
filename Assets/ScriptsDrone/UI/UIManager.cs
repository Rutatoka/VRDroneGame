using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
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
        LoadConfigToUI();

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveConfig);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetConfig);

        if (backButton != null)
            backButton.onClick.AddListener(BackToMenu);
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
    }

    private void SaveConfig()
    {
        if (ConfigManager.Instance?.DroneConfig == null) return;
        var config = ConfigManager.Instance.DroneConfig;

        if (speedInput != null && float.TryParse(speedInput.text, out float speed)) config.maxSpeed = speed;
        if (accelerationInput != null && float.TryParse(accelerationInput.text, out float accel)) config.acceleration = accel;
        if (rotationInput != null && float.TryParse(rotationInput.text, out float rotation)) config.rotationSpeed = rotation;
        if (batteryInput != null && float.TryParse(batteryInput.text, out float battery)) config.batteryLife = battery;
        if (penaltyInput != null && float.TryParse(penaltyInput.text, out float penalty)) config.obstaclePenalty = penalty;

        UpdateDisplayTexts();
        Debug.Log("Configuration saved!");

        if (SaveSlotManager.Instance?.currentSave != null)
        {
            SaveSlotManager.Instance.SaveCurrentGame();
        }
    }

    private void ResetConfig()
    {
        if (ConfigManager.Instance?.DroneConfig == null) return;
        ConfigManager.Instance.DroneConfig.ResetToDefault();
        LoadConfigToUI();
        Debug.Log("Configuration reset to default!");
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
        if (GameManager.Instance != null)
            GameManager.Instance.OpenSlotSelection();
        else
            SceneManager.LoadScene(0);
    }
}