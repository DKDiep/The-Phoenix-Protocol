using UnityEngine;
using System.Collections;

public class PortalCollision : MonoBehaviour 
{
	PortalLogic portalLogic;

	void Start()
	{
		portalLogic = GetComponentInChildren<PortalLogic>();
	}
		
	void OnTriggerEnter (Collider col)
	{
			portalLogic.collision();
	}
}

