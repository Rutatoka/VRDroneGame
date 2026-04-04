using UnityEngine;

public class CameraCollisionSync : MonoBehaviour
{
    private Transform xrRig;
    private CharacterController controller;
    private Vector3 lastValidLocalPosition;

    void Start()
    {
        // Находим XR Rig (родительский объект)
        xrRig = transform.parent?.parent; // Путь: Main Camera -> Camera Offset -> XR Rig
        if (xrRig == null)
        {
            Debug.LogError("XR Rig не найден!");
            return;
        }

        controller = xrRig.GetComponent<CharacterController>();
        lastValidLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (controller == null || xrRig == null) return;

        // Проверяем, не провалилась ли камера сквозь стену
        if (Physics.CheckSphere(transform.position, 0.1f))
        {
            // Камера в стене - возвращаем в последнюю безопасную позицию
            transform.localPosition = lastValidLocalPosition;
        }
        else
        {
            // Сохраняем безопасную позицию
            lastValidLocalPosition = transform.localPosition;
        }

        // Опционально: не даем камере уезжать слишком далеко от центра XR Rig
        float maxOffset = 0.5f; // Максимальное смещение камеры от центра
        Vector3 localPos = transform.localPosition;

        if (Mathf.Abs(localPos.x) > maxOffset || Mathf.Abs(localPos.z) > maxOffset)
        {
            localPos.x = Mathf.Clamp(localPos.x, -maxOffset, maxOffset);
            localPos.z = Mathf.Clamp(localPos.z, -maxOffset, maxOffset);
            transform.localPosition = localPos;
        }
    }
}