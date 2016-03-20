using UnityEngine;
using System.Collections;

public class SmartBombAbility : CommanderAbility {

    private float originalSpeed;
    [SerializeField] GameObject smartBomb;

	// Use this for initialization
	private void Awake () 
    {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        cooldown = settings.smartBombCooldown;
        state = GameObject.Find("GameManager").GetComponent<GameState>();
	}

	// Update is called once per frame
	private void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.Alpha9))
        {
            UseAbility();
        }
	}

    internal override void ActivateAbility()
    {
        Debug.Log("Smart bomb used");
        Instantiate(smartBomb,state.PlayerShip.transform.position, Quaternion.identity);
    }

    internal override void DeactivateAbility()
    {

    }


}
