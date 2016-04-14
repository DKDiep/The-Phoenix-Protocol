using System;

[System.Serializable]
public class UpgradeProperties
{
    public string name;
    public ComponentType type;
    public string description;
    public int cost;
    public int numberOfLevels;
    public bool repairable;
    public int currentLevel;
}
