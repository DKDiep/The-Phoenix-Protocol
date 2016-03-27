using UnityEngine;

/// <summary>
/// Interface describing objects that can generate destruction events and send to registered listeners.
/// </summary>
public interface DestructibleObject
{
	void RegisterDestructionListener(DestructionListener listener);
}
