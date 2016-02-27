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

    public static bool startServer;
    NetworkManager manager;
    MessageHandler messageHandler;
    Vector3 randomRotation;
    [SerializeField] float rotationSpeed;

    void Start()
    {
        manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        messageHandler = manager.GetComponent<MessageHandler>();
        randomRotation = new Vector3(Random.value, Random.value, Random.value);
        transform.rotation = Random.rotation;
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

        client.RegisterHandler(891, messageHandler.OnCrosshairMessage);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
