using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientMenu : MonoBehaviour {

[SerializeField] Texture2D waiting;
[SerializeField] GameObject menuCam;
[SerializeField] GameObject debug;
GameState gameState;
NetworkManager manager;

	// Use this for initialization
	void Start () 
	{
		StartCoroutine ("Connect");
		manager = GetComponent<NetworkManager>();
		gameState = GetComponent<GameState>();
	}
	
	// Sketchy method to detect when client game has started. Needs replacing.
	void Update () 
	{
		GameObject temp = GameObject.Find ("PlayerShip(Clone)");
		
		if(temp != null)
		{
			Destroy (menuCam);
			debug.SetActive (true);
			Destroy (this);
		}
	}
	
	void OnGUI()
	{
		GUI.DrawTexture(new Rect((Screen.width / 2) - 442, (Screen.height / 2) - 50, 882, 100), waiting, ScaleMode.ScaleToFit, true, 0f);
	}
	
	IEnumerator Connect()
	{
		yield return new WaitForSeconds(1f);
		manager.StartClient ();
	}
}
