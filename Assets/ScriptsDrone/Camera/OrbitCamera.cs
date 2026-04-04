using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; 
    [Header("Orbit Settings")]
    [SerializeField] private float distance = 5f; 
    [SerializeField] private float height = 2f; 
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private bool autoRotate = false;
    [SerializeField] private float autoRotateSpeed = 0.5f;

    [Header("Zoom Settings")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 15f;

    [Header("Angle Limits")]
    [SerializeField] private bool limitVerticalAngle = true;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;

    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 20f;
    private float currentDistance;
    private Vector3 currentVelocity = Vector3.zero;

    private bool isActive = false;

    private void Start()
    {
        currentDistance = distance;

        if (target == null)
        {
            target = FindObjectOfType<DroneController>()?.transform;
        }
    }

    private void LateUpdate()
    {
        if (!isActive || target == null) return;

        if (Input.GetMouseButton(1))
        {
            currentHorizontalAngle += Input.GetAxis("Mouse X") * rotationSpeed;
            currentVerticalAngle -= Input.GetAxis("Mouse Y") * rotationSpeed;

            if (limitVerticalAngle)
            {
                currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
            }
        }
        if (autoRotate && !Input.GetMouseButton(1))
        {
            currentHorizontalAngle += autoRotateSpeed * Time.deltaTime;
        }
        transform.LookAt(target);
        if (enableZoom)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);

        Vector3 direction = rotation * Vector3.back;

        Vector3 desiredPosition = target.position + direction * currentDistance + Vector3.up * height;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        transform.LookAt(target.position + Vector3.up * (height * 0.5f));
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void Activate()
    {
        isActive = true;

        GetComponent<Camera>().enabled = true;
        var audioListener = GetComponent<AudioListener>();
        if (audioListener != null) audioListener.enabled = true;

        Debug.Log("Orbit camera activated");
    }

    public void Deactivate()
    {
        isActive = false;

        GetComponent<Camera>().enabled = false;
        var audioListener = GetComponent<AudioListener>();
        if (audioListener != null) audioListener.enabled = false;
    }

    public void ResetCameraAngle()
    {
        currentHorizontalAngle = 0f;
        currentVerticalAngle = 20f;
        currentDistance = distance;
    }

    public void SetDistance(float newDistance)
    {
        currentDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }
}