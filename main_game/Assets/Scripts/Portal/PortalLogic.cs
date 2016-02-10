using UnityEngine;
using System.Collections;

public class PortalLogic : MonoBehaviour {

	public float distanceToWin = 0.9f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void collision() 
	{
		Debug.Log("You have reached the portal. You have won then game!");
	}
}
