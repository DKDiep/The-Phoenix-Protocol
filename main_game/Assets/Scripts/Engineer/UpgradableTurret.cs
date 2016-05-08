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
	/// The base firing delay.
	/// </summary>
	/// <value>The firing delay.</value>
	public float MinFireDelay { get; private set; }

	private float maxDamageUpgradeRate;
	private float minFireDelayUpgradeRate;

	/// <summary>
	/// Initializes a new <see cref="UpgradableTurret"/>.
	/// </summary>
	/// <param name="initialDelay">The initial firing delay.</param>
    public UpgradableTurret(float initialDelay, int initialDamage, float maxDamageUpgradeRate, float minFireDelayUpgradeRate) : base()
	{
		this.Type         = ComponentType.Turret;
		this.MaxHealth    = this.Health = 100; // TODO: read this from GameSettings
		this.MinFireDelay = initialDelay;
		this.MaxDamage    = initialDamage;

        this.maxDamageUpgradeRate    = maxDamageUpgradeRate;
        this.minFireDelayUpgradeRate = minFireDelayUpgradeRate;
	}

	// TODO: balance values

	/// <summary>
	/// Gets the efficiency of the component. Efficiency decreases when the component is damaged.
	/// </summary>
	/// <returns>The efficiency as a value between 0 and 1.</returns>
	public override float GetEfficiency()
	{
		float damage = MaxHealth - Health;

		return (MaxHealth - (damage / 2)) / MaxHealth;
	}

	/// <summary>
	/// Gets the damage per shot at the current health level.
	/// </summary>
	/// <returns>The maximum shield value.</returns>
	public float GetCurrentDamage()
	{
		return GetEfficiency() * MaxDamage;
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
        
            MaxDamage *= (int)maxDamageUpgradeRate;
            MinFireDelay = MinFireDelay / minFireDelayUpgradeRate;
	}
}

