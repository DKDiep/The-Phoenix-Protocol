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

	// Use this for initialization
	void Start () 
    {
        state = transform.parent.gameObject.GetComponent<GameState>();
	}
	
	// Update is called once per frame
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
