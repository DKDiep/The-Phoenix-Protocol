/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Causes client-side rotation of asteroid based on speed sent from server
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class AsteroidRotation : NetworkBehaviour
{
	[SyncVar] private float speed;
	private GameObject player;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields	
	[SerializeField] private Mesh highPoly;
	[SerializeField] private Mesh medPoly;
	[SerializeField] private Mesh lowPoly;
	#pragma warning restore 0649

	private MeshFilter myFilter;

  // Only one packet needs to be sent to the client to control the asteroid's rotation
	void Start ()
	{
	    if(isServer)
			speed = GetComponentInChildren<AsteroidLogic>().speed;
	    
			player   = GameObject.Find("PlayerShip(Clone)");
	   
		myFilter = GetComponent<MeshFilter>();
		StartCoroutine("AsteroidLOD");
	}
	
	void Update()
	{
		transform.Rotate(transform.forward * speed * Time.deltaTime);
	}

	IEnumerator AsteroidLOD()
	{
		float distance = Vector3.Distance(transform.position, player.transform.position);
		if(distance < 250)
		    myFilter.mesh = highPoly;
		else if(distance < 500)
		    myFilter.mesh = medPoly;
		else
		    myFilter.mesh = lowPoly;

		yield return new WaitForSeconds(1f);
		StartCoroutine("AsteroidLOD");
	}
}
