using UnityEngine;
using System.Collections;

public class Shooting : CommanderAbility {

    [SerializeField] float readyDelay;

	// Use this for initialization
	private void Awake () 
    {
        cooldown = readyDelay;
	}

	// Update is called once per frame
	private void Update () 
    {
	    if(Input.GetMouseButtonDown(0))
        {
            UseAbility();
        }
	}

    internal override void AbilityEffect()
    {
        Debug.Log("Shoot");
    }


}
