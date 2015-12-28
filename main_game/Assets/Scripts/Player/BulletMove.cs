using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BulletMove : NetworkBehaviour 
{

  [SyncVar] float speed;

  void Start ()
  {
      if(isServer)
       {
            speed = GetComponentInChildren<BulletLogic>().speed;
       }
  }
	
	// Update is called once per frame
  // Update is called once per frame
  void Update () 
  {
    transform.position += transform.forward * Time.deltaTime * speed;
  }
}
