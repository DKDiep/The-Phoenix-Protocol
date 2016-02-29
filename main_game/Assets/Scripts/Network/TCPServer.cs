using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCPServer : MonoBehaviour
{    
    public enum MsgType
	{
        SetupStage
    }

	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
    [SerializeField] private int listenPort;
    [SerializeField] private int maxReceivedMessagesPerInterval;
	#pragma warning restore 0649

    private UDPServer udpServer;
    private TcpListener tcpServer = null;
    private Socket client = null;
    private bool connected = false; // no easy way to tell from library
    private byte[] recvBuff = new byte[1024]; // allocate 1KB receive buffer
    
	// Use this for initialization
	void Start ()
    {
        if (MainMenu.startServer)
        {
            Debug.Log("Starting TCP server on port: " + listenPort);
            tcpServer = new TcpListener(IPAddress.Any, listenPort);
            udpServer = this.gameObject.GetComponent<UDPServer>();

            // Start listening for client requests
            tcpServer.Start();
            
            StartCoroutine("ConnectionHandler");
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        
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
                int receivedMessages = 0;
                // read data if availale
                while (client.Available > 0 && receivedMessages < maxReceivedMessagesPerInterval)
                {
                    numRead = client.Receive(recvBuff, recvBuff.Length, 0);
                    // TODO: deal with received data
                    Debug.Log("TCP Received Data: " + Encoding.ASCII.GetString(recvBuff, 0, numRead));
                    receivedMessages++;
                }
                // check if the connection is closed
                if ((client.Available == 0) && client.Poll(1000, SelectMode.SelectRead))
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
    public bool SendSignal(MsgType type) {
        switch (type)
        {
            case MsgType.SetupStage:
                return SendMsg("{\"type\":\"GM_STP\"}");
            // TODO: Implement more message types.
            default:
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
            try {
                client.Send(data);
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
}
