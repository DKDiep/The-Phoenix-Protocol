/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com

using UnityEngine;
using System.Collections;
namespace ProFlares {
public class AddForceToTarget : MonoBehaviour {
	public Transform target;
	public float force;
	
	public ForceMode mode;
	 
	void FixedUpdate () {
		 
		 
			float dist = (Vector3.Distance(transform.position,target.position)*0.01f);
		
 
			Vector3 dir = target.position-transform.position;
#if UNITY_5_0		 
		 	GetComponent<Rigidbody>().AddForce(dir*(force*dist),mode);
#else
			GetComponent<Rigidbody>().AddForce(dir*(force*dist),mode);
#endif
		
	}
}
}