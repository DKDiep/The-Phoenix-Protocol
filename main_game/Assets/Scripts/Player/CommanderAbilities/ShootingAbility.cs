using UnityEngine;
using System.Collections;

public class ShootingAbility : CommanderAbility {

    [SerializeField] float readyDelay;
    private GameObject shootAnchor;

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
            if(shootAnchor == null)
                shootAnchor = GameObject.Find("CommanderShootAnchor");
            UseAbility();
        }
	}

    internal override void AbilityEffect()
    {
        Debug.Log("Commander Shoot");
    }


}
