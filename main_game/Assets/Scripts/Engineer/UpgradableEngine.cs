using System;

/// <summary>
/// An upgradable engine.
/// </summary>
public class UpgradableEngine : UpgradableComponent
{
	/// <summary>
	/// The maximum forwards speed.
	/// </summary>
	/// <value>The speed.</value>
	public int MaxSpeed { get; private set; }

	/// <summary>
	/// The maximum turning speed.
	/// </summary>
	/// <value>The turning speed.</value>
	public int MaxTurningSpeed { get; private set; }

	public UpgradableEngine() : base()
	{
		this.Type = ComponentType.Engine;
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
	/// Gets the speed at the current health level.
	/// </summary>
	/// <returns>The current speed.</returns>
	public int GetCurrentSpeed()
	{
		return Convert.ToInt32(GetEfficiency() * MaxSpeed);
	}

	/// <summary>
	/// Gets the turning speed at the current health level.
	/// </summary>
	/// <returns>The current turning speed.</returns>
	public int GetCurrentTurningSpeed()
	{
		return Convert.ToInt32(GetEfficiency() * MaxTurningSpeed);
	}

	/// <summary>
	/// Upgrades this component to the next level. Even levels give a boost to forward speed,
	/// odd levels give a boost to turning speed.
	/// </summary>
	public override void Upgrade()
	{
		base.Upgrade();

		if (Level % 2 == 0)
			MaxSpeed = Convert.ToInt32(MaxSpeed * 1.5);
		else
			MaxTurningSpeed = Convert.ToInt32(MaxTurningSpeed * 1.5);
	}
}

