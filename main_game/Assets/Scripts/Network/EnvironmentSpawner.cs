/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Spawn environment object
*/

using UnityEngine;
using System.Collections;

public class EnvironmentSpawner : MonoBehaviour 
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private GameObject spaceScene;
	#pragma warning restore 0649 

	private GameState state;

	void Start () 
    {
        state = transform.parent.gameObject.GetComponent<GameState>();
	}

	void Update () 
    {
        if (state.Status == GameState.GameStatus.Started)
        {
            GameObject space = Instantiate(Resources.Load("Prefabs/SpaceScene 1", typeof(GameObject))) as GameObject;

            // Add Earth Collision script
            GameObject.Find("Ground").AddComponent<EarthCollision>();
            ServerManager.NetworkSpawn(space);
            Destroy(this);
        }
	}
}
