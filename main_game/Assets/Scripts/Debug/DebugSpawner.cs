using UnityEngine;
using System.Collections;

public class DebugSpawner : MonoBehaviour {

	[SerializeField]
	GameObject gameManager;
	private GameState state;
	[SerializeField] GameObject myCounter;
	bool debug = false;

	// Use this for initialization
	void Start () {
		if (gameManager != null)
		{
			state = gameManager.GetComponent<GameState>();
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (state.GetStatus() == GameState.Status.Started)
		{
			if(!debug)
			{
				myCounter.SetActive (true);
				debug = true;
			}
		}
	}
}
