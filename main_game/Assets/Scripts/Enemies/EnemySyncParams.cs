using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class EnemySyncParams : NetworkBehaviour
{

	[SyncVar] private bool isHacked;

	/// <summary>
	/// Sets whether this enemy is hacked.
	/// </summary>
	/// <param name="hacked">If set to <c>true</c>, hacked.</param>
	public void SetHacked(bool hacked)
	{
		isHacked = hacked;
	}

	/// <summary>
	/// Gets whether this enemy is hacked.
	/// </summary>
	/// <returns><c>true</c> if this enemy is hacked.</returns>
	public bool GetHacked()
	{
		return isHacked;
	}
}
