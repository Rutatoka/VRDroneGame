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
    public float maxSpeed = 10f;
    public float acceleration = 5f;
    public float rotationSpeed = 90f;
    public float batteryLife = 120f;
    public float obstaclePenalty = 10f;
}