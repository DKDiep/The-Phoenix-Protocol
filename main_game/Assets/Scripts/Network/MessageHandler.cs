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
}
