using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MessageHandler : MonoBehaviour {

    NetworkClient client = null;
    PlayerController controller = null;

	public void SetClient(NetworkClient newClient)
    {
        client = newClient;
    }

    public void OnServerOwner(NetworkMessage netMsg)
    {

        if (controller == null)
        {
            if (ClientScene.localPlayers[0].IsValid)
                controller = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();
            else
                return;
        }

        ControlledObjectMessage msg = netMsg.ReadMessage<ControlledObjectMessage>();
        GameObject obj = msg.controlledObject;

        if (obj != null)
        {
            controller.SetControlledObject(obj);
        }
    }
    
    public void OnServerJob(NetworkMessage netMsg)
    {
        if (controller == null)
        {
            if (ClientScene.localPlayers[0].IsValid)
                controller = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();
            else
                return;
        }

        // Parse the message as an EngineerJobMessage
        EngineerJobMessage msg = netMsg.ReadMessage<EngineerJobMessage>();

        // Notify engineer of the new job
        controller.AddJob(msg.upgrade, msg.part);
    }

    // Callback defined for when crosshairs are received
    public void OnCrosshairMessage(NetworkMessage netMsg)
    {
        // Get player owned by this client
        PlayerController controller = null;
        if (ClientScene.localPlayers[0].IsValid)
            controller = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();
        else
            return;

        // Set crosshairs owned by that player
        CrosshairMessage msg = netMsg.ReadMessage<CrosshairMessage>();
        controller.CallLocalSetCrosshair(msg.crosshairId, msg.screenId, msg.position);
    }
}

// Message for setting a crosshair position on a screen
public class CrosshairMessage : MessageBase
{
    public int crosshairId;
    public int screenId;
    public Vector3 position;
}
