using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class EngineerController : NetworkBehaviour {

    //Private vars
    private float speed;
    private float upMultiplier;
    private float jumpSpeed;
    private Quaternion currRotation;

	// Use this for initialization
    void Start()
    {
        //Initialize with default values
        if (isServer)
        {
            speed = 3;
            upMultiplier = 2;
            jumpSpeed = 2;
            currRotation = Quaternion.identity;
        }
    }

    [Command]
    public void CmdSetRotation(Quaternion rotation)
    {
        currRotation = rotation;
        gameObject.transform.rotation = currRotation;
    }

    [Command]
    public void CmdMove(Vector2 movement, bool jumping, bool sprinting)
    {
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * movement.y + transform.right * movement.x + transform.up * (movement.y * currRotation.x * upMultiplier);

        Vector3 actualMove;
        actualMove.x = desiredMove.x * speed;
        actualMove.z = desiredMove.z * speed;
        actualMove.y = desiredMove.y * speed;

        if (jumping)
        {
            actualMove.y += jumpSpeed;
        }

        gameObject.transform.Translate(actualMove);
        //m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
        //ProgressStepCycle(speed);
    }
}
