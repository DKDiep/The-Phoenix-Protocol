using UnityEngine;
using System.Collections;

public class DestroyDebris : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		StartCoroutine("DestroyMe");
	}

	IEnumerator DestroyMe()
	{
		yield return new WaitForSeconds(10f);
		Destroy(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
