﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCPServer : MonoBehaviour
{    
    public enum MsgType
	{
        GameEnded
    }

	private GameSettings settings;

	// Configuration parameters loaded through GameSettings
    private int listenPort;
    private int maxReceivedMessagesPerInterval;
    
    // Constants for splitting the received messages
	private readonly String[] SEMICOLON = {";"};
	private readonly String[] COLON 	= {":"};
	private readonly String[] COMMA 	= {","};
	private readonly String[] PLUS 		= {"+"};

    private GameState gameState;
    private UDPServer udpServer;
    private ServerManager serverManager;
    private GameStatsManager statsManager;
    private TcpListener tcpServer = null;
    private Socket client 		  = null;
    private bool connected	      = false; 			// no easy way to tell from library
    private byte[] recvBuff       = new byte[1024]; // allocate 1KB receive buffer
    
    public void Initialise()
    {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
		LoadSettings();
        Debug.Log("Starting TCP server on port: " + listenPort);
        tcpServer = new TcpListener(IPAddress.Any, listenPort);
        gameState = this.gameObject.GetComponent<GameState>();
        udpServer = this.gameObject.GetComponent<UDPServer>();
        serverManager = this.gameObject.GetComponent<ServerManager>();
        statsManager = this.gameObject.GetComponent<GameStatsManager>();
        // Start listening for client requests
        tcpServer.Start();
        
        StartCoroutine(ConnectionHandler());
        StartCoroutine(SendUpdatedObjects());
    }

	private void LoadSettings()
	{
		listenPort 					   = settings.TCPListenPort;
		maxReceivedMessagesPerInterval = settings.TCPMaxReceivedMessagesPerInterval;
	}
    
    // Handles incomming messages from the phone server
    IEnumerator ConnectionHandler()
    {
        while (true)
        {
            // Probe for a new connection
            while (client == null || !connected)
            {
                if (tcpServer.Pending())
                {
                    client = tcpServer.AcceptSocket();
                    connected = true;
                    Debug.Log("TCP Client Connected! " + ((IPEndPoint)client.RemoteEndPoint).ToString());
                    
                    // Tell the UDP channel the address of the phone server
                    udpServer.SetClientAddress(((IPEndPoint)client.RemoteEndPoint).Address);
                }
                yield return new WaitForSeconds(0.2f);
            }
            
            // Receive data untill the connection is closed
            while (connected)
            {
                int numRead;
                String newData;
                String[] messages;
                int receivedMessages = 0;
                // read data if availale
                while (client.Available > 0 && receivedMessages < maxReceivedMessagesPerInterval)
                {
                    numRead = client.Receive(recvBuff, recvBuff.Length, 0);
                    newData = Encoding.ASCII.GetString(recvBuff, 0, numRead);
                    // It is possible to get multiple messages in a single receive
                    messages = newData.Split(SEMICOLON, StringSplitOptions.RemoveEmptyEntries);
                    // bla
                    foreach (String msg in messages)
                    {
                        try {
                            HandleMessage(msg);
                        } catch(Exception e) {
                        Debug.LogError(e);
                        }
                        receivedMessages++;
                    }
                }
                // check if the connection is closed
                if (((client.Available == 0) && client.Poll(1000, SelectMode.SelectRead)) || Input.GetKeyDown(KeyCode.P))
                {
                    Debug.Log("TCP Connection Closed! " + ((IPEndPoint)client.RemoteEndPoint).ToString());
                    connected = false;
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    
                    // Stops the transmission on the UDP channnel
                    udpServer.SetClientAddress(IPAddress.Any);
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
    
    /// <summary>
	/// Sends a signal to the phone server, notifying of a change
    /// in state. The valid signals are as follows:
    /// SETUP_STAGE: this is used to notify that a game has ended
    ///              and the system should enter the pre-game
    ///              state for the next game.
	/// </summary>
    /// <return>
    /// Indicates whether the signal was sent succesfully.
    /// </return>
    public bool SendSignal(MsgType type)
    {
        switch (type)
        {
            case MsgType.GameEnded:
                return SendMsg("{\"type\":\"GM_END\"}");
            default:
                return false;
        }
    }
    
    // Multiplexes the received message into unique actions
    private void HandleMessage(String msg)
    {
        String[] fields;
        String[] subFields;
        String[] parts = msg.Split(COLON, StringSplitOptions.RemoveEmptyEntries);
        switch(parts[0])
        {
            case "START":
                Dictionary<uint, Officer> officerMap = gameState.GetOfficerMap();
                Debug.Log("Received a Start Game signal with data:");

                fields = parts[1].Split(COMMA, StringSplitOptions.RemoveEmptyEntries);
                gameState.SetTeamName(fields[0]);
                statsManager.SetStatsIP(fields[1]);
                // Clear the officer dictionary to avoid having officers from last game in there
                officerMap.Clear();
                uint remoteId = 0;

                for (int i = 2; i < fields.Length; i++)
                {
                    string plr = fields[i];
                    subFields = plr.Split(PLUS, StringSplitOptions.RemoveEmptyEntries);
                    String userName = subFields[0];
                    uint userId = UInt32.Parse(subFields[1]);
                    officerMap.Add(remoteId, new Officer(userId, userName, remoteId));
                    Debug.Log("Username: " + userName + " id:" + userId);
                    remoteId++;
                }

                // Send the map of officers to clients that need it
                serverManager.SendOfficers();

                ReadyScreen readyScreen = GameObject.Find("ReadyCanvas(Clone)").GetComponent<ReadyScreen>();
                readyScreen.InitialiseGame();
                break;
            case "CTRL":
                fields = parts[1].Split(PLUS, StringSplitOptions.RemoveEmptyEntries);
                int idOfControlled = Int32.Parse(fields[0]);
                uint idOfControllingPlayer = UInt32.Parse(fields[1]);
                String nameOfControllingPlayer = fields[2];
                // Debug.Log("Received an Enemy Controll signal: id: " + idOfControlled);

                // Set enemy to controlled by spectator
                if (udpServer.InstanceIDToEnemy.ContainsKey(idOfControlled))
                    udpServer.InstanceIDToEnemy[idOfControlled].GetComponentInChildren<EnemyLogic>().SetHacked(true, idOfControllingPlayer, nameOfControllingPlayer);
                else
                    Debug.LogError("Tried to take control of invalid enemy");
                break;
            case "RESET":
                serverManager.Reset();
                break;
            default:
                Debug.Log("Received an unexpected message: " + msg);
                break;
        }
    }
    
    IEnumerator SendUpdatedObjects()
    {
        while (true)
        {
            if(connected  && gameState.Status == GameState.GameStatus.Started)
            {
                SendNotificationsUpdate();
            }
            yield return new WaitForSeconds(0.07f);
        }
    }
    
    // Send the changes in notifications
    private void SendNotificationsUpdate()
    {
        List<Notification> newNotif = gameState.GetNewNotifications();
        List<Notification> remNotif = gameState.GetRemovedNotifications();
        bool hasNew = newNotif != null && newNotif.Count > 0;
        bool hasRemoved = remNotif != null && remNotif.Count > 0;
        if (hasNew || hasRemoved)
        {
            try {
                string jsonMsg = "{\"type\":\"NOTIFY_UPD\",\"data\":[";
                if (hasNew)
                {
                    foreach (Notification notif in newNotif)
                    {
                        jsonMsg += "{\"type\":\"" + ComponentTypeToString(notif.Component) + "\",";
                        jsonMsg += "\"isUpgrade\":" + notif.IsUpgrade.ToString().ToLower() + ",";
                        jsonMsg += "\"toSet\":true},";
                    }
                }
                if (hasRemoved)
                {
                    foreach (Notification notif in remNotif)
                    {
                        jsonMsg += "{\"type\":\"" + ComponentTypeToString(notif.Component) + "\",";
                        jsonMsg += "\"isUpgrade\":" + notif.IsUpgrade.ToString().ToLower() + ",";
                        jsonMsg += "\"toSet\":false},";
                    }
                }
                jsonMsg = jsonMsg.Remove(jsonMsg.Length - 1);
                jsonMsg += "]};";
                bool success = SendMsg(jsonMsg);
                if (success) {
                    gameState.ClearNewNotifications();
                    gameState.ClearRemovedNotifications();
                }
            } catch(Exception e) {
                Debug.LogError(e);
            }
        }
    }
    
    // Send a score update for a player as a value to be added
    public bool SendSpectatorScoreIncrement(uint playerId, uint scoreIncrement) {
        try {
            string jsonMsg = "{\"type\":\"SCR_INC\",\"data\":{" +
                             "\"id\":" + playerId +
                             ",\"scr\":" + scoreIncrement +
                             "}};";
            return SendMsg(jsonMsg);
        } catch(Exception e) {
            Debug.LogError(e);
            return false;
        }
    }
    
    // Send a JSON encoded message to the phone server
    // return value indicates the success of the send
    private bool SendMsg(String jsonMsg)
    {
        if (client == null || !connected)
        {
            return false;
        }
        else
        {
            Byte[] data = Encoding.ASCII.GetBytes(jsonMsg);
            try
            {
                client.Send(data);
                // Debug.Log("Sent: " + jsonMsg);
                return true;
            }
            // Might not be the best way to deal with exceptions here
            // but it hasn't caused problems yet
            catch (SocketException)
            {
                connected = false;
                return false;
            }
        }
    }
    
    private String ComponentTypeToString(ComponentType comp)
    {
        switch(comp) {
            case ComponentType.ShieldGenerator:
                return "SHIELDS";
            case ComponentType.Turret:
                return "TURRETS";
            case ComponentType.Engine:
                return "ENGINES";
            case ComponentType.Hull:
                return "HULL";
        }
        
        return "";
    }
}
