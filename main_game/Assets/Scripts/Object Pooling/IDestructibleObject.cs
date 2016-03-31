using UnityEngine;

/// <summary>
/// Interface describing objects that can generate destruction events and send to registered listeners.
/// </summary>
public interface IDestructibleObject
{
	void RegisterDestructionListener(IDestructionListener listener);
}
