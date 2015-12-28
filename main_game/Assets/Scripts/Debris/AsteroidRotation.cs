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

  [SyncVar] float speed;

  void Start ()
  {
    if(isServer) speed = GetComponentInChildren<AsteroidLogic>().speed;
  }
	
  void Update()
  {
    transform.Rotate(transform.forward * speed * Time.deltaTime);
  }

}
