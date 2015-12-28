using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class EngineerController : NetworkBehaviour {

    //Private vars
    [SerializeField] private float upMultiplier;
    [SerializeField] private bool isWalking;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float m_StepInterval;
    [SerializeField] private Text upgradeText;

    private Camera camera;
    private MouseLook mouseLook;
    private bool jump;
    private Vector2 input;
    private CollisionFlags m_CollisionFlags;
    private float m_StepCycle;
    private float m_NextStep;

	// Use this for initialization
    void Start()
    {
        //Initialize with default values
        if (isServer)
        {
            gameObject.transform.rotation = Quaternion.identity;
        }
    }

    [Command]
    void CmdSetRotation(Quaternion rotation)
    {
        Debug.Log("Setting rotation");
        gameObject.transform.rotation = rotation;
    }

    [Command]
    void CmdMove(Vector2 movement, bool jumping, bool sprinting)
    {
        Debug.Log("Moving");
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * movement.y + transform.right * movement.x +
            transform.up * (movement.y * gameObject.transform.rotation.x * upMultiplier);

        float speed = sprinting ? runSpeed : walkSpeed;
        Vector3 actualMove;
        actualMove.x = desiredMove.x * speed;
        actualMove.z = desiredMove.z * speed;
        actualMove.y = desiredMove.y * speed;

        if (jumping)
        {
            actualMove.y += jumpSpeed;
        }

        gameObject.transform.Translate(actualMove);
    }

    //Initialize player instance and local game here. Kind of a replacement for Start()
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        //Disable all cameras first
        foreach (Camera cam in Camera.allCameras)
        {
            cam.enabled = false;
        }

        //Disable all AudioListeners
        foreach (AudioListener l in Resources.FindObjectsOfTypeAll<AudioListener>())
        {
            l.enabled = false;
        }

        GameObject cameraObj = GameObject.Instantiate(Resources.Load("Prefabs/EngineerCamera", typeof(GameObject))) as GameObject; // add camera
        cameraObj.transform.localPosition = new Vector3(0, 0.8f);
        cameraObj.transform.parent = gameObject.transform;
        camera = cameraObj.GetComponent<Camera>();
        mouseLook = gameObject.GetComponent<MouseLook>();

        mouseLook.Init(transform, camera.transform);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        RotateView();
        jump = Input.GetButton("Jump");

        // Do forward raycast from camera to the center of the screen to see if an upgradeable object is in front of the player
        int x = Screen.width / 2;
        int y = Screen.height / 2;
        Ray ray = camera.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 5.0f))
        {
            if (hitInfo.collider.CompareTag("Upgrade"))
            {
                upgradeText.text = "Press and hold E to upgrade";
            }
            else
            {
                ResetUpgradeText();
            }
        }
        else
        {
            ResetUpgradeText();
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        float speed;
        GetInput(out speed);

        // Move the player if they have moved
        if (input.x != 0 && input.y != 0)
        {
            CmdMove(input, jump, !isWalking);
        }

        ProgressStepCycle(speed);
    }

    private void ProgressStepCycle(float speed)
    {
        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;
    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool waswalking = isWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        isWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = isWalking ? walkSpeed : runSpeed;
        input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (input.sqrMagnitude > 1)
        {
            input.Normalize();
        }
    }


    private void RotateView()
    {
        mouseLook.LookRotation(transform, camera.transform);
        // Send the rotaion to the server
        CmdSetRotation(transform.rotation);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
    }

    private void ResetUpgradeText()
    {
        // upgradeText.text = "";
    }
}
