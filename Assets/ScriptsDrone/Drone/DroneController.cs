using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ActionBasedController leftController;
    [SerializeField] private ActionBasedController rightController;
    [SerializeField] private Transform modelTransform;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference leftMoveAction;
    [SerializeField] private InputActionReference rightMoveAction;
    [SerializeField] private InputActionReference rightRotateAction;

    [Header("Control Settings")]
    [SerializeField] private bool useKeyboardForDebug = true;
    [SerializeField] private float hoverHeight = 2f;
    [SerializeField] private float hoverForce = 10f;

    [Header("Smoothing Settings")]
    [SerializeField] private float movementSmoothing = 5f; 
    [SerializeField] private float rotationSmoothing = 10f; 
    [SerializeField] private float verticalSmoothing = 3f; 
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Damping Settings")]
    [SerializeField] private float linearDamping = 2f; 
    [SerializeField] private float angularDamping = 3f; 
    [SerializeField] private float verticalDamping = 2f; 

    [Header("Model Settings")]
    [SerializeField] private Vector3 modelRotationOffset = new Vector3(270, 0, 0);
    [SerializeField] private float modelTiltAmount = 30f; 
    [SerializeField] private float modelTiltSmoothing = 5f; 

    [Header("Visual")]
    [SerializeField] private bool showDebugArrow = true;

    private GameManager _gm;
    private float maxSpeed;
    private float acceleration;
    private float rotationSpeed;
    private float batteryLife;
    private float obstaclePenalty;
    private bool useVRControls = false;
    private bool isPCModeActive = true;
    private float currentBattery;
    private bool isActive = true;

    private Vector2 currentMoveInput;
    private float currentYawInput;
    private float currentVerticalInput;

    private Vector2 targetMoveInput;
    private float targetYawInput;
    private float targetVerticalInput;

    private Vector3 currentModelTilt;
    private Vector3 targetModelTilt;

    private Quaternion compensationRotation;
    private Quaternion originalModelRotation;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.drag = linearDamping;
        rb.angularDrag = angularDamping;

        FindControllers();

        if (leftMoveAction == null || rightMoveAction == null || rightRotateAction == null)
        {
            TryFindInputActions();
        }

        SetupModelCompensation();
    }
    public void UpdateControlModeByCamera(bool isDroneCameraActive)
    {
        bool shouldUseVR = !isDroneCameraActive;

        SetControlMode(shouldUseVR);
        Debug.Log($"Control mode updated: {(shouldUseVR ? "VR (controllers)" : "PC (WASD)")}, DroneCameraActive: {isDroneCameraActive}");
    }
    public void SetControlMode(bool useVR)
    {
        useVRControls = useVR;

        targetMoveInput = Vector2.zero;
        targetYawInput = 0;
        targetVerticalInput = 0;
        currentMoveInput = Vector2.zero;
        currentYawInput = 0;
        currentVerticalInput = 0;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (useVR)
        {
            Debug.Log("=== VR CONTROL MODE ACTIVE ===");
            Debug.Log("WASD and mouse are DISABLED");
            Debug.Log("Use HTC Vive controllers to fly the drone");

            // Включаем XR input actions
            if (leftMoveAction != null && leftMoveAction.action != null)
                leftMoveAction.action.Enable();
            if (rightMoveAction != null && rightMoveAction.action != null)
                rightMoveAction.action.Enable();
            if (rightRotateAction != null && rightRotateAction.action != null)
                rightRotateAction.action.Enable();
        }
        else
        {
            Debug.Log("=== PC CONTROL MODE ACTIVE ===");
            Debug.Log("WASD + mouse ENABLED");
            Debug.Log("VR controllers are IGNORED");

            if (leftMoveAction != null && leftMoveAction.action != null)
                leftMoveAction.action.Disable();
            if (rightMoveAction != null && rightMoveAction.action != null)
                rightMoveAction.action.Disable();
            if (rightRotateAction != null && rightRotateAction.action != null)
                rightRotateAction.action.Disable();
        }
    }
    private void SetupModelCompensation()
    {
        if (modelTransform == null)
        {
            modelTransform = GetComponentInChildren<MeshRenderer>()?.transform;
            if (modelTransform == null && transform.childCount > 0)
            {
                modelTransform = transform.GetChild(0);
            }
        }

        if (modelTransform != null)
        {
            originalModelRotation = modelTransform.localRotation;
            compensationRotation = Quaternion.Inverse(Quaternion.Euler(modelRotationOffset));
            modelTransform.localRotation = compensationRotation * originalModelRotation;
        }
    }

    private void FindControllers()
    {
        var xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null)
        {
            var controllers = xrOrigin.GetComponentsInChildren<ActionBasedController>();
            foreach (var controller in controllers)
            {
                if (controller.name.Contains("Left") || controller.name == "Left Controller")
                    leftController = controller;
                else if (controller.name.Contains("Right") || controller.name == "Right Controller")
                    rightController = controller;
            }
        }
    }

    private void TryFindInputActions()
    {
        var inputActions = Resources.Load<InputActionAsset>("XRI Default Input Actions");
        if (inputActions == null)
            inputActions = Resources.Load<InputActionAsset>("InputActions/XRI Default Input Actions");

        if (inputActions != null)
        {
            var moveActionLeft = inputActions.FindAction("XRI LeftHand Locomotion/Move");
            var moveActionRight = inputActions.FindAction("XRI RightHand Locomotion/Move");
            var turnAction = inputActions.FindAction("XRI RightHand Locomotion/Turn");

            if (moveActionLeft != null)
                leftMoveAction = InputActionReference.Create(moveActionLeft);
            if (moveActionRight != null)
                rightMoveAction = InputActionReference.Create(moveActionRight);
            if (turnAction != null)
                rightRotateAction = InputActionReference.Create(turnAction);
        }
    }

    private void Start()
    {
        LoadConfiguration();
        transform.rotation = Quaternion.identity;
        _gm = FindFirstObjectByType<GameManager>()?.GetComponent<GameManager>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        if (!isActive) return;

        GetInput();
        ApplySmoothing();
        UpdateModelVisuals();

        if (batteryLife > 0)
        {
            currentBattery -= Time.deltaTime;
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                isActive = false;
                rb.isKinematic = true;
                _gm.DroneDeath("Батарея");
                Debug.Log("Drone battery depleted!");
            }
        }

        if (useKeyboardForDebug)
        {
            if (Input.GetKeyDown(KeyCode.R)) LoadConfiguration();
            if (Input.GetKeyDown(KeyCode.P)) transform.rotation = Quaternion.identity;
            if (Input.GetKeyDown(KeyCode.F1)) Debug.Log($"Speed: {rb.velocity.magnitude:F1} | Battery: {GetBatteryPercentage():F0}%");
        }
    }

    private void GetInput()
    {
        targetMoveInput = Vector2.zero;
        targetYawInput = 0;
        targetVerticalInput = 0;

        if (useVRControls)
        {
            // ===== ТОЛЬКО VR КОНТРОЛЛЕРЫ =====
            if (leftMoveAction != null && leftMoveAction.action != null && leftMoveAction.action.enabled)
            {
                targetMoveInput = leftMoveAction.action.ReadValue<Vector2>();
            }

            if (rightMoveAction != null && rightMoveAction.action != null && rightMoveAction.action.enabled)
            {
                Vector2 rightMove = rightMoveAction.action.ReadValue<Vector2>();
                targetVerticalInput = rightMove.y;
                if (Mathf.Abs(rightMove.x) > Mathf.Abs(targetYawInput))
                    targetYawInput = rightMove.x;
            }

            if (rightRotateAction != null && rightRotateAction.action != null && rightRotateAction.action.enabled)
            {
                targetYawInput = rightRotateAction.action.ReadValue<float>();
            }
        }
        else
        {
            // ===== ТОЛЬКО ПК  =====

            targetMoveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (Input.GetKey(KeyCode.Q))
                targetYawInput = -1;
            else if (Input.GetKey(KeyCode.E))
                targetYawInput = 1;
            else if (Input.GetMouseButton(1)) // Правая кнопка мыши
                targetYawInput = Input.GetAxis("Mouse X") * 2f;
            else
                targetYawInput = 0;

            if (Input.GetKey(KeyCode.Space))
                targetVerticalInput = 1;
            else if (Input.GetKey(KeyCode.LeftControl))
                targetVerticalInput = -1;
            else
                targetVerticalInput = 0;
        }

        if (targetMoveInput.magnitude > 1f)
            targetMoveInput.Normalize();
    }

    private void ApplySmoothing()
    {
        currentMoveInput = Vector2.Lerp(currentMoveInput, targetMoveInput, movementSmoothing * Time.deltaTime);
        currentYawInput = Mathf.Lerp(currentYawInput, targetYawInput, rotationSmoothing * Time.deltaTime);
        currentVerticalInput = Mathf.Lerp(currentVerticalInput, targetVerticalInput, verticalSmoothing * Time.deltaTime);
    }

    private void UpdateModelVisuals()
    {
        if (modelTransform == null) return;

        float forwardTilt = -currentMoveInput.y * modelTiltAmount;
        float rightTilt = currentMoveInput.x * modelTiltAmount;

        targetModelTilt = new Vector3(forwardTilt, 0, rightTilt);
        currentModelTilt = Vector3.Lerp(currentModelTilt, targetModelTilt, modelTiltSmoothing * Time.deltaTime);

        Quaternion tiltRotation = Quaternion.Euler(currentModelTilt);
        modelTransform.localRotation = compensationRotation * originalModelRotation * tiltRotation;
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        MoveDrone();
        ApplyYaw();
        ApplyVerticalMovement();
        ApplyHoverEffect();
        ApplyDamping();
    }

    private void LoadConfiguration()
    {
        if (ConfigManager.Instance != null && ConfigManager.Instance.DroneConfig != null)
        {
            var config = ConfigManager.Instance.DroneConfig;
            maxSpeed = config.maxSpeed;
            acceleration = config.acceleration;
            rotationSpeed = config.rotationSpeed;
            batteryLife = config.batteryLife;
            obstaclePenalty = config.obstaclePenalty;
            currentBattery = batteryLife;

            rb.drag = linearDamping * (maxSpeed / 10f);
            rb.angularDrag = angularDamping;
        }
        else
        {
            SetDefaultValues();
        }
    }

    private void SetDefaultValues()
    {
        maxSpeed = 10f;
        acceleration = 5f;
        rotationSpeed = 90f;
        batteryLife = 120f;
        obstaclePenalty = 10f;
        currentBattery = batteryLife;
    }

    private void MoveDrone()
    {
        float speedFactor = accelerationCurve.Evaluate(Mathf.Abs(currentMoveInput.magnitude));
        float currentAcceleration = acceleration * speedFactor;

        Vector3 targetVelocity = transform.forward * currentMoveInput.y * maxSpeed;
        targetVelocity += transform.right * currentMoveInput.x * maxSpeed;

        Vector3 velocityChange = targetVelocity - rb.velocity;
        velocityChange.y = 0;

        Vector3 force = velocityChange * currentAcceleration;
        if (force.magnitude > currentAcceleration * maxSpeed)
        {
            force = force.normalized * currentAcceleration * maxSpeed;
        }

        rb.AddForce(force, ForceMode.Acceleration);

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
        }
    }

    private void ApplyYaw()
    {
        if (Mathf.Abs(currentYawInput) > 0.05f)
        {
            float speedFactor = 1f - Mathf.Clamp01(rb.velocity.magnitude / maxSpeed) * 0.5f;
            float rotation = currentYawInput * rotationSpeed * Time.fixedDeltaTime * speedFactor;
            transform.Rotate(0, rotation, 0);
        }
    }

    private void ApplyVerticalMovement()
    {
        if (Mathf.Abs(currentVerticalInput) > 0.05f)
        {
            float speedFactor = accelerationCurve.Evaluate(Mathf.Abs(currentVerticalInput));
            float verticalForce = currentVerticalInput * (maxSpeed * 0.5f) * acceleration * speedFactor;
            rb.AddForce(Vector3.up * verticalForce, ForceMode.Acceleration);
        }
    }

    private void ApplyHoverEffect()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, hoverHeight + 2f))
        {
            float distanceToGround = hit.distance;
            float hoverStrength = 0f;

            float hoverZone = hoverHeight * 0.3f;

            if (distanceToGround < hoverHeight - hoverZone)
            {
                hoverStrength = (hoverHeight - distanceToGround) * hoverForce;
            }
            else if (distanceToGround > hoverHeight + hoverZone)
            {
                hoverStrength = -(distanceToGround - hoverHeight) * hoverForce;
            }
            else if (Mathf.Abs(distanceToGround - hoverHeight) < hoverZone)
            {
                float t = Mathf.InverseLerp(0, hoverZone, Mathf.Abs(distanceToGround - hoverHeight));
                hoverStrength = (distanceToGround - hoverHeight) * hoverForce * (1f - t);
            }

            hoverStrength -= rb.velocity.y * verticalDamping;

            rb.AddForce(Vector3.up * hoverStrength, ForceMode.Acceleration);
        }
        else
        {
          
            rb.AddForce(Vector3.down * 9.81f * 0.5f, ForceMode.Acceleration);
        }
    }

    private void ApplyDamping()
    {

        if (Mathf.Abs(currentMoveInput.magnitude) < 0.05f)
        {

            Vector3 dampingForce = -rb.velocity * linearDamping * 0.5f;
            dampingForce.y = 0;
            rb.AddForce(dampingForce, ForceMode.Acceleration);
        }


        if (Mathf.Abs(currentYawInput) < 0.05f)
        {
            rb.angularVelocity *= 0.95f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
   
            if (collision.gameObject.CompareTag("Obstacle")|| collision.transform.name == "CircleScore")
            {
                if (obstaclePenalty > 0)
                {
                    currentBattery -= obstaclePenalty;
                    Debug.Log($"Collision! Battery: {currentBattery:F1}s remaining");


                    Vector3 collisionNormal = collision.contacts[0].normal;
                    float collisionVelocity = collision.relativeVelocity.magnitude;

                    float bounceForce = Mathf.Clamp(obstaclePenalty * collisionVelocity, 5f, maxSpeed);
                    Vector3 bounceDirection = collisionNormal;

                    bounceDirection += Random.insideUnitSphere * 0.3f;
                    bounceDirection.Normalize();

                    rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);

                    rb.angularVelocity *= 0.5f;

                    Invoke(nameof(ResetAfterCollision), 0.1f);

                    if (currentBattery <= 0)
                    {
                        isActive = false;
                        rb.isKinematic = true;
                        if (_gm != null)
                            _gm.DroneDeath("Разряд батареи");
                    }
                }
           
        }
    }
    private void ResetAfterCollision()
    {

        if (isActive)
        {
            DroneStabilizer stabilizer = GetComponent<DroneStabilizer>();
            if (stabilizer != null)
            {
                stabilizer.ResetStabilization();
            }

            Debug.Log("Post-collision stabilization activated");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("trigger") && other.transform.name == "EventObstacle")
        {
            _gm?.EventChecked(other.transform.name);
        }

        if (other.CompareTag("Finish"))
        {
            LevelTimerUI timer = FindObjectOfType<LevelTimerUI>();
            if (timer != null)
            {
                timer.StopTimer();
                timer.ShowFinalTime();
            }

            int score = Mathf.RoundToInt(currentBattery * 10);
            _gm?.CompleteLevel(score);
            Debug.Log($"Финиш! Получено очков: {score}");
        }

        if (other.CompareTag("Collectible"))
        {
            _gm?.AddScore(100);
            Destroy(other.gameObject);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        rb.isKinematic = !active;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public float GetBatteryPercentage() => batteryLife > 0 ? (currentBattery / batteryLife) * 100f : 0;
    public bool IsActive() => isActive;
    public float GetCurrentSpeed() => rb.velocity.magnitude;
}