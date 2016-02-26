using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class OutPostManager : MonoBehaviour {

    private GameState gameState;
    private PlayerController playerController;
    private float timeSinceLastEvent = 0;

	// Use this for initialization
	void Start () {
        //print("OutpostManager started");
        GameObject playerControllerObject = GameObject.Find("PlayerController(Clone)");
        playerController = playerControllerObject.GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
        timeSinceLastEvent += Time.deltaTime;
        if (timeSinceLastEvent > 10)
        {
            //print("outpost list time");
            List<GameObject> outpostList = gameState.GetOutpostList();
            if (outpostList != null)
            {
                //print("outpostlist !=null");
                playerController.RpcOutpostNotification("Outpost found");
            }
            timeSinceLastEvent = 0;
        }
    }

    public void giveGameStateReference(GameState newGameState)
    {
        gameState = newGameState;
    }
}