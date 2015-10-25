//// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


/// CopyRotationValue.cs
/// Copy the targets transforms rotational value.

using UnityEngine;
using System.Collections;

namespace ProFlares {
	public class CopyRotationValue : MonoBehaviour {
		public Transform target;
		
		Transform thisTrans;
		void Start () {
			thisTrans = transform;
			if(target == null)
				this.enabled = false;
		}
		
		void LateUpdate () {
			
			thisTrans.localRotation = target.rotation;
		}
	}
}
