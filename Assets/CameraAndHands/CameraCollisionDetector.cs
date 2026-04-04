using UnityEngine;

public class CameraCollisionDetector : MonoBehaviour
{
    private Vector3 lastSafePosition;
    private Transform xrRig;

    void Start()
    {
        xrRig = transform.parent?.parent;
        lastSafePosition = transform.position;
    }

    void Update()
    {
        // Проверяем, не внутри ли камера стены
        if (IsCameraInsideWall())
        {
 
            transform.position = lastSafePosition;

            // Также пробуем отодвинуть XR Rig
            if (xrRig != null)
            {
                Vector3 direction = (lastSafePosition - xrRig.position).normalized;
                xrRig.position = lastSafePosition - direction * 0.5f;
            }
        }
        else
        {
            lastSafePosition = transform.position;
        }
    }

    bool IsCameraInsideWall()
    {
        // Проверяем коллизии вокруг камеры
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.2f);
        foreach (var col in colliders)
        {
            if (!col.isTrigger && col.gameObject != gameObject &&
                col.gameObject.transform.parent != transform.parent)
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}