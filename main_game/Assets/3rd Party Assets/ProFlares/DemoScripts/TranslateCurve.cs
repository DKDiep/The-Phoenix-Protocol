/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


using UnityEngine;
using System.Collections;

namespace ProFlares {
	public class TranslateCurve : MonoBehaviour {
		Transform thisTransform;
		Vector3 pos;
		public float speed = 0.3f;
		public WrapMode wrapMode;
		public Vector3 axis = Vector3.one;
		
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0.1f), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.1f));
		void Start () {
			thisTransform = transform;
			pos = thisTransform.localPosition;
			Curve.postWrapMode = wrapMode;
		}
		
		void Update () {
			thisTransform.transform.localPosition = pos+(axis*Curve.Evaluate(Time.time*speed));
		}
	}
}
