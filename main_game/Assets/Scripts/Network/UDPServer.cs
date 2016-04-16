using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Networking;

public class UDPServer : MonoBehaviour
{
	private GameSettings settings;

    // Constants for splitting the received messages
    private readonly String[] colon = {":"};
    private readonly String[] comma = {","};
    private readonly String[] plus = {"+"};

	// Configuration parameters loaded through GameSettings
    private int listenPort;
    private int clientPort;
    private int maxReceivedMessagesPerInterval;

    private GameState state;
    private PlayerShooting playerShooting;
    private ServerManager serverManager;
    public Dictionary<int, GameObject> InstanceIDToEnemy { get; private set; }

    private UdpClient socket;
    private IPEndPoint clientEndPoint;
    private byte[] receive_byte_array;

    void Start()
    {
		
    }
    
    public void Initialise()
    {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();
        Debug.Log("Starting UDP server on port: " + listenPort);

        GameObject server = GameObject.Find("GameManager");
        serverManager = server.GetComponent<ServerManager>();

        InstanceIDToEnemy = new Dictionary<int, GameObject>();
        socket = new UdpClient(listenPort);
        clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        state = this.gameObject.GetComponent<GameState>();

        StartCoroutine(ConnectionHandler());
        StartCoroutine(SendUpdatedOjects());
    }

	private void LoadSettings()
	{
		listenPort 					   = settings.UDPListenPort;
		clientPort 					   = settings.UDPClientPort;
		maxReceivedMessagesPerInterval = settings.UDPMaxReceivedMessagesPerInterval;
	}

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Sets the address to which the udp server sends data
    public void SetClientAddress(IPAddress addr) {
        clientEndPoint = new IPEndPoint(addr, clientPort);
    }

