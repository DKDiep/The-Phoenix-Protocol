using System;

/// <summary>
/// An upgradable drone.
/// </summary>
public class UpgradableDrone : UpgradableComponent
{
	/// <summary>
	/// The maximum drone's speed.
	/// </summary>
	/// <value>The movement speed.</value>
	public int MovementSpeed { get; private set; }

	/// <summary>
	/// The maximum drone's repair/upgrade speed.
	/// </summary>
	/// <value>The work speed.</value>
	public int ImprovementSpeed { get; private set; }

	public UpgradableDrone() : base()
	{
		this.Type = ComponentType.Turret;
		this.MaxHealth = this.Health = 100; // TODO: read this from GameSettings
	}

	// TODO: balance values

	/// <summary>
	/// Gets the efficiency of the component. Efficiency does not decrease when the drone is damaged.
	/// </summary>
	/// <returns>1.</returns>
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
			MovementSpeed = Convert.ToInt32(MovementSpeed * 1.5);
		else
			ImprovementSpeed = Convert.ToInt32(ImprovementSpeed * 1.5);
	}
}

