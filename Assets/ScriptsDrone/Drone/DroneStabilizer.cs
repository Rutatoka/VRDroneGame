using UnityEngine;

public class DroneStabilizer : MonoBehaviour
{
    [Header("Stabilization Settings")]
    [SerializeField] private bool enableStabilization = true;
    [SerializeField] private float stabilizationStrength = 10f;
    [SerializeField] private float maxStabilizationTorque = 15f;
    [SerializeField] private float stabilizationDelay = 0.2f;
    [SerializeField] private float stabilizationSpeed = 5f;

    [Header("Recovery Settings")]
    [SerializeField] private bool autoRecoverOnUpsideDown = true;
    [SerializeField] private float upsideDownThreshold = 120f; 
    [SerializeField] private float recoveryDelay = 0.5f; 
    [SerializeField] private float recoveryTorque = 20f; 

    [Header("Conditions")]
    [SerializeField] private bool stabilizeOnlyWhenNoInput = true;
    [SerializeField] private float minInputThreshold = 0.1f;

    [Header("Visual")]
    [SerializeField] private bool showDebugInfo = true;

    private Rigidbody rb;
    private DroneController droneController;
    private float noInputTimer = 0f;
    private float recoveryTimer = 0f;
    private bool isRecovering = false;
    private Quaternion targetRotation;

    private Vector3 targetAngularVelocity;
    private float stabilizationCooldown = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        droneController = GetComponent<DroneController>();

        if (rb == null)
        {
            Debug.LogError("DroneStabilizer: Rigidbody not found!");
        }

