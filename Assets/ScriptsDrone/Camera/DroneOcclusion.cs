using UnityEngine;
using System.Collections.Generic;

public class DroneOcclusion : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Camera droneCamera;

    [Header("Occlusion Settings")]
    [SerializeField] private LayerMask obstacleLayer = -1;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxDistance = 5f;

    private Dictionary<Renderer, Material[]> wallMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<Renderer, Color[]> originalColors = new Dictionary<Renderer, Color[]>();
    private Dictionary<Renderer, float> fadeProgress = new Dictionary<Renderer, float>();

    private void Start()
    {
        if (droneCamera == null)
        {
            droneCamera = GetComponentInChildren<Camera>();
            if (droneCamera == null)
            {
                droneCamera = Camera.main;
            }
        }
    }

    private void Update()
    {
        if (droneCamera == null) return;

        CheckOcclusion();
        UpdateFading();
    }

    private void CheckOcclusion()
    {
        Vector3 direction = (transform.position - droneCamera.transform.position).normalized;
        float distance = Vector3.Distance(droneCamera.transform.position, transform.position);

        RaycastHit[] hits = Physics.RaycastAll(droneCamera.transform.position, direction, distance, obstacleLayer);

        HashSet<Renderer> currentBlockers = new HashSet<Renderer>();

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && hit.distance < maxDistance)
            {
                currentBlockers.Add(renderer);

                if (!wallMaterials.ContainsKey(renderer))
                {
                    Material[] materials = renderer.materials;
                    wallMaterials[renderer] = materials;

                    Color[] colors = new Color[materials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        colors[i] = materials[i].color;
                        MakeTransparent(materials[i]);
                    }
                    originalColors[renderer] = colors;
                    fadeProgress[renderer] = 0f;
                }
                else
                {
                    fadeProgress[renderer] = Mathf.Min(fadeProgress[renderer] + Time.deltaTime / fadeDuration, 1f);
                    ApplyAlpha(renderer, Mathf.Lerp(1f, minAlpha, fadeProgress[renderer]));
                }
            }
        }

        List<Renderer> toRemove = new List<Renderer>();
        foreach (var renderer in wallMaterials.Keys)
        {
            if (!currentBlockers.Contains(renderer))
            {
                fadeProgress[renderer] = Mathf.Max(fadeProgress[renderer] - Time.deltaTime / fadeDuration, 0f);

                if (fadeProgress[renderer] <= 0f)
                {
                    RestoreMaterial(renderer);
                    toRemove.Add(renderer);
                }
                else
                {
                    ApplyAlpha(renderer, Mathf.Lerp(1f, minAlpha, fadeProgress[renderer]));
                }
            }
        }

        foreach (var renderer in toRemove)
        {
            wallMaterials.Remove(renderer);
            originalColors.Remove(renderer);
            fadeProgress.Remove(renderer);
        }
    }

    private void UpdateFading()
    {
        foreach (var renderer in wallMaterials.Keys)
        {
            if (renderer != null && fadeProgress.ContainsKey(renderer))
            {
                float alpha = Mathf.Lerp(1f, minAlpha, fadeProgress[renderer]);
                ApplyAlpha(renderer, alpha);
            }
        }
    }

    private void MakeTransparent(Material mat)
    {
        if (mat.shader.name == "Standard")
        {
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
        }
    }

    private void ApplyAlpha(Renderer renderer, float alpha)
    {
        if (renderer == null) return;

        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length && i < originalColors[renderer].Length; i++)
        {
            Color color = originalColors[renderer][i];
            color.a = alpha;
            materials[i].color = color;
        }
    }

    private void RestoreMaterial(Renderer renderer)
    {
        if (renderer == null) return;

        if (originalColors.ContainsKey(renderer))
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length && i < originalColors[renderer].Length; i++)
            {
                materials[i].color = originalColors[renderer][i];

                if (materials[i].shader.name == "Standard")
                {
                    materials[i].SetFloat("_Mode", 0);
                    materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    materials[i].SetInt("_ZWrite", 1);
                    materials[i].DisableKeyword("_ALPHATEST_ON");
                    materials[i].DisableKeyword("_ALPHABLEND_ON");
                    materials[i].renderQueue = -1;
                }
            }
        }
    }
}