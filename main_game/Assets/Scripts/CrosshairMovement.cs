using UnityEngine;
using System.Collections;
using WiimoteApi;
using System;

public class CrosshairMovement : MonoBehaviour {

    private int controlling = 0;
    private const int N_CROSSHAIRS = 4;

	private bool init = true;
	private Vector3 initPos;
	private float movx;
	private float movy;
	public static Vector3 crosshairPosition;

	// Use this for initialization
	void Start ()
    {
		StartCoroutine(FindRemotes());
	}
	
	// Update is called once per frame
    void Update()
    {
        // Check to see if any of the crosshair keys have been pressed
        for (int i = 0; i < N_CROSSHAIRS; i++)
            if (Input.GetKeyDown(i.ToString()))
            {
                controlling = i;
                Debug.Log("Controlling " + i);
            }

    }

	void FixedUpdate ()
    {
        // Get the currently controlled crosshair
        Transform selectedCrosshair = this.transform.Find("CrosshairImage" + controlling);

		// Control crosshair by mouse if there is no wii remote connected.
		if (WiimoteManager.Wiimotes.Count < 1) 
		{
			// Update its position to the current mouse position
			crosshairPosition = selectedCrosshair.position;
			crosshairPosition.x = Input.mousePosition.x;
			crosshairPosition.y = Input.mousePosition.y;
			selectedCrosshair.position = crosshairPosition;
		} 
		else 
		{
			Wiimote remote;

			try 
			{
				remote = WiimoteManager.Wiimotes [0];
				if (this.init) 
				{
					remote.SendPlayerLED (true, false, false, false);
					remote.SetupIRCamera (IRDataType.BASIC);
					this.init = false;
				}

				if (remote.ReadWiimoteData () > 0) 
				{ 
					float[] pointer = remote.Ir.GetPointingPosition ();
					
					if(pointer[0] != -1 && pointer[1] != -1) 
					{
						Vector3 position = selectedCrosshair.position;
						position.x = pointer[0] * Screen.width;
						position.y = pointer[1] * Screen.height;
						selectedCrosshair.position = position;
					}
				}
			} 
			catch (Exception e) 
			{
				WiimoteManager.FindWiimotes ();
				this.init = true;
			}  
		}
    }

	IEnumerator FindRemotes()
	{	
		WiimoteManager.FindWiimotes ();
		yield return new WaitForSeconds(5f);
	}

}
