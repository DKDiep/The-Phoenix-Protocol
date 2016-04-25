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

	private bool configDisabled = false;
	private const string TEXT_CONFIG_DISABLED = "Disable";

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
        startServer = true;
        SetHandler(manager.StartHost());
        role = RoleEnum.Camera;
		UpdateAddress();
	}

    public void JoinCamera()
    {
        startServer = false;
        SetHandler(manager.StartClient());
        role = RoleEnum.Camera;
		UpdateAddress();
    }

    public void JoinEngineer()
    {
        startServer = false;
        SetHandler(manager.StartClient());
        role = RoleEnum.Engineer;
		UpdateAddress();
    }

    public void JoinCommander()
    {
        startServer = false;
        SetHandler(manager.StartClient());
        role = RoleEnum.Commander;
		UpdateAddress();
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

		SaveConfigToFile(ipAddress, role, startServer);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

	/// <summary>
	/// Saves the server and role configuration to a file.
	/// </summary>
	/// <param name="ipAddress">The server IP address.</param>
	private void SaveConfigToFile(string ipAddress, RoleEnum role, bool isServer)
	{
		using (StreamWriter w = new StreamWriter(@configFile))
		{
			// Write a short help
			w.WriteLine("# Lines starting with a # are ignored, and if the first line is \"" + TEXT_CONFIG_DISABLED +
				"\", then the config is ignored");
			w.WriteLine("# The first line without a # is the server IP address, " +
				"the second one is the role (camera, engineer, commander), " +
				"the third one is \"True\" for the server");
			w.WriteLine("# You can edit this file manually, but behaviour is undefined if the file structure is not respected.");

			// Save the disabled state
			if (configDisabled)
				w.WriteLine(TEXT_CONFIG_DISABLED);

			// Save the server data and the client role
			w.WriteLine(ipAddress);
			w.WriteLine(role);
			w.WriteLine(isServer);
		}
	}

	private void LoadConfigFile()
	{
		string[] lines;

		// Get all the lines in the file
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
			Debug.Log("Config file is empty");
			return;
		}

		// Check the disable flag
		if (lines[i].StartsWith(TEXT_CONFIG_DISABLED, System.StringComparison.CurrentCultureIgnoreCase))
		{
			configDisabled = true;
			Debug.Log("Config loading disabled");

			i++;

			// Skip comment lines
			while (lines[i].StartsWith("#") && i < lines.Length)
				i++;

			if (i >= lines.Length)
			{
				Debug.Log("Config file is empty");
				return;
			}

			// Get the IP address
			networkAddressInputField.text = lines[i];
		}
		else
			networkAddressInputField.text = lines[i];

		i++;

		// If the disable flag is set, only load the IP address
		if (configDisabled)
			return;

		// Skip comment lines
		while (lines[i].StartsWith("#") && i < lines.Length)
			i++;

		// Get the role
		try
		{
			role = (RoleEnum)System.Enum.Parse(typeof(RoleEnum), lines[i]);
			i++;
		}
		catch(System.Exception)
		{
			Debug.Log("No valid role found in config");
			return;
		}

		// Skip comment lines
		while (lines[i].StartsWith("#") && i < lines.Length)
			i++;

		// Read if this client is a server
		try
		{
			startServer = System.Boolean.Parse(lines[i]);
		}
		catch(System.Exception)
		{
			startServer = false;
		}

		// Log the loaded parameters
		string debugPrompt = "Loaded config: " + networkAddressInputField.text + " as " + role;
		if (startServer)
			debugPrompt += " (server)";
		Debug.Log(debugPrompt);

		// If this is not a server, try to connect to the last server
		if (!startServer)
			manager.StartClient();
	}
}
