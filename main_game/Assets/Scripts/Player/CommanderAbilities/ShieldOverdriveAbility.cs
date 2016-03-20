using UnityEngine;
using System.Collections;

public class ShieldOverdriveAbility : CommanderAbility {

    private float originalShield;
    private bool abilityActive = false;

	// Use this for initialization
	private void Awake () 
    {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        cooldown = settings.shieldOverdriveCooldown;
        duration = settings.shieldOverdriveDuration;
        state = GameObject.Find("GameManager").GetComponent<GameState>();
	}

	// Update is called once per frame
	private void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.Alpha7))
        {
            UseAbility();
        }

        if(abilityActive)
            state.SetShipShield(100f);
	}

    internal override void ActivateAbility()
    {
        originalShield = state.GetShipShield();
        abilityActive = true;
    }

    internal override void DeactivateAbility()
    {
        abilityActive = false;
        state.SetShipShield(originalShield);
    }


}
