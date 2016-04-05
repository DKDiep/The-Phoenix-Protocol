using UnityEngine;
using System.Collections;

public class BoostAbility : CommanderAbility {

	// Use this for initialization
	private void Awake () 
    {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        cooldown = settings.boostCooldown;
        duration = settings.boostDuration;
        state = GameObject.Find("GameManager").GetComponent<GameState>();
	}

	// Update is called once per frame
	private void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.Alpha6))
        {
            UseAbility();
        }
	}

    internal override void ActivateAbility()
    {
        Debug.Log("Boost ability enabled");
        AIVoice.SendCommand(5);
        float originalSpeed = state.GetShipSpeed();
		state.ActivateBoost(originalSpeed * settings.boostSpeedMultiplier);
    }

    internal override void DeactivateAbility()
    {
        Debug.Log("Boost ability disabled");
		state.DeactivateBoost();
    }


}
