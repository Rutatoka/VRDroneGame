using UnityEngine;

public class HandController : MonoBehaviour
{
    public Transform leftHand;
    public Transform rightHand;
    public Transform head;

    [Header("—мещени€ рук относительно головы")]
    public Vector3 leftHandOffset = new Vector3(-0.3f, -0.2f, 0.5f);
    public Vector3 rightHandOffset = new Vector3(0.3f, -0.2f, 0.5f);

    void Start()
    {
        if (head == null)
            head = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (head == null) return;

        // ѕринудительно устанавливаем позиции рук
        if (leftHand != null)
        {
            leftHand.position = head.TransformPoint(leftHandOffset);
            leftHand.rotation = head.rotation;
        }

        if (rightHand != null)
        {
            rightHand.position = head.TransformPoint(rightHandOffset);
            rightHand.rotation = head.rotation;
        }
    }
}