/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Dillon Keith Diep, Andrei Poenaru, Marc Steene
    Description: Server-side logic for asteroid spawner
*/

using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour 
{
    [SerializeField] GameObject asteroid1; // 3 asteroid objects for the 3 different models
    [SerializeField] GameObject asteroid2;
    [SerializeField] GameObject asteroid3;
    [SerializeField] GameObject gameManager;
    [SerializeField] int maxAsteroids; // Maximum number of asteroids that can exist simultaneously
    [SerializeField] float maxVariation; // Max variation in size (0-10)
    [SerializeField] float minDistance; // Minimum distance to the player that an asteroid can spawn
    [SerializeField] float maxDistance; // Maximum distance to the player that an asteroid can spawn

    GameObject player, temp, asteroid, logic;
    public static int numAsteroids = 0;
    private GameState state;

    void Start ()
    {
        // Set game state reference
        if (gameManager != null) state = gameManager.GetComponent<GameState>();
        player = null;
        temp = new GameObject(); // A temporary game object to spawn asteroids on
        logic = Instantiate(Resources.Load("Prefabs/AsteroidLogic", typeof(GameObject))) as GameObject;
        temp.name = "AsteroidSpawnLocation";
        StartCoroutine("Cleanup");
    }

    void Update () 
    {
        if (state.GetStatus() == GameState.Status.Started)
        {
            if(player == null) player = state.GetPlayerShip();

            // Spawn a new asteroid in a random position if there are less than specified by maxAsteroids
            if (numAsteroids < maxAsteroids)
            {
                // The temp object is positioned randomly within the bounds set by minDistance and maxDistance
                temp.transform.position = player.transform.position;
                temp.transform.rotation = Random.rotation;
                temp.transform.Translate(transform.forward * Random.Range(minDistance,maxDistance));

                int rnd = Random.Range(0,3); // Choose which asteroid prefab to spawn

                if(rnd == 0) asteroid = asteroid1;
                else if(rnd == 1) asteroid = asteroid2;
                else asteroid = asteroid3;

                // Spawn object and logic
                GameObject asteroidObject = Instantiate(asteroid, temp.transform.position, Quaternion.identity) as GameObject;
                GameObject asteroidLogic = Instantiate(logic, temp.transform.position, Quaternion.identity) as GameObject;

                // Initialise logic
    			asteroidLogic.transform.parent = asteroidObject.transform;
                asteroidLogic.transform.localPosition = Vector3.zero;
                asteroidObject.AddComponent<AsteroidCollision>();
                asteroidLogic.GetComponent<AsteroidLogic>().SetPlayer(state.GetPlayerShip(), maxVariation, rnd);
                asteroidLogic.GetComponent<AsteroidLogic>().SetStateReference(state);

                // Add collider and rigidbody
    			SphereCollider sphere = asteroidObject.AddComponent<SphereCollider>();
    			sphere.isTrigger = true;
    			Rigidbody rigid = asteroidObject.AddComponent<Rigidbody>();
    			rigid.isKinematic = true;

                // Spawn on the network and add to GameState
                state.AddAsteroidList(asteroidObject);
    			ServerManager.NetworkSpawn(asteroidObject);
                numAsteroids += 1;
            }
        }
    }

    // Remove asteroid from GameState if destroyed
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);
        if (state.GetStatus() == GameState.Status.Started)
        {
            for (int i = state.GetAsteroidListCount() - 1; i >= 0; i--)
            {
                GameObject asteroidObject = state.GetAsteroidAt(i);
                if(asteroidObject == null)
                {
                    state.RemoveAsteroidAt(i);
                }
            }
        }
        StartCoroutine("Cleanup");
    }
}
