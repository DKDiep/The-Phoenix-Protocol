using UnityEngine;
using System.Collections;

public class EngineerInteraction : MonoBehaviour {
    private bool upgradeable = false;
    private bool repairable = false;
	public ComponentType Type { get; private set; }

	public void Start()
	{
		// Set the component type based on the parent object's name
		Type = gameObject.name.GetComponentType();
	}

    public void setUpgradeable(bool value)
    {
        upgradeable = value;
    }

    public bool getUpgradeable()
    {
        return this.upgradeable;
    }

    public void setRepairable(bool value)
    {
        repairable = value;
    }

    public bool getRepairable()
    {
        return this.repairable;
    }
}
