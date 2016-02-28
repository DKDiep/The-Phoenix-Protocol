using UnityEngine;
using System.Collections;

/// <summary>
/// Class that contains all the game settings. These are all set through the Unity IDE and should only be retrieved from here.
/// </summary>
public class GameSettings : MonoBehaviour
{
	[Header("Asteroid Logic")]
	public int AsteroidMaxDroppedResources; // The maximum number of resources that can be dropped by an asteroid. 
	public float AsteroidMinSpeed;
	public float AsteroidMaxSpeed;

	[Header("Asteroid Spawner")]
	public GameObject AsteroidModel1;   // 3 asteroid objects for the 3 different models
	public GameObject AsteroidModel2;
	public GameObject AsteroidModel3;
	public GameObject GameManager;
	public int MaxAsteroids;            		// Maximum number of asteroids that can exist simultaneously
	public float AsteroidMaxVariation;  		// Max variation in size (0-10)
	public float AsteroidMinDistance;   		// Minimum distance to the player that an asteroid can spawn
	public float AsteroidMaxDistance;   		// Maximum distance to the player that an asteroid can spawn
	public int MaxAsteroidsSpawnedPerFrame;
	public float AsteroidAvgSize;               // The average asteroid size. Please update this manually if you change the sizes to avoid useless computation
	public float AsteroidFieldSpacingFactor;    // Higher values make asteroid fields more sparse. TODO: 2f looks good, but is quite expensive
}
