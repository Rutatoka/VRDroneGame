using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DroneController droneController;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private TextMeshProUGUI batteryText;
    [SerializeField] private Image batteryFillImage;

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color mediumColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;

    [Header("Settings")]
    [SerializeField] private float lowThreshold = 25f; 
    [SerializeField] private float mediumThreshold = 50f; 

    private void Start()
    {
        if (droneController == null)
            droneController = FindObjectOfType<DroneController>();
    }

    private void Update()
    {
        if (droneController == null) return;

        float batteryPercent = droneController.GetBatteryPercentage();
        if (batterySlider != null)
        {
            batterySlider.value = batteryPercent / 100f;
        }

        if (batteryText != null)
        {
            batteryText.text = $"Батарея: {batteryPercent:F0}%";
        }

        if (batteryFillImage != null)
        {
            if (batteryPercent <= lowThreshold)
                batteryFillImage.color = lowColor;
            else if (batteryPercent <= mediumThreshold)
                batteryFillImage.color = mediumColor;
            else
                batteryFillImage.color = fullColor;
        }
    }
}