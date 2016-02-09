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
  GameObject player;
  [SerializeField] Mesh highPoly;
  [SerializeField] Mesh medPoly;
  [SerializeField] Mesh lowPoly;
  MeshFilter myFilter;

  // Only one packet needs to be sent to the client to control the asteroid's rotation
  void Start ()
  {
    if(isServer) speed = GetComponentInChildren<AsteroidLogic>().speed;
    player = GameObject.Find("PlayerShip(Clone)");
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
