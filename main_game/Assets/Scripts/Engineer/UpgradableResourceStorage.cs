using System;

/// <summary>
/// An upgradable resource storage facility.
/// </summary>
public class UpgradableResourceStorage : UpgradableComponent
{
	/// <summary>
	/// The extra amount of resources received when collecting resources.
	/// </summary>
	/// <value>The bonus value.</value>
	public int CollectionBonus { get; private set; }

	/// <summary>
	/// The resource interest rate. Interest rate regularly increases the player's resources based on the amount already in storage
	/// </summary>
	/// <value>The interest rate.</value>
	public int InterestRate { get; private set; }

	public UpgradableResourceStorage() : base()
	{
		this.Type = ComponentType.Turret;
		this.MaxHealth = this.Health = 100; // TODO: read this from GameSettings
	}

	// TODO: balance values

	/// <summary>
	/// Gets the efficiency of the component. Efficiency does not decrease when the resource storage is damaged.
	/// </summary>
	/// <returns>The efficiency as a value between 0 and 1.</returns>
	public override float GetEfficiency()
	{
		return 1;
	}

	/// <summary>
	/// Upgrades this component to the next level. Even levels give a boost to forward speed,
	/// odd levels give a boost to turning speed.
	/// </summary>
	public override void Upgrade()
	{
		base.Upgrade();

		if (Level % 2 == 0)
			CollectionBonus = Convert.ToInt32(CollectionBonus * 1.5);
		else
			InterestRate = Convert.ToInt32(InterestRate * 1.5);
	}
}

