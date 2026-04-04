using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public class XRCharacterMovement : MonoBehaviour
{
    private CharacterController characterController;
    private XRDeviceSimulator simulator;
    private float gravity = -9.81f;
    private Vector3 velocity;
    private bool isGrounded;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravityMultiplier = 2f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        simulator = FindObjectOfType<XRDeviceSimulator>();
    }

    void Update()
    {
        // Получаем ввод от XR Device Simulator
        Vector2 inputAxis = Vector2.zero;

        // Эмуляция WASD для теста
        if (Input.GetKey(KeyCode.W)) inputAxis.y = 1;
        if (Input.GetKey(KeyCode.S)) inputAxis.y = -1;
        if (Input.GetKey(KeyCode.A)) inputAxis.x = -1;
        if (Input.GetKey(KeyCode.D)) inputAxis.x = 1;

        // Движение относительно камеры
        Vector3 move = new Vector3(inputAxis.x, 0, inputAxis.y);
        move = Camera.main.transform.TransformDirection(move);
        move.y = 0;

        // Применяем гравитацию
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Небольшое прижатие к земле
        }

        velocity.y += gravity * gravityMultiplier * Time.deltaTime;

        // Двигаем персонажа
        Vector3 finalMovement = move * moveSpeed * Time.deltaTime + velocity * Time.deltaTime;
        characterController.Move(finalMovement);
    }
}