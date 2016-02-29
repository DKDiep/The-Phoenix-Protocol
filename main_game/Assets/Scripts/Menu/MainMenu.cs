/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene, Dillon Diep
    Description: Basic menu system
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MainMenu : NetworkBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float rotationSpeed;

	public static bool startServer;
    
	private NetworkManager manager;
    private MessageHandler messageHandler;
    private Vector3 randomRotation;

    void Start()
    {
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

        manager            = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        messageHandler     = manager.GetComponent<MessageHandler>();
        randomRotation     = new Vector3(Random.value, Random.value, Random.value);
        transform.rotation = Random.rotation;
    }

	private void LoadSettings()
	{
		rotationSpeed = settings.MainMenuRotationSpeed;
	}

    void Update()
    {
        transform.Rotate(randomRotation * Time.deltaTime * rotationSpeed);
    }

    public void CreateGame()
    {
        startServer = true;
        SetHandler(manager.StartHost());
    }

    public void JoinGame()
    {
        startServer = false;
        SetHandler(manager.StartClient());
    }

    private void SetHandler(NetworkClient client)
    {
        messageHandler.SetClient(client);

        // Register handler for the Owner message from the server to the client
        client.RegisterHandler(890, messageHandler.OnServerOwner);

        // Register handler for EngineerJob messages from the server
        // These carry new jobs that the engineers need to do
        client.RegisterHandler(123, messageHandler.OnServerJob);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
