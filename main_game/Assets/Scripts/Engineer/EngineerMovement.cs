using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EngineerMovement : MonoBehaviour
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private bool isWalking;
	[SerializeField] private float walkSpeed;
	[SerializeField] private float runSpeed;
	[SerializeField] private MouseLook mouseLook;
	[SerializeField] private float stepInterval;
    [SerializeField] private Text upgradeText;
	#pragma warning restore 0649

    private Camera m_Camera;
	private Vector2 input;
	private float stepCycle;
	private float nextStep;

    // Use this for initialization
    private void Start()
    {
        m_Camera = gameObject.GetComponent<Camera>();
        stepCycle = 0f;
        nextStep = stepCycle / 2f;
        mouseLook.Init(transform, m_Camera.transform);
    }

    // Update is called once per frame
    private void Update()
    {
        RotateView();

        // Do forward raycast from camera to the center of the screen to see if an upgradeable object is in front of the player
        int x = Screen.width / 2;
        int y = Screen.height / 2;
        Ray ray = m_Camera.ScreenPointToRay(new Vector3(x, y, 0));
        RaycastHit hitInfo;

		if (Physics.Raycast(ray, out hitInfo, 5.0f))
		{
			if (hitInfo.collider.CompareTag("Upgrade"))
			{
				upgradeText.text = "Press and hold E to upgrade";
			}
		}
    }

    private void FixedUpdate()
    {
        float speed;
        GetInput(out speed);

        ProgressStepCycle(speed);
    }

    private void ProgressStepCycle(float speed)
    {
        if (!(stepCycle > nextStep))
        {
            return;
        }

        nextStep = stepCycle + stepInterval;
    }

    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

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
        mouseLook.LookRotation(transform, m_Camera.transform);
    }
}