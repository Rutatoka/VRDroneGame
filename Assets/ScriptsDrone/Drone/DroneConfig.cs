using UnityEngine;

[CreateAssetMenu(fileName = "DroneConfig", menuName = "Drone/Drone Configuration")]
public class DroneConfig : ScriptableObject
{
    [Header("Movement Settings")]
    public float maxSpeed = 10f;
    public float acceleration = 5f;
    public float rotationSpeed = 90f;

    [Header("Physics Settings")]
    [Tooltip("Сопротивление воздуха (чем выше, тем быстрее торможение)")]
    public float drag = 1f;

    [Tooltip("Сопротивление вращению")]
    public float angularDrag = 1f;

    [Header("Hover Settings")]
    public float hoverHeight = 2f;
    public float hoverForce = 10f;
    public float hoverStability = 3f;

    [Header("Battery Settings")]
    public float batteryLife = 120f;

    [Header("Gameplay Settings")]
    public float obstaclePenalty = 10f;

    [Header("Visual Settings")]
    public float modelTiltAmount = 30f;

    public void ResetToDefault()
    {
        maxSpeed = 10f;
        acceleration = 5f;
        rotationSpeed = 90f;
        drag = 1f;
        angularDrag = 1f;
        hoverHeight = 2f;
        hoverForce = 10f;
        hoverStability = 3f;
        batteryLife = 120f;
        obstaclePenalty = 10f;
        modelTiltAmount = 30f;
    }
}