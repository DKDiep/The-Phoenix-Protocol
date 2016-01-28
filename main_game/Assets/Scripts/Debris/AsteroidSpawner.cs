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

    [SerializeField] GameObject asteroid1;
    [SerializeField] GameObject asteroid2;
    [SerializeField] GameObject asteroid3;
    [SerializeField] GameObject gameManager;
    [SerializeField] int maxAsteroids;
    [SerializeField] float maxVariation; // Max variation in size (0-10)
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;

    GameObject player, temp, asteroid;

    public static int numAsteroids = 0;
    private GameState state;

    void Start ()
    {
        // Set game state reference
        if (gameManager != null) state = gameManager.GetComponent<GameState>();
        player = null;
        temp = new GameObject();
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
                temp.transform.position = player.transform.position;
                temp.transform.rotation = Random.rotation;
                temp.transform.Translate(transform.forward * Random.Range(minDistance,maxDistance));

                int rnd = Random.Range(0,3); // Choose which asteroid prefab to spawn

                if(rnd == 0) asteroid = asteroid1;
                else if(rnd == 1) asteroid = asteroid2;
                else asteroid = asteroid3;

                GameObject asteroidObject = Instantiate(asteroid, temp.transform.position, Quaternion.identity) as GameObject;
    			GameObject asteroidLogic = Instantiate(Resources.Load("Prefabs/AsteroidLogic", typeof(GameObject))) as GameObject;
    			asteroidLogic.transform.parent = asteroidObject.transform;
                asteroidLogic.transform.localPosition = Vector3.zero;
                asteroidLogic.GetComponent<AsteroidLogic>().SetPlayer(state.GetPlayerShip(), maxVariation, rnd);
                asteroidLogic.GetComponent<AsteroidLogic>().SetStateReference(state);
                asteroidObject.AddComponent<AsteroidCollision>();
    			SphereCollider sphere = asteroidObject.AddComponent<SphereCollider>();
    			sphere.isTrigger = true;
    			Rigidbody rigid = asteroidObject.AddComponent<Rigidbody>();
    			rigid.isKinematic = true;
                state.AddAsteroidList(asteroidObject);
    			ServerManager.NetworkSpawn(asteroidObject);
                numAsteroids += 1;
            }
        }
    }

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
