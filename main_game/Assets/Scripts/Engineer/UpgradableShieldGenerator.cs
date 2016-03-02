using System;

/// <summary>
/// An upgradable shield generator.
/// </summary>
public class UpgradableShieldGenerator : UpgradableComponent
{
	/// <summary>
	/// The maximum shield capacity.
	/// </summary>
	/// <value>The maximum shield value.</value>
	public int MaxShield { get; private set; }

	/// <summary>
	/// The shield recharge rate.
	/// </summary>
	/// <value>The recharge rate.</value>
	public int MaxRechargeRate { get; private set; }

	public UpgradableShieldGenerator() : base()
	{
		this.Type = ComponentType.ShieldGenerator;
		this.MaxHealth = this.Health = 100; // TODO: read this from GameSettings
	}

	// TODO: balance values

	/// <summary>
	/// Gets the efficiency of the component. Efficiency decreases when the component is damaged.
	/// </summary>
	/// <returns>The efficiency as a value between 0 and 1.</returns>
	public override float GetEfficiency()
	{
		int damage = MaxHealth - Health;
		return (MaxHealth - (damage / 2)) / MaxHealth;
	}

	/// <summary>
	/// Gets the maximum shield capacity at the current health level.
	/// </summary>
	/// <returns>The maximum shield value.</returns>
	public int GetCurrentMaxShield()
	{
		return Convert.ToInt32(GetEfficiency() * MaxShield);
	}

	/// <summary>
	/// Gets the recharge rate at the current health level.
	/// </summary>
	/// <returns>The current recharge rate.</returns>
	public int GetCurrentRechargeRate()
	{
		return Convert.ToInt32(GetEfficiency() * MaxRechargeRate);
	}

	/// <summary>
	/// Upgrades this component to the next level. Even levels give a boost to forward speed,
	/// odd levels give a boost to turning speed.
	/// </summary>
	public override void Upgrade()
	{
		base.Upgrade();

		if (Level % 2 == 0)
			MaxShield = Convert.ToInt32(MaxShield * 1.5);
		else
			MaxRechargeRate = Convert.ToInt32(MaxRechargeRate * 1.5);
	}
}

