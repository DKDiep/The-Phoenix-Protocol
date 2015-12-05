using UnityEngine;
using System.Collections;

public class EngineerSpawner : MonoBehaviour {

    [SerializeField]
    GameObject engineerPrefab;
    [SerializeField]
    GameObject gameManager;

    private GameState gameState;
    private ServerManager serverManager;
    private const int maxEngineers = 1;
    private int numEngineers = 0;

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
        if (gameState.GetStatus() == GameState.Status.Started && numEngineers <= maxEngineers)
        {
            GameObject engineer = (GameObject)Instantiate(engineerPrefab, new Vector3(0,0,0), Quaternion.identity);
            ServerManager.NetworkSpawn(engineer);
            numEngineers++;
        }
	}
}
