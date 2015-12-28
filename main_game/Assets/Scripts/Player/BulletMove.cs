/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Client side bullet movement
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BulletMove : NetworkBehaviour 
{

  [SyncVar] float speed;

  void Start ()
  {
      if(isServer) speed = GetComponentInChildren<BulletLogic>().speed;
  }

  void Update () 
  {
    transform.position += transform.forward * Time.deltaTime * speed;
  }
}
