using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MessageHandler : MonoBehaviour {

    NetworkClient client = null;

	public void SetClient(NetworkClient newClient)
    {
        client = newClient;
    }

    public void OnClientConnect(NetworkMessage netMsg)
    {
        // Tell the server we want to be an engineer
        PickRoleMessage message = new PickRoleMessage();
        message.role = (int)RoleEnum.ENGINEER;
        client.Send(789, message);
    }

    public void OnServerOwner(NetworkMessage netMsg)
    {
        PlayerController controller = null;

        if (ClientScene.localPlayers[0].IsValid)
            controller = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();
        else
            return;

        ControlledObjectMessage msg = netMsg.ReadMessage<ControlledObjectMessage>();
        GameObject obj = msg.controlledObject;

        if (obj != null)
        {
            controller.SetControlledObject(obj);
        }
    }
}
