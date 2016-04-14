﻿/*
    Basic menu system
*/

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
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
        UpdateAddress();
        startServer = true;
        SetHandler(manager.StartHost());
    }

    public void JoinCamera()
    {
        UpdateAddress();
        startServer = false;
        SetHandler(manager.StartClient());
    }

    public void JoinEngineer()
    {
        UpdateAddress();
        startServer = false;
        SetHandler(manager.StartClient());
    }

    public void JoinCommander()
    {
        UpdateAddress();
        startServer = false;
        SetHandler(manager.StartClient());
    }

    private void SetHandler(NetworkClient client)
    {
        // Register handler for the Owner message from the server to the client
        client.RegisterHandler(MessageID.OWNER, messageHandler.OnServerOwner);

        // Register handler for EngineerJob messages from the server
        // These carry new jobs that the engineers need to do
        client.RegisterHandler(MessageID.ENGINEER_JOB, messageHandler.OnServerJob);

        // Handler for JobFinished messages from the server. These are used by the
        // CommandConsole.
        client.RegisterHandler(MessageID.JOB_FINISHED, messageHandler.OnJobFinished);

        // Handler for the OfficerList message from the server. This tells the command console
        // the list of officers for the current game
        client.RegisterHandler(MessageID.OFFICER_LIST, messageHandler.OnServerOfficerList);
    }

    public void UpdateAddress()
    {
        manager.networkAddress = GameObject.Find("NetworkAddressInput").GetComponent<InputField>().text;
        Debug.Log("Address now set to " + manager.networkAddress);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
