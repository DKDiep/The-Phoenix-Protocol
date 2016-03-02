using System;

/// <summary>
/// An upgradable turret.
/// </summary>
public class UpgradableTurret : UpgradableComponent
{
	/// <summary>
	/// The maximum damage per shot.
	/// </summary>
	/// <value>The maximum damage.</value>
	public int MaxDamage { get; private set; }

	/// <summary>
	/// The maximum fire rate.
	/// </summary>
	/// <value>The fire rate.</value>
	public int MaxFireRate { get; private set; }

	public UpgradableTurret() : base()
	{
		this.Type = ComponentType.Turret;
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
	/// Gets the damage per shot at the current health level.
	/// </summary>
	/// <returns>The maximum shield value.</returns>
	public int GetCurrentDamage()
	{
		return Convert.ToInt32(GetEfficiency() * MaxDamage);
	}

	/// <summary>
	/// Gets the firing rate at the current health level.
	/// </summary>
	/// <returns>The current recharge rate.</returns>
	public int GetCurrentFireRate()
	{
		return Convert.ToInt32(GetEfficiency() * MaxFireRate);
	}

	/// <summary>
	/// Upgrades this component to the next level. Even levels give a boost to forward speed,
	/// odd levels give a boost to turning speed.
	/// </summary>
	public override void Upgrade()
	{
		base.Upgrade();

		if (Level % 2 == 0)
			MaxDamage = Convert.ToInt32(MaxDamage * 1.5);
		else
			MaxFireRate = Convert.ToInt32(MaxFireRate * 1.5);
	}
}

