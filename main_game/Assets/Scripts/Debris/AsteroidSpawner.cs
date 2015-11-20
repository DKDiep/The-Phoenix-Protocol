using UnityEngine;
using System.Collections;

public class AsteroidSpawner : MonoBehaviour 
{

	[SerializeField] GameObject asteroid;
	[SerializeField] int maxAsteroids;
	[SerializeField] float maxVariation; // Max variation in size (0-10)
	public static int numAsteroids = 0;

    [SerializeField] GameObject gameManager;
    private GameState state;
	
	void Start ()
    {
        if (gameManager != null)
            state = gameManager.GetComponent<GameState>();
        StartCoroutine("Cleanup");
    }
    
    // Spawn a new asteroid in a random position if there are less than specified by maxAsteroids
	void Update () 
	{
		if(numAsteroids < maxAsteroids)
		{
			Vector3 rand_position = new Vector3(transform.position.x + Random.Range (-800, 800), transform.position.y + Random.Range (-800, 800), transform.position.z + 150 + Random.Range (50, 1000));
			GameObject asteroidObject = Instantiate (asteroid, rand_position, Quaternion.identity) as GameObject;
			//asteroidObject.GetComponent<AsteroidLogic>().SetPlayer (state.playerShip, maxVariation);
            //state.asteroidList.Add(asteroidObject);
			numAsteroids += 1;
		}
	}

    // Automatically destroy if 100 units behind the player
    IEnumerator Cleanup()
    {
        yield return new WaitForSeconds(1f);
        /*
        for (int i = state.asteroidList.Count - 1; i >= 0; i--)
        {
            //GameObject asteroid = state.asteroidList[i];
            AsteroidLogic asteroidLogic = asteroid.GetComponent<AsteroidLogic>();
            if (asteroid.transform.position.z < asteroidLogic.player.transform.position.z - 100f)
            {
                numAsteroids -= 1;
                Destroy(asteroid.gameObject);
                //state.asteroidList.RemoveAt(i);
            }
        }*/
        StartCoroutine("Cleanup");
    }
}
