/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com

/// <summary>
/// Zoom.cs
/// Attach to a transform, which will then move in on the Z Axis when the mouse wheel is used.
/// </summary>
using UnityEngine;
using System.Collections;

namespace ProFlares{
	public class Zoom : MonoBehaviour {
		
		Transform thisTrans;
		
		public float current;
		
		public float prev;
		
		void Start () {
			thisTrans = transform;
		}
		
		public float pos = 0;
		
		public float dif;
		
		public float offset;
		
		void Update () {
	   		prev = current;
		    current = Input.GetAxis("Mouse ScrollWheel"); 
			
			if(Input.GetKey(KeyCode.UpArrow))
				current = 0.1f;
			
			if(Input.GetKey(KeyCode.DownArrow))
				current = -0.1f;
			
		 	dif = (prev-current)*-0.3f;
			pos = Mathf.Clamp(pos + dif,-1f,1f);
	
			Vector3 newPos = thisTrans.localPosition;
			newPos.z = Mathf.Clamp(thisTrans.localPosition.z+current,-2f,3f);
			thisTrans.localPosition = newPos;
		}
	}
}
