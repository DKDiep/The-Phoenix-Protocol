using UnityEngine;
using System.Collections;

public class SetCanvasCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    GetComponent<Canvas>().worldCamera = Camera.main;
	}

}
