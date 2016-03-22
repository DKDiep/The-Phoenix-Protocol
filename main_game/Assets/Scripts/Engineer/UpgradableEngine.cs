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
	public float MaxSpeed { get; private set; }

	/// <summary>
	/// The maximum turning speed.
	/// </summary>
	/// <value>The turning speed.</value>
	public float MaxTurningSpeed { get; private set; }

	/// <summary>
	/// Initializes a new <see cref="UpgradableEngine"/>.
	/// </summary>
	/// <param name="initialSpeed">The initial ship speed.</param>
	/// <param name="initialTurningSpeed">The initial ship turning speed.</param>
	public UpgradableEngine(float initialSpeed, float initialTurningSpeed) : base()
	{
		this.Type 	   		 = ComponentType.Engine;
		this.MaxHealth 		 = this.Health = 100; // TODO: read this from GameSettings
		this.MaxSpeed  		 = initialSpeed;
		this.MaxTurningSpeed = initialTurningSpeed;
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
	public float GetCurrentSpeed()
	{
		return GetEfficiency() * MaxSpeed;
	}

	/// <summary>
	/// Gets the turning speed at the current health level.
	/// </summary>
	/// <returns>The current turning speed.</returns>
	public float GetCurrentTurningSpeed()
	{
		return GetEfficiency() * MaxTurningSpeed;
	}

	/// <summary>
	/// Upgrades this component to the next level. Even levels give a boost to forward speed,
	/// odd levels give a boost to turning speed.
	/// </summary>
	public override void Upgrade()
	{
		base.Upgrade();

		if (Level % 2 == 0)
			MaxSpeed = MaxSpeed * 1.5f;
		else
			MaxTurningSpeed = MaxTurningSpeed * 1.5f;
	}
}

