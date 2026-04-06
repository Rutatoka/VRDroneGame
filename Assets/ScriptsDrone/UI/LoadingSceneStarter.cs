using UnityEngine;

public class LoadingSceneStarter : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== LoadingScene загружена ===");

        // Небольшая задержка для гарантии
        Invoke(nameof(StartLoading), 0.1f);
    }

    void StartLoading()
    {
        if (LoadingManager.Instance != null)
        {
            Debug.Log("Вызываем LoadingManager.StartLoadingTargetScene()");
            LoadingManager.Instance.StartLoadingTargetScene();
        }
        else
        {
            Debug.LogError("LoadingManager.Instance = null! Создаю...");
            GameObject go = new GameObject("LoadingManager");
            LoadingManager lm = go.AddComponent<LoadingManager>();

            // Копируем настройки (если нужно)
            // lm.minLoadTime = 1f;

            // Повторяем попытку
            Invoke(nameof(StartLoading), 0.2f);
        }
    }
}