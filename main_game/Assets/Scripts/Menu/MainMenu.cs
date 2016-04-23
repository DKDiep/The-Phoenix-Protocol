/*
    Basic menu system
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;
using System.IO;

public class MainMenu : NetworkBehaviour 
{
	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
	private float rotationSpeed;
	private string configFile;

	public static bool startServer;
    public static RoleEnum role;

    private NetworkManager manager;
    private MessageHandler messageHandler;
    private Vector3 randomRotation;
    private PlayerController playerController;
	private InputField networkAddressInputField;

    void Start()
    {
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();

        manager            = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        messageHandler     = manager.GetComponent<MessageHandler>();
        randomRotation     = new Vector3(Random.value, Random.value, Random.value);
        transform.rotation = Random.rotation;

		networkAddressInputField = GameObject.Find("NetworkAddressInput").GetComponent<InputField>();

		LoadConfigFile();
    }

	private void LoadSettings()
	{
		rotationSpeed = settings.MainMenuRotationSpeed;
		configFile 	  = settings.ConfigFile;
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
        role = RoleEnum.Camera;
}

    public void JoinCamera()
    {
        UpdateAddress();
        startServer = false;
        SetHandler(manager.StartClient());
        role = RoleEnum.Camera;
    }

    public void JoinEngineer()
    {
        UpdateAddress();
        startServer = false;
        SetHandler(manager.StartClient());
        role = RoleEnum.Engineer;
    }

    public void JoinCommander()
    {
        UpdateAddress();
        startServer = false;
        SetHandler(manager.StartClient());
        role = RoleEnum.Commander;
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

    /*private void SetHandler(NetworkServer server)
    {
        // Register handler for the Owner message from the server to the client
        server.RegisterHandler(MessageID.ROLE, messageHandler.OnServerRole);
    }*/

    public void UpdateAddress()
    {
		string ipAddress 	   = networkAddressInputField.text;
		manager.networkAddress = ipAddress;
		Debug.Log("Address now set to " + ipAddress);

		SaveConfigToFile(ipAddress);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

	/// <summary>
	/// Saves the server and role configuration to a file.
	/// </summary>
	/// <param name="ipAddress">The server IP address.</param>
	private void SaveConfigToFile(string ipAddress)
	{
		using (StreamWriter w = new StreamWriter(@configFile))
		{
			// Write a short help
			w.WriteLine("# Lines starting with a # are ignored");
			w.WriteLine("# The first line without a # is the server IP address, the second one is the role (camera, engineer, commander)");
			w.WriteLine("# You can edit this file manually, but behaviour is undefined if the file structure is not respected.");

			// Save the server data
			w.WriteLine(ipAddress);
		}
	}

	private void LoadConfigFile()
	{
		string[] lines;

		try
		{
			lines = File.ReadAllLines(@configFile);
		}
		catch(FileNotFoundException)
		{
			Debug.Log("No config file found");
			return;
		}

		// Skip comment lines at the start of the file
		int i = 0;
		while (lines[i].StartsWith("#") && i < lines.Length)
			i++;

		if (i >= lines.Length)
		{
			Debug.Log("Config file is empty.");
			return;
		}
			
		// Get the IP address
		networkAddressInputField.text = lines[i];

		Debug.Log("Loaded IP address.");
	}
}
