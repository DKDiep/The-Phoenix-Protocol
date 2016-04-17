using UnityEngine;
using System.Collections;

public class ShootManager : MonoBehaviour {

    private PlayerShooting[] shooting;

	// Use this for initialization
	void Start () 
    {
        shooting = new PlayerShooting[4];
        foreach(Transform child in this.transform)
        {
            //shooting[int.TryParse(child.name, )] = child.gameObject.GetComponent<PlayerShooting>();
        }
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
