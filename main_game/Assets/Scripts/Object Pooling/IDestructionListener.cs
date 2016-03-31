using UnityEngine;

/// <summary>
/// Interface describing objects that can listen for the destruction of other objects.
/// </summary>
public interface IDestructionListener
{
	void OnObjectDestructed(GameObject source);
}
