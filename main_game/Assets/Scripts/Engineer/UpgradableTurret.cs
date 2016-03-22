using System;

/// <summary>
/// An upgradable turret.
/// </summary>
public class UpgradableTurret : UpgradableComponent
{
	// TODO: currently, we cannot easily change the damage of the bullets, so only the fire rate is used.

	/// <summary>
	/// The maximum damage per shot.
	/// </summary>
	/// <value>The maximum damage.</value>
	public int MaxDamage { get; private set; }

	/// <summary>
	/// The base firing delay.
	/// </summary>
	/// <value>The firing delay.</value>
	public float MinFireDelay { get; private set; }

	/// <summary>
	/// Initializes a new <see cref="UpgradableTurret"/>.
	/// </summary>
	/// <param name="initialDelay">The initial firing delay.</param>
	public UpgradableTurret(float initialDelay) : base()
	{
		this.Type         = ComponentType.Turret;
		this.MaxHealth    = this.Health = 100; // TODO: read this from GameSettings
		this.MinFireDelay = initialDelay;
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
	/// Gets the firing delay at the current health level.
	/// </summary>
	/// <returns>The current firring delay.</returns>
	public float GetCurrentFireDelay()
	{
		return MinFireDelay / GetEfficiency();
	}

	/// <summary>
	/// Upgrades this component to the next level. Even levels give a boost to forward speed,
	/// odd levels give a boost to turning speed.
	/// </summary>
	public override void Upgrade()
	{
		base.Upgrade();

		// TODO: the damage is currently not upgraded
		/*if (Level % 2 == 0)
			MaxDamage = Convert.ToInt32(MaxDamage * 1.5);
		else*/
		MinFireDelay = MinFireDelay / 1.5f;
	}
}

