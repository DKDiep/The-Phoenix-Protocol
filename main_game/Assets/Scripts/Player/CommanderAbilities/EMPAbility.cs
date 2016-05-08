using UnityEngine;
using System.Collections;

public class EMPAbility : CommanderAbility {

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] GameObject emp;
	#pragma warning restore 0649

	// Use this for initialization
	private void Awake () 
    {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        cooldown = settings.empCooldown;
        state = GameObject.Find("GameManager").GetComponent<GameState>();
	}

	// Update is called once per frame
	private void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.Alpha8))
        {
            UseAbility();
        }
	}

    internal override void ActivateAbility()
    {
        // Debug.Log("EMP used");
        AIVoice.SendCommand(7);
        GameObject temp = Instantiate(emp,state.PlayerShip.transform.position, Quaternion.identity) as GameObject;
        ServerManager.NetworkSpawn(temp);
    }

    internal override void DeactivateAbility()
    {

    }
}
