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

  GameState state;

	void Start () 
    {
        state = transform.parent.gameObject.GetComponent<GameState>();
	}

	void Update () 
    {
        if (state.GetStatus() == GameState.Status.Started)
        {
          Debug.Log("Attempting to spawn space scene");
          GameObject space = Instantiate(Resources.Load("Prefabs/SpaceScene 1", typeof(GameObject))) as GameObject;
          ServerManager.NetworkSpawn(space);
          Destroy(this);
        }
	}
}
