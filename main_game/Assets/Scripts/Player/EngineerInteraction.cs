using UnityEngine;
using System.Collections;

public class EngineerInteraction : MonoBehaviour
{
	public bool Upgradeable { get; set; }
	public bool Repairable  { get; set; }
	public ComponentType Type { get; private set; }

    /// <summary>
    /// This automatically sets the component type
    /// based on the name of the object the script
    /// is attached to
    /// </summary>
    public void Initialize()
    {
        // Set the component type based on the parent object's name
        Type = gameObject.name.GetComponentType();
        Upgradeable = Repairable = false;
    }

	public void Start()
	{
        Initialize();
	}
}
