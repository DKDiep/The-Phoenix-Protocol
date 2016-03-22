using System;

/// <summary>
/// An upgradable resource storage facility.
/// </summary>
public class UpgradableResourceStorage : UpgradableComponent
{
	/// <summary>
	/// The percentage of extra resources received when collecting resources.
	/// </summary>
	/// <value>The bonus value.</value>
	public float CollectionBonus { get; private set; }

	/// <summary>
	/// The resource interest rate. Interest rate regularly increases the player's resources based on the amount already in storage
	/// </summary>
	/// <value>The interest rate.</value>
	public float InterestRate { get; private set; }

	/// <summary>
	/// Initializes a new <see cref="UpgradableResourceStorage"/>.
	/// </summary>
	/// <param name="initialCollectionBonus">The initial resource collection bonus percentage.</param>
	/// <param name="initialInterestRate">The initial interest rate.</param>
	public UpgradableResourceStorage(float initialCollectionBonus, float initialInterestRate) : base()
	{
		this.Type 			 = ComponentType.ResourceStorage;
		this.MaxHealth 		 = this.Health = 100; // Currently, this can't be damaged
		this.CollectionBonus = initialCollectionBonus;
		this.InterestRate    = initialInterestRate;
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

		// TODO: maybe move these to GameSettings?
		if (Level % 2 == 0)
			CollectionBonus += 0.15f;
		else
			InterestRate += 0.05f;
	}
}

