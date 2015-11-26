using UnityEngine;
using System.Collections;

public class EngineerSpawner : MonoBehaviour {

    [SerializeField]
    GameObject engineer;
    [SerializeField]
    GameObject gameManager;

    private GameState gameState;
    private ServerManager serverManager;

	// Use this for initialization
	void Start ()
    {
        if (gameManager != null)
        {
            gameState = gameManager.GetComponent<GameState>();
            serverManager = gameManager.GetComponent<ServerManager>();
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
