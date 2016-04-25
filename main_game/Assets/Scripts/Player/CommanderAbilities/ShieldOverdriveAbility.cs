using UnityEngine;
using System.Collections;

public class ShieldOverdriveAbility : CommanderAbility {

    private float originalShield;
    private bool abilityActive = false;
    private ShieldEffects shieldEffects;

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

        if(abilityActive)
            state.SetShipShield(100f);
	}

    internal override void ActivateAbility()
    {
        if(shieldEffects == null)
                shieldEffects = state.PlayerShip.GetComponentInChildren<ShieldEffects>();

        Debug.Log("Shield ability enabled");
        AIVoice.SendCommand(6);
        shieldEffects.ActivateOverdrive();
        originalShield = state.GetShipShield();
        abilityActive = true;
    }

    internal override void DeactivateAbility()
    {
        Debug.Log("Shield ability disabled");
        shieldEffects.overdriveEnabled = false;
        abilityActive = false;
        state.SetShipShield(originalShield);
    }


}
