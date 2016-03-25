using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

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
    private UdpClient socket;
    private IPEndPoint clientEndPoint;
    private byte[] receive_byte_array;

    void Start()
    {
		settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        playerShooting = GameObject.Find("PlayerShootLogic(Clone)").GetComponent<PlayerShooting>();
		LoadSettings();

        if (MainMenu.startServer)
        {
            Debug.Log("Starting UDP server on port: " + listenPort);

            socket = new UdpClient(listenPort);
            clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            state = this.gameObject.GetComponent<GameState>();

            StartCoroutine("ConnectionHandler");
            StartCoroutine("SendUpdatedOjects");
        }
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
                HandleMessage(received_data);
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
                // TODO: move enemy
                fields = parts[1].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                int idToMove = Int32.Parse(fields[0]);
                float posX = float.Parse(fields[1]);
                float posZ = float.Parse(fields[2]);
                Debug.Log("Received a Move Command: id: " + idToMove + " x: " + posX + " z: " + posZ);
                break;
            case "ATT":
                // TODO: attack specified enemy
                fields = parts[1].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                int idOfAttacker = Int32.Parse(fields[0]);
                int idOfAttacked = Int32.Parse(fields[1]);
                Debug.Log("Received an Attack Command: attacker: " + idOfAttacker + " attacked: " + idOfAttacked);
                break;
            case "CH": // Wii remote x,y data
                fields = parts[1].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                foreach (String plr in fields)
                {
                    subFields = plr.Split(plus, StringSplitOptions.RemoveEmptyEntries);
                    uint controllerId = UInt32.Parse(subFields[0]);
                    uint screenId = UInt32.Parse(subFields[1]);
                    float x = float.Parse(subFields[2]);
                    float y = float.Parse(subFields[3]);
                    Debug.Log("Controller: " + controllerId + " screenId: " + screenId + " x: " + x + " y: " + y);
                    GameObject gameManager = GameObject.Find("GameManager");
                    ServerManager serverManager = gameManager.GetComponent<ServerManager>();
                    serverManager.SetCrosshairPosition((int)controllerId, 0, new Vector2(x, y));
                }
                break;
            case "BP": // Wii remot button shoot press
                fields = parts[1].Split(comma, StringSplitOptions.RemoveEmptyEntries);
                int idOfPlayer = Int32.Parse(fields[0]);
                Debug.Log(idOfPlayer);
                playerShooting.ShootBullet(idOfPlayer);
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
                SendShipPosition();
                SendEnemies();
                SendRemovedEnemies();
                SendNewAsteroids();
                SendRemovedAsteroids();
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
                             "\"x\":"  + playerShip.transform.position.x +
                             ",\"y\":" + playerShip.transform.position.z +
                             ",\"rot\":" + playerShip.transform.eulerAngles.y +
                             "}}";
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
                            ",\"x\":" + ast.transform.position.x +
                            ",\"y\":" + ast.transform.position.z +
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
            string jsonMsg = "{\"type\":\"ENM_UPD\",\"data\":[";
            foreach (GameObject enemy in enemies)
            {
                jsonMsg += "{\"id\":" + enemy.GetInstanceID() +
                            ",\"x\":" + enemy.transform.position.x +
                            ",\"y\":" + enemy.transform.position.z +
                            "},";
            }
            jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
            jsonMsg += "]}";
            SendMsg(jsonMsg);
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