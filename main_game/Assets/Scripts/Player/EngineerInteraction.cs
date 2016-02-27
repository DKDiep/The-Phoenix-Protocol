using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class EngineerInteraction : NetworkBehaviour
{
	public bool Upgradeable { get; set; }
	public bool Repairable  { get; set; }
	public ComponentType Type { get; private set; }

	public void Start()
	{
		// Set the component type based on the parent object's name
		Type = gameObject.name.GetComponentType();

		Upgradeable = Repairable = false;
	}
}
