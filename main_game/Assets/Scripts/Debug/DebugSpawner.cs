using UnityEngine;
using System.Collections;

public class DebugSpawner : MonoBehaviour 
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] GameObject gameManager;
    [SerializeField] GameObject myCounter;
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields

	private GameState state;
	private bool debug = false;
    
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
