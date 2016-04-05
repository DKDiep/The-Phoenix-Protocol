using UnityEngine;
using System.Collections;

public class ResourcesCollision : MonoBehaviour {

	// The outpost that these resources are attached to
	private GameObject outpost;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	// Collect resources from outpost on collision
	void OnTriggerEnter (Collider col)
	{
		outpost.GetComponentInChildren<OutpostLogic>().ResourceCollision();
	}

	/// <summary>
	/// Sets the outpost that contains these resources.
	/// </summary>
	/// <param name="outpost">The outpost.</param>
	public void SetOutpost(GameObject outpost)
	{
		this.outpost = outpost;
	}
}
