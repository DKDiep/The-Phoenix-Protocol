/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com

using UnityEngine;
using System.Collections;

public class RotateTransform : MonoBehaviour {
	
	Transform thisTransform;
	
	public Vector3 Speed = new Vector3(0,20f,0);
	
	void Start () {
		thisTransform = transform;
	}
	
	void Update () {
 		Quaternion offset = Quaternion.Euler(Speed * Time.deltaTime);
		thisTransform.localRotation = thisTransform.localRotation*offset;
	}
	
}
