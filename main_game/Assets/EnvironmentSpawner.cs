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
