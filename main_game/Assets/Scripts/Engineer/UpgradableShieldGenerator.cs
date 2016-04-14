using System;
using UnityEngine;

/// <summary>
/// An upgradable shield generator.
/// </summary>
public class UpgradableShieldGenerator : UpgradableComponent
{
	/// <summary>
	/// The maximum shield capacity.
	/// </summary>
	/// <value>The maximum shield value.</value>
	public float MaxShield { get; private set; }

	/// <summary>
	/// The shield recharge rate.
	/// </summary>
	/// <value>The recharge rate.</value>
	public float MaxRechargeRate { get; private set; }


    private float MaxShieldUpgradeRate;
    private float MaxRechargeRateUpgradeRate;

	/// <summary>
	/// Initializes a new <see cref="UpgradableShieldGenerator"/>.
	/// </summary>
	/// <param name="initialShield">The initial value for the maximum shield.</param>
	/// <param name="initialRechargeRate">The initial valuie for the maximum recharge rate.</param>
    public UpgradableShieldGenerator(float initialShield, float initialRechargeRate, float MaxShieldUpgradeRate, float MaxRechargeRateUpgradeRate) : base()
	{
		this.Type 			 = ComponentType.ShieldGenerator;
		this.MaxHealth 		 = this.Health = 100; // TODO: read this from GameSettings
		this.MaxShield 		 = initialShield;
		this.MaxRechargeRate = initialRechargeRate;

        this.MaxShieldUpgradeRate       = MaxShieldUpgradeRate;
        this.MaxRechargeRateUpgradeRate = MaxRechargeRateUpgradeRate;
        Debug.Log(MaxShieldUpgradeRate);
        Debug.Log(MaxRechargeRateUpgradeRate);
	}

	// TODO: balance values

	/// <summary>
	/// Gets the efficiency of the component. Efficiency decreases when the component is damaged.
	/// </summary>
	/// <returns>The efficiency as a value between 0 and 1.</returns>
	public override float GetEfficiency()
	{
		if (Health == 0)
			return 0;
		
		int damage = MaxHealth - Health;
		return (MaxHealth - (damage / 2)) / MaxHealth;
	}

	/// <summary>
	/// Gets the maximum shield capacity at the current health level.
	/// </summary>
	/// <returns>The maximum shield value.</returns>
	public float GetCurrentMaxShield()
	{
		return GetEfficiency() * MaxShield;
	}

	/// <summary>
	/// Gets the recharge rate at the current health level.
	/// </summary>
	/// <returns>The current recharge rate.</returns>
	public float GetCurrentRechargeRate()
	{
		return GetEfficiency() * MaxRechargeRate;
	}

	/// <summary>
	/// Upgrades this component to the next level. Even levels give a boost to forward speed,
	/// odd levels give a boost to turning speed.
	/// </summary>
	public override void Upgrade()
	{
		base.Upgrade();



        MaxShield = Convert.ToInt32(MaxShield * MaxShieldUpgradeRate);
        MaxRechargeRate = Convert.ToInt32(MaxRechargeRate * MaxRechargeRateUpgradeRate);
	}
}