        if (rb != null)
        {
            rb.maxAngularVelocity = 10f; 
        }
    }

    private void Update()
    {
        if (!enableStabilization) return;
        if (droneController != null && !droneController.IsActive()) return;

        if (stabilizationCooldown > 0)
            stabilizationCooldown -= Time.deltaTime;

        bool hasInput = CheckForInput();

        if (stabilizeOnlyWhenNoInput && hasInput)
        {
            noInputTimer = 0f;
            isRecovering = false;
            recoveryTimer = 0f;
            return;
        }

        if (!hasInput)
        {
            noInputTimer += Time.deltaTime;
        }
        else
        {
            noInputTimer = 0f;
        }

        bool isUpsideDown = CheckIfUpsideDown();

        if (autoRecoverOnUpsideDown && isUpsideDown)
        {
            HandleUpsideDownRecovery();
        }
        else
        {
            float currentTiltX = NormalizeAngle(transform.eulerAngles.x);
            float currentTiltZ = NormalizeAngle(transform.eulerAngles.z);
            float tiltMagnitude = Mathf.Sqrt(currentTiltX * currentTiltX + currentTiltZ * currentTiltZ);

            if (tiltMagnitude > 15f && noInputTimer >= stabilizationDelay && stabilizationCooldown <= 0)
            {
                StabilizeDrone(currentTiltX, currentTiltZ);
            }
        }

        if (showDebugInfo)
        {
            float tiltX = NormalizeAngle(transform.eulerAngles.x);
            float tiltZ = NormalizeAngle(transform.eulerAngles.z);

            if (Mathf.Abs(tiltX) > 30f || Mathf.Abs(tiltZ) > 30f)
            {
                Debug.Log($"Drone tilted: X={tiltX:F1}°, Z={tiltZ:F1}°, Recovery={isRecovering}");
            }
        }
    }

    private void FixedUpdate()
    {
        if (!enableStabilization) return;
        if (droneController != null && !droneController.IsActive()) return;

        if (isRecovering || stabilizationCooldown > 0)
        {
            ApplyRecoveryForce();
        }
    }

    private bool CheckIfUpsideDown()
    {
        Vector3 up = transform.up;
        return Vector3.Dot(up, Vector3.up) < -0.5f; 
    }

    private void HandleUpsideDownRecovery()
    {
        if (!isRecovering)
        {
            recoveryTimer += Time.deltaTime;

            if (recoveryTimer >= recoveryDelay)
            {
                isRecovering = true;
                recoveryTimer = 0f;

                if (showDebugInfo)
                    Debug.Log("Drone is upside down! Starting recovery...");

                rb.angularVelocity = Vector3.zero;
                rb.velocity = Vector3.zero;
            }
        }
    }

    private void StabilizeDrone(float tiltX, float tiltZ)
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetYawRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y, 0);

        transform.rotation = Quaternion.Slerp(currentRotation, targetYawRotation, stabilizationSpeed * Time.deltaTime);

        if (rb != null)
        {
            Vector3 stabilizationTorque = new Vector3(
                -tiltX * stabilizationStrength,
                0,
                -tiltZ * stabilizationStrength
            );

            stabilizationTorque = Vector3.ClampMagnitude(stabilizationTorque, maxStabilizationTorque);

            rb.AddTorque(stabilizationTorque, ForceMode.Acceleration);

      
            rb.angularVelocity *= 0.95f;
        }

        stabilizationCooldown = 0.1f; 
    }

    private void ApplyRecoveryForce()
    {
        if (rb == null) return;

        Quaternion targetRotation = Quaternion.LookRotation(
            Vector3.ProjectOnPlane(transform.forward, Vector3.up),
            Vector3.up
        );

        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(transform.rotation);

        Vector3 targetAngularVelocity;
        float angle;
        Vector3 axis;
        deltaRotation.ToAngleAxis(out angle, out axis);

        if (angle > 180)
            angle -= 360;

        if (angle > 1f)
        {
            targetAngularVelocity = axis.normalized * angle * Mathf.Deg2Rad * recoveryTorque;
            targetAngularVelocity = Vector3.ClampMagnitude(targetAngularVelocity, 10f);

            rb.AddTorque(targetAngularVelocity, ForceMode.Acceleration);

            if (angle < 30f)
            {
                rb.angularVelocity *= 0.98f;
            }
        }
        else
        {
            isRecovering = false;
            stabilizationCooldown = 0.5f; 

            transform.rotation = targetRotation;
            rb.angularVelocity = Vector3.zero;

            if (showDebugInfo)
                Debug.Log("Recovery completed!");
        }
    }

    private bool CheckForInput()
    {
        if (droneController == null) return false;

        bool hasInput = false;

        if (Input.GetAxis("Horizontal") != 0 ||
            Input.GetAxis("Vertical") != 0 ||
            Input.GetKey(KeyCode.Q) ||
            Input.GetKey(KeyCode.E) ||
            Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.LeftControl))
        {
            hasInput = true;
        }

        if (Input.GetMouseButton(1) && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
        {
            hasInput = true;
        }

        return hasInput;
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }
    public void SetStabilizationEnabled(bool enabled)
    {
        enableStabilization = enabled;
        if (!enabled)
        {
            isRecovering = false;
            recoveryTimer = 0f;
            noInputTimer = 0f;
        }
        Debug.Log($"Stabilization {(enabled ? "enabled" : "disabled")}");
    }

    public void ManualRecovery()
    {
        if (enableStabilization && CheckIfUpsideDown())
        {
            isRecovering = true;
            recoveryTimer = 0f;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("Manual recovery initiated!");
        }
    }

    public void ResetStabilization()
    {
        noInputTimer = 0f;
        isRecovering = false;
        recoveryTimer = 0f;
        stabilizationCooldown = 0f;
        if (showDebugInfo) Debug.Log("Stabilization reset");
    }

    public bool IsStabilizing() => isRecovering || stabilizationCooldown > 0;
    public float GetCurrentTiltX() => NormalizeAngle(transform.eulerAngles.x);
    public float GetCurrentTiltZ() => NormalizeAngle(transform.eulerAngles.z);
    public bool IsUpsideDown() => CheckIfUpsideDown();
}