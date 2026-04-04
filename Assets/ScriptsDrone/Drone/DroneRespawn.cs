using UnityEngine;


public class DroneRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private float defaultRespawnDuration = 2f;
    [SerializeField] private bool enableKeyboardControls = true; 
    [SerializeField] private KeyCode smoothRespawnKey = KeyCode.O; 
    [SerializeField] private KeyCode instantRespawnKey = KeyCode.P; 

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem respawnParticles; 
    [SerializeField] private AudioSource respawnSound;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;
    private DroneController droneController;


    private bool isRespawning = false;
    private float currentRespawnTime = 0f;
    private float respawnDuration = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        droneController = GetComponent<DroneController>();

        if (rb == null)
            Debug.LogError("DroneRespawn: Rigidbody not found on this object!");
    }

    private void Start()
    {

        SaveCurrentPositionAsStart();
    }

    private void Update()
    {
        if (isRespawning)
        {
            UpdateSmoothRespawn();
        }

        if (enableKeyboardControls)
        {
            if (Input.GetKeyDown(smoothRespawnKey))
            {
                SmoothResetToStart();
            }

            if (Input.GetKeyDown(instantRespawnKey))
            {
                InstantResetToStart();
            }
        }
    }

    public void SaveCurrentPositionAsStart()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        Debug.Log($"Start position saved: {startPosition}");
    }

    public void SetStartPosition(Vector3 position, Quaternion rotation)
    {
        startPosition = position;
        startRotation = rotation;
        Debug.Log($"Start position manually set: {position}");
    }

    public void SmoothResetToStart(float duration = -1)
    {
        respawnDuration = duration > 0 ? duration : defaultRespawnDuration;
        currentRespawnTime = respawnDuration;
        isRespawning = true;

        if (droneController != null && !droneController.IsActive())
        {
            droneController.SetActive(true);
        }


        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        PlayRespawnEffects();

        Debug.Log($"Starting smooth respawn to start position in {respawnDuration} seconds");
    }


    public void InstantResetToStart()
    {

        transform.position = startPosition;
        transform.rotation = startRotation;


        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        if (droneController != null && !droneController.IsActive())
        {
            droneController.SetActive(true);
        }


        isRespawning = false;


        PlayRespawnEffects();

        Debug.Log("Instant respawn completed!");
    }


    private void UpdateSmoothRespawn()
    {
        if (!isRespawning) return;

        if (currentRespawnTime > 0)
        {

            currentRespawnTime -= Time.deltaTime;

            float progress = 1f - (currentRespawnTime / respawnDuration);
            progress = Mathf.Clamp01(progress);

            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            transform.position = Vector3.Lerp(transform.position, startPosition, smoothProgress * 3f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, startRotation, smoothProgress * 4f * Time.deltaTime);

            if (progress > 0.9f && rb != null)
            {
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * 5f);
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.deltaTime * 5f);
            }
        }
        else
        {
            transform.position = startPosition;
            transform.rotation = startRotation;

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            isRespawning = false;
            Debug.Log("Smooth respawn completed!");
        }
    }
    private void PlayRespawnEffects()
    {
        if (respawnParticles != null)
        {
            respawnParticles.Play();
        }

        if (respawnSound != null)
        {
            respawnSound.Play();
        }
    }

    public bool IsRespawning()
    {
        return isRespawning;
    }

    public Vector3 GetStartPosition()
    {
        return startPosition;
    }

    public Quaternion GetStartRotation()
    {
        return startRotation;
    }
    public void CancelRespawn()
    {
        isRespawning = false;
        Debug.Log("Respawn cancelled");
    }
}