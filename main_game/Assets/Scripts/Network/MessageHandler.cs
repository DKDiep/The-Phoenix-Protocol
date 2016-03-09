using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MessageHandler : MonoBehaviour {
    private PlayerController controller = null;

    /// <summary>
    /// Sets the PlayerController of the current client
    /// </summary>
    private bool SetController()
    {
        if (ClientScene.localPlayers[0].IsValid)
        {
            controller = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerController>();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Client side handler for the OWNER message.
    /// This message contains the game object that the client owns
    /// </summary>
    /// <param name="netMsg">The message from the server</param>
    public void OnServerOwner(NetworkMessage netMsg)
    {
        // This works because of short circuiting
        if (controller == null && !SetController())
            return;

        ControlledObjectMessage msg = netMsg.ReadMessage<ControlledObjectMessage>();
        GameObject obj = msg.controlledObject;

        if (obj != null)
            controller.SetControlledObject(obj);
    }

    /// <summary>
    /// Client side handler for the ENGINEER_JOB message.
    /// This message contains the type of job (upgrade or repair)
    /// and the component it needs to be carried out on
    /// </summary>
    /// <param name="netMsg">The message from the server</param>
    public void OnServerJob(NetworkMessage netMsg)
    {
        // This works because of short circuiting
        if (controller == null && !SetController())
            return;

        // Parse the message as an EngineerJobMessage
        EngineerJobMessage msg = netMsg.ReadMessage<EngineerJobMessage>();

        // Notify engineer of the new job
        controller.AddJob(msg.upgrade, msg.part);
    }

    /// <summary>
    /// Client side handler for the COMPONENT_STATUS message.
    /// This message contains the component's health and upgrade level
    /// </summary>
    /// <param name="netMsg"></param>
    public void OnServerComponentStatus(NetworkMessage netMsg)
    {
        // This works because of short circuiting
        if (controller == null && !SetController())
            return;

        ComponentStatusMessage msg = netMsg.ReadMessage<ComponentStatusMessage>();
        controller.UpdateComponentStatus(msg.health, msg.level);
    }

    /// <summary>
    /// Client side handler for the JOB_FINISHED message.
    /// This message is the same as an EngineerJobMessage but is used
    /// in a different way. The isUpgrade boolean is ignored and the part
    /// represents the part that has just been upgraded
    /// </summary>
    /// <param name="netMsg"></param>
    public void OnJobFinished(NetworkMessage netMsg)
    {
        // This works because of short circuiting
        if (controller == null && !SetController())
            return;

        EngineerJobMessage msg = netMsg.ReadMessage<EngineerJobMessage>();
        controller.FinishUpgrade(msg.part);
    }
}
