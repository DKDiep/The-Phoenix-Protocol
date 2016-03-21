/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Causes client-side rotation of asteroid based on speed sent from server
*/

using UnityEngine;
using System.Collections;

public class AsteroidRotation : MonoBehaviour
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private int maxRenderDistance;

	private float speed, distance;
	private GameObject player;

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields	
	[SerializeField] private Mesh highPoly;
	[SerializeField] private Mesh medPoly;
	[SerializeField] private Mesh lowPoly;
	#pragma warning restore 0649

	private MeshFilter myFilter;
    private ObjectPoolManager poolManager;
	private new Renderer renderer;

  // Only one packet needs to be sent to the client to control the asteroid's rotation
	void Start ()
	{
		player   = GameObject.Find("PlayerShip(Clone)");
        myFilter = GetComponent<MeshFilter>();
		renderer = GetComponent<Renderer>();

		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

		StartCoroutine("AsteroidLOD");
	}

	private void LoadSettings()
	{
		maxRenderDistance = settings.AsteroidMaxRenderDistance;
	}

    public void SetSpeed(float tempSpeed)
    {
        if(poolManager == null)
            poolManager = GameObject.Find("AsteroidManager").GetComponent<ObjectPoolManager>();
        poolManager.SetAsteroidSpeed(gameObject.name, tempSpeed);
        speed = tempSpeed;
    }

    public void SetClientSpeed(float tempSpeed)
    {
        speed = tempSpeed;
    }
	
	void Update()
	{
        if(distance < 800)
		    transform.Rotate(transform.forward * speed * Time.deltaTime);

		renderer.enabled = distance < maxRenderDistance;
	}

	IEnumerator AsteroidLOD()
	{
		distance = Vector3.Distance(transform.position, player.transform.position);
		if(distance < 250)
		    myFilter.mesh = highPoly;
		else if(distance < 500)
		    myFilter.mesh = medPoly;
		else
		    myFilter.mesh = lowPoly;

		yield return new WaitForSeconds(1f);
		StartCoroutine("AsteroidLOD");
	}
}
