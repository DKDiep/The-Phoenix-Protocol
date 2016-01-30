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

  [SerializeField] GameObject spaceScene;
  GameState state;

	void Start () 
    {
        state = transform.parent.gameObject.GetComponent<GameState>();
	}

	void Update () 
    {
        if (state.GetStatus() == GameState.Status.Started)
        {
          Instantiate(spaceScene, Vector3.zero, Quaternion.identity);
          ServerManager.NetworkSpawn(spaceScene);
          Destroy(this);
        }
	}
}
