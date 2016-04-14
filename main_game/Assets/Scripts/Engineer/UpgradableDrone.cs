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
	public float MovementSpeed { get; private set; }

	/// <summary>
	/// The drone's repair/upgrade time.
	/// </summary>
	/// <value>The work time.</value>
	public float ImprovementTime { get; private set; }

    private float ImprovementTimeUpgradeRate;
    private float MovementSpeedUpgradeRate;

	/// <summary>
	/// Initializes a new <see cref="UpgradableDrone"/>.
	/// </summary>
	/// <param name="initialMovementSpeed">The initial movement speed.</param>
	/// <param name="initialWorkTime">The initial work time.</param>
    public UpgradableDrone(float initialMovementSpeed, float initialWorkTime, float MovementSpeedUpgradeRate, float ImprovementTimeUpgradeRate) : base()
	{
		this.Type 			 = ComponentType.Turret;
		this.MaxHealth 		 = this.Health = 100; // TODO: read this from GameSettings
		this.MovementSpeed   = initialMovementSpeed;
		this.ImprovementTime = initialWorkTime;
        this.MovementSpeedUpgradeRate = MovementSpeedUpgradeRate;
        this.ImprovementTimeUpgradeRate = ImprovementTimeUpgradeRate;
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
            MovementSpeed *= MovementSpeedUpgradeRate;
		else
            ImprovementTime /= ImprovementTimeUpgradeRate;
	}
}