    // Handles incomming messages from the phone server
    IEnumerator ConnectionHandler()
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            int receivedMessages = 0;
            while (socket.Available > 0 && receivedMessages < maxReceivedMessagesPerInterval)
            {
                receive_byte_array = socket.Receive(ref sender);
                
                string received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
                try
                { 
                    HandleMessage(received_data);
                } catch(Exception e) {
                    Debug.LogError(e);
                }
                receivedMessages++;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    // Multiplexes the received message into unique actions
    private void HandleMessage(String msg)
    {
        String[] fields;
        String[] subFields;
        String[] parts = msg.Split(colon, StringSplitOptions.RemoveEmptyEntries);
        switch(parts[0]) {
            case "MV":
                fields = parts[1].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                int idToMove = Int32.Parse(fields[0]);
                float posX = float.Parse(fields[1]);
                float posZ = float.Parse(fields[2]);
                Debug.Log("Received a Move Command: id: " + idToMove + " x: " + posX + " z: " + posZ);

                // Move specified enemy
                if (InstanceIDToEnemy.ContainsKey(idToMove))
                    InstanceIDToEnemy[idToMove].GetComponentInChildren<EnemyLogic>().HackedMove(posX, posZ);
                else
                    Debug.LogError("Tried to move non existant enemy from phone server");

                break;
            case "ATT":
                fields = parts[1].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                int idOfAttacker = Int32.Parse(fields[0]);
                int idOfAttacked = Int32.Parse(fields[1]);
                Debug.Log("Received an Attack Command: attacker: " + idOfAttacker + " attacked: " + idOfAttacked);

                // Give the order to attack
                if (InstanceIDToEnemy.ContainsKey(idOfAttacker) && InstanceIDToEnemy.ContainsKey(idOfAttacked))
                    InstanceIDToEnemy[idOfAttacker].GetComponentInChildren<EnemyLogic>().HackedAttack(InstanceIDToEnemy[idOfAttacked]);
                else
                    Debug.LogError("Tried to attack with non existent ID, or target ID is non existant");

                break;
            case "CH": // Wii remote x,y data
                fields = parts[1].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (String plr in fields)
                {
                    subFields = plr.Split(plus, StringSplitOptions.RemoveEmptyEntries);
                    uint controllerId = UInt32.Parse(subFields[0]);
                    int screenId = Int32.Parse(subFields[1]);
                    float x = float.Parse(subFields[2]) * Screen.width;
                    float y = float.Parse(subFields[3]) * Screen.height;
                    serverManager.GetCrosshairObject(screenId).GetComponent<CrosshairMovement>().SetCrosshairPositionWiiRemote((int)controllerId, screenId, new Vector2(x, y));
                }
                break;
            case "BP": // Wii remote button shoot press 
                fields = parts[1].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                int idOfPlayer = Int32.Parse(fields[0]);
                playerShooting = GameObject.Find("PlayerShootLogic(Clone)").GetComponent<PlayerShooting>();
				playerShooting.TryShoot(idOfPlayer, true);
                break;
            default:
                Debug.Log("Received an unexpected message: " + msg);
                break;
        }
    }

    IEnumerator SendUpdatedOjects()
    {
        while (true)
        {
            if(!clientEndPoint.Address.Equals(IPAddress.Any))
            {
                try
                {
                    SendShipPosition();
                    SendOfficerAmmo();
                    SendOfficerScore();
                    SendEnemies();
                    SendRemovedEnemies();
                    SendNewAsteroids();
                    SendRemovedAsteroids();
                } catch(Exception e) {
                    Debug.LogError(e);
                }
            }
            yield return new WaitForSeconds(0.07f);
        }
    }

    private void SendShipPosition()
    {
        GameObject playerShip = state.PlayerShip;
        if (playerShip != null)
        {
            string jsonMsg = "{\"type\":\"SHP_UPD\",\"data\":{" +
                             "\"x\":"  + playerShip.transform.position.x.ToString("0.000") +
                             ",\"y\":" + playerShip.transform.position.z.ToString("0.000") +
                             ",\"z\":" + playerShip.transform.position.y.ToString("0.000") +
                             ",\"fX\":" + playerShip.transform.forward.x.ToString("0.000") +
                             ",\"fY\":" + playerShip.transform.forward.z.ToString("0.000") +
                             ",\"fZ\":" + playerShip.transform.forward.y.ToString("0.000") +
                             ",\"rX\":" + playerShip.transform.right.x.ToString("0.000") +
                             ",\"rY\":" + playerShip.transform.right.z.ToString("0.000") +
                             ",\"rZ\":" + playerShip.transform.right.y.ToString("0.000") +
                             ",\"uX\":" + playerShip.transform.up.x.ToString("0.000") +
                             ",\"uY\":" + playerShip.transform.up.z.ToString("0.000") +
                             ",\"uZ\":" + playerShip.transform.up.y.ToString("0.000") +
                             "}}";
            SendMsg(jsonMsg);
        }
    }
    
    private void SendOfficerAmmo()
    {
        Dictionary<uint, Officer> officers = state.GetOfficerMap();
        if (officers != null && officers.Count > 0)
        {
            string jsonMsg = "{\"type\":\"AMMO_UPD\",\"data\":[";
            foreach(KeyValuePair<uint, Officer> entry in officers) {
                jsonMsg += "{\"id\":" + entry.Key + ",";
                jsonMsg += "\"ammo\":" + entry.Value.Ammo + "},";
            }
            jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
            jsonMsg += "]}";
            SendMsg(jsonMsg);
        }
    }

    private void SendOfficerScore()
    {
        Dictionary<uint, Officer> officers = state.GetOfficerMap();
        if (officers != null && officers.Count > 0)
        {
            int i = 0;
            string jsonMsg = "{\"type\":\"SCORE_UPD\",\"data\":[";
            foreach(KeyValuePair<uint, Officer> entry in officers) {
                jsonMsg += "{\"id\":" + entry.Key + ",";
                jsonMsg += "\"score\":" + state.GetPlayerScore(i) + "},";
                i++;
            }
            jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
            jsonMsg += "]}";
            SendMsg(jsonMsg);
        }
    }

    private void SendNewAsteroids()
    {
        List<GameObject> newAsteroids = state.GetNewAsteroids();
        if(newAsteroids != null && newAsteroids.Count > 0)
        {
            string jsonMsg = "{\"type\":\"NEW_AST\",\"data\":[";
            foreach (GameObject ast in newAsteroids)
            {
                jsonMsg += "{\"id\":" + (uint)ast.GetInstanceID() +
                            ",\"x\":" + ast.transform.position.x.ToString("0.000") +
                            ",\"y\":" + ast.transform.position.z.ToString("0.000") +
                            ",\"z\":" + ast.transform.position.y.ToString("0.000") +
                            "},";
            }
            jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
            jsonMsg += "]}";
            SendMsg(jsonMsg);
            state.ClearNewAsteroids();
        }
    }

    private void SendRemovedAsteroids()
    {
        List<uint> removedAsteroids = state.GetRemovedAsteroids();
        if (removedAsteroids != null && removedAsteroids.Count > 0)
        {
            string jsonMsg = "{\"type\":\"RMV_AST\",\"data\":[";
            foreach (uint id in removedAsteroids)
            {
                jsonMsg += id + ",";
            }
            jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
            jsonMsg += "]}";
            SendMsg(jsonMsg);
            state.ClearRemovedAsteroids();
        }
    }
    
    private void SendEnemies()
    {
        List<GameObject> enemies = state.GetEnemyList();
        if(enemies != null && enemies.Count > 0)
        {
			bool foundNullEnemy = false;

            string jsonMsg = "{\"type\":\"ENM_UPD\",\"data\":[";
            foreach (GameObject enemy in enemies)
            {
				if (enemy == null)
				{
					Debug.LogWarning("There is a null enemy in the enemy list.");
					foundNullEnemy = true;
					continue;
				}

				// Add the enemy to the dictionary if it isn't already in there
                if (!InstanceIDToEnemy.ContainsKey((enemy.GetInstanceID())))
                    InstanceIDToEnemy[enemy.GetInstanceID()] = enemy;

                jsonMsg += "{\"id\":" + enemy.GetInstanceID() +
                            ",\"x\":" + enemy.transform.position.x.ToString("0.000") +
                            ",\"y\":" + enemy.transform.position.z.ToString("0.000") +
                            ",\"z\":" + enemy.transform.position.y.ToString("0.000") +
                            ",\"fX\":" + enemy.transform.forward.x.ToString("0.000") +
                            ",\"fY\":" + enemy.transform.forward.z.ToString("0.000") +
                            ",\"fZ\":" + enemy.transform.forward.y.ToString("0.000") +
                            "},";
            }
            jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
            jsonMsg += "]}";
            SendMsg(jsonMsg);

			// Send the notification at the end of the loop to avoid iterator issues
			if (foundNullEnemy)
				state.NotifyNullEnemy();
        }
    }
    
    private void SendRemovedEnemies()
    {
        List<int> removedEnemies = state.GetRemovedEnemies();
        if (removedEnemies != null && removedEnemies.Count > 0)
        {
            string jsonMsg = "{\"type\":\"RMV_ENM\",\"data\":[";
            foreach (int id in removedEnemies)
            {
                // Remove the instance ID from the dictionary if it is in there
                if (InstanceIDToEnemy.ContainsKey(id))
                    InstanceIDToEnemy.Remove(id);

                jsonMsg += id + ",";
            }
            jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
            jsonMsg += "]}";
            SendMsg(jsonMsg);
            state.ClearRemovedEnemies();
        }
    }

    // Send a JSON encoded message to the phone server
    private void SendMsg(String jsonMsg)
    {
        Byte[] data = Encoding.ASCII.GetBytes(jsonMsg);
        socket.Send(data, data.Length, clientEndPoint);
    }
}
