using UnityEngine;
using System.Collections;

public class MothershipLogic : MonoBehaviour {

    private float health;
    GameSettings settings;
    GameState gameState;


	// Use this for initialization
	void Start () {

        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        LoadSettings();

        GameObject server = settings.GameManager;
        gameState         = server.GetComponent<GameState>();
	
	}
	
    private void LoadSettings()
    {
        health = settings.GlomMothershipHealth;
    }

    // Detect collisions with other game objects
    public void collision(float damage, int playerId)
    {
            if(health > damage)
            {
                health -= damage;
                Debug.Log(health);
            }
            else if (transform.parent != null) // The null check prevents trying to destroy an object again while it's already being destroyed
            {
                Debug.Log("Glom mothership destroyed");
                /*if(playerId != -1)
                {
                    // Update player score
                    gameState.AddToPlayerScore(playerId, 10);
                }
                // Automatically collect resources from enemy ship
                gameState.AddShipResources(droppedResources);

                // Destroy Object
                GameObject temp = explosionManager.RequestObject();
                temp.transform.position = transform.position;

                explosionManager.EnableClientObject(temp.name, temp.transform.position, temp.transform.rotation, temp.transform.localScale);*/
            }
    }
}
