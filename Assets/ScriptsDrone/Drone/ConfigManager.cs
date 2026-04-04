using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    private static ConfigManager _instance;
    public static ConfigManager Instance => _instance;
    
    [Header("Drone Configuration")]
    [SerializeField] private DroneConfig droneConfig;
    
    public DroneConfig DroneConfig => droneConfig;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (droneConfig == null)
            LoadConfig();
    }
    
    private void LoadConfig()
    {
        droneConfig = Resources.Load<DroneConfig>("DroneConfig");
        
        if (droneConfig == null)
        {
            Debug.LogWarning("DroneConfig not found! Creating default...");
            CreateDefaultConfig();
        }
    }
    
    private void CreateDefaultConfig()
    {
        droneConfig = ScriptableObject.CreateInstance<DroneConfig>();
        droneConfig.ResetToDefault();
        
        #if UNITY_EDITOR
        if (!System.IO.Directory.Exists(Application.dataPath + "/Resources"))
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
        
        UnityEditor.AssetDatabase.CreateAsset(droneConfig, "Assets/Resources/DroneConfig.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("Default config created!");
        #endif
    }
    
    public void UpdateConfig(DroneConfig newConfig) => droneConfig = newConfig;
}