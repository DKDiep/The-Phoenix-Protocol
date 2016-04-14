using System;

/// <summary>
/// An upgradable hull.
/// </summary>
public class UpgradableHull : UpgradableComponent
{
    private float MaxHealthUpgradeRate;
    public UpgradableHull(float MaxHealthUpgradeRate) : base()
	{
		this.Type = ComponentType.Hull;
		this.MaxHealth = this.Health = 100; // TODO: read this from GameSettings
        this.MaxHealthUpgradeRate = MaxHealthUpgradeRate;
	}

	// TODO: balance values

	/// <summary>
	/// Gets the efficiency of the component. Efficiency does not decreases when the hull is damaged.
	/// </summary>
	/// <returns>1.</returns>
	public override float GetEfficiency()
	{
		return 1; // The hull doesn't lose efficiency with damage
	}
		
	/// <summary>
	/// Upgrades this component to the next level. Even levels give a boost to forward speed,
	/// odd levels give a boost to turning speed.
	/// </summary>
	public override void Upgrade()
	{
		base.Upgrade();

        MaxHealth *= (int)MaxHealthUpgradeRate;
	}
}

