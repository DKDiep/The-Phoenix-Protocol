using UnityEngine;
using System.Collections;

public class DebugSpawner : MonoBehaviour 
{
	[SerializeField] GameObject gameManager;
    [SerializeField] GameObject myCounter;
	private GameState state;
	bool debug = false;
    
	void Start () 
    {
		if (gameManager != null)
		{
			state = gameManager.GetComponent<GameState>();
		}
	}

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
