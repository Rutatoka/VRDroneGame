using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform loadingIcon;
    [SerializeField] private float rotationSpeed = 180f;

    void Update()
    {
        if (loadingIcon != null)
        {
            loadingIcon.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }
}