using UnityEngine;
using System;

[System.Serializable]
public class UpgradeProperties 
{
    public string name;
    public ComponentType type;

    [Multiline]
    public string description;
    public int cost;
    public int numberOfLevels;
    public bool repairable;
    public int currentLevel;

    // Upgrade rates for each componentType

    // Drone
    public float droneMovementSpeedUpgradeRate;
    public float droneImprovementTimeUpgradeRate;

    // Engine
    public float engineMaxSpeedUpgradeRate;
    public float engineMaxTurningSpeedUpgradeRate;

    // Hull
    public float hullMaxHealthUpgradeRate;

    // Storage
    public float storageCollectionBonusUpgradeRate;
    public float storageInterestRateUpgradeBonus;

    // Shields
    public float shieldsMaxShieldUpgradeRate;
    public float shieldsMaxRechargeRateUpgradeRate;

    // Turrets
    public float turretsMaxDamageUpgradeRate;
    public float turretsMinFireDelayUpgradeRate;

}
    

