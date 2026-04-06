using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string version = "1.0";
    public string profileName = "Новый профиль";
    public float bestTime = 0f;
    public int totalScore = 0;
    public List<string> unlockedLevels = new List<string>();
    public int playTime = 0;

    public DroneSettings droneSettings = new DroneSettings();
}

[Serializable]
public class DroneSettings
{
    // Movement Settings
    public float maxSpeed = 10f;
    public float acceleration = 5f;
    public float rotationSpeed = 90f;

    // Physics Settings
    public float drag = 1f;
    public float angularDrag = 1f;

    // Hover Settings
    public float hoverHeight = 2f;
    public float hoverForce = 10f;
    public float hoverStability = 3f;

    // Battery Settings
    public float batteryLife = 120f;

    // Gameplay Settings
    public float obstaclePenalty = 10f;

    // Visual Settings
    public float modelTiltAmount = 30f;
}