using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private OrbitCamera droneCamera;

    [Header("Settings")]
    [SerializeField] private KeyCode switchKey = KeyCode.C;
    [SerializeField] private bool startWithPlayerCamera = true;

    [Header("UI")]
    [SerializeField] private GameObject switchPrompt;

    private bool isDroneCameraActive = false;
    private Transform playerTransform;
    private Transform droneTransform;
    private DroneController droneController;
    private XROrigin xrOrigin;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Сцена загружена: {scene.name}");
        Invoke(nameof(FindAndSetupCameras), 0.1f);
    }

    private void Start()
    {
        FindAndSetupCameras();
    }

    private void FindAndSetupCameras()
    {
        Debug.Log("Поиск камер...");

        xrOrigin = FindObjectOfType<XROrigin>();

        if (playerCamera == null)
        {
            if (xrOrigin != null)
            {
                playerCamera = xrOrigin.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    Debug.Log($"Player camera found: {playerCamera.name}");
                }
            }

            if (playerCamera == null)
            {
                GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
                if (mainCam != null)
                {
                    playerCamera = mainCam.GetComponent<Camera>();
                    Debug.Log($"Main camera found by tag: {playerCamera.name}");
                }
            }
        }

        DroneController drone = FindObjectOfType<DroneController>();
        if (drone != null)
        {
            droneTransform = drone.transform;
            droneController = drone;

            if (droneCamera == null)
            {
                droneCamera = drone.GetComponentInChildren<OrbitCamera>();
                if (droneCamera != null)
                {
                    Debug.Log($"Drone camera found: {droneCamera.name}");
                    droneCamera.SetTarget(droneTransform);
                }
            }
        }

        if (droneCamera == null && droneTransform != null)
        {
            CreateOrbitCamera();
        }

        if (startWithPlayerCamera)
        {
            if (SceneManager.GetActiveScene().name != "Menu")
            {
                ActivatePlayerCamera();
            }
            else
            {
                ActivateDroneCamera();
            }
        }
        else
        {
            ActivateDroneCamera();
        }

        if (switchPrompt != null)
        {
            switchPrompt.SetActive(true);
        }

        Debug.Log($"Camera setup complete. Player camera: {(playerCamera != null ? "OK" : "MISSING")}, Drone camera: {(droneCamera != null ? "OK" : "MISSING")}");
    }

    private void CreateOrbitCamera()
    {
        if (droneTransform == null) return;

        GameObject cameraObject = new GameObject("OrbitCamera");
        cameraObject.transform.SetParent(droneTransform);
        cameraObject.transform.localPosition = new Vector3(0, 1, -3);
        cameraObject.transform.localRotation = Quaternion.identity;

        Camera cam = cameraObject.AddComponent<Camera>();
        cam.fieldOfView = 60;

        AudioListener listener = cameraObject.AddComponent<AudioListener>();

        droneCamera = cameraObject.AddComponent<OrbitCamera>();
        droneCamera.SetTarget(droneTransform);

        Debug.Log("Orbit camera created automatically");
    }

    private void Update()
    {
        if (playerCamera == null || droneCamera == null)
        {
            FindAndSetupCameras();
        }

        if (Input.GetKeyDown(switchKey))
        {
            SwitchCamera();
        }
    }

    private void SwitchCamera()
    {
        if (isDroneCameraActive)
        {
            ActivatePlayerCamera();
        }
        else
        {
            ActivateDroneCamera();
        }

        Debug.Log($"Switched to {(isDroneCameraActive ? "Drone" : "Player")} camera");
    }

    private void ActivatePlayerCamera()
    {
        isDroneCameraActive = false;

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            AudioListener playerListener = playerCamera.GetComponent<AudioListener>();
            if (playerListener != null) playerListener.enabled = true;
        }

        if (droneCamera != null)
        {
            droneCamera.Deactivate();
        }

        if (xrOrigin != null)
        {
            xrOrigin.gameObject.SetActive(true);
        }

        if (droneController != null)
        {
            droneController.SetControlMode(true); 
            Debug.Log("Player camera activated - VR control mode (controllers only)");
        }

        UpdateUIPrompt("Press C to switch to Drone Camera (PC controls)");
    }

    private void ActivateDroneCamera()
    {
        isDroneCameraActive = true;

        if (playerCamera != null)
        {
            playerCamera.enabled = false;
          //  AudioListener playerListener = playerCamera.GetComponent<AudioListener>();
          //  if (playerListener != null) playerListener.enabled = false;
        }

        if (droneCamera != null)
        {
            droneCamera.Activate();
        }

        if (droneController != null)
        {
            droneController.SetControlMode(false); // false = ПК режим
            Debug.Log("Drone camera activated - PC control mode (WASD + mouse)");
        }

        UpdateUIPrompt("Press C to switch to Player Camera (VR controls)");
    }

    private void UpdateUIPrompt(string text)
    {
        if (switchPrompt != null)
        {
            var tmpText = switchPrompt.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = text;
            }
        }
    }

    public void SwitchToPlayerCamera()
    {
        if (isDroneCameraActive) SwitchCamera();
    }

    public void SwitchToDroneCamera()
    {
        if (!isDroneCameraActive) SwitchCamera();
    }

    public bool IsDroneCameraActive() => isDroneCameraActive;
}