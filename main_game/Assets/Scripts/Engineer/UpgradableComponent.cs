using System;

/// <summary>
/// Upgradable component base class.
/// </summary>
public abstract class UpgradableComponent
{
	/// <summary>
	/// The component's upgrade level
	/// </summary>
	/// <value>The level.</value>
	public int Level { get; protected set; }

	/// <summary>
	/// The component's health value.
	/// </summary>
	/// <value>The health value.</value>
	public int Health { get; protected set; }

	/// <summary>
	/// The component's maximum health value.
	/// </summary>
	/// <value>The max health value.</value>
	protected int MaxHealth { get; set; }

	/// <summary>
	/// The component's type.
	/// </summary>
	/// <value>The type.</value>
	public ComponentType Type { get; protected set; }

	protected UpgradableComponent()
	{
		this.Level = 1;
	}

	/// <summary>
	/// Gets the efficiency of the component. Efficiency decreases when the component is damaged.
	/// </summary>
	/// <returns>The efficiency as a value between 0 and 1.</returns>
	public abstract float GetEfficiency();

	/// <summary>
	/// Upgrades this component to the next level.
	/// </summary>
	public virtual void Upgrade()
	{
		Level++;
	}

	/// <summary>
	/// Decreases the component's heatlh.
	/// </summary>
	/// <param name="damage">The damage value to inflict.</param>
	public virtual void Damage(int damage)
	{
		Health -= damage;
		if (Health < 0)
			Health = 0;
	}

	/// <summary>
	/// Repairs this component to full health.
	/// </summary>
	public virtual void Repair()
	{
		Health = MaxHealth;
	}
}

