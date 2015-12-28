using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class AsteroidRotation : NetworkBehaviour
{

  [SyncVar] float speed;

  void Start ()
  {
      if(isServer)
       {
            speed = GetComponentInChildren<AsteroidLogic>().speed;
       }
  }
	
	// Update is called once per frame
  void Update()
  {
    transform.Rotate(transform.forward * speed * Time.deltaTime);
  }
}
