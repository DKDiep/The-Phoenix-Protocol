using UnityEngine;
using System.Collections;
using WiimoteApi;
using System;

public class CrosshairMovement : MonoBehaviour {

    private int controlling = 0;
    private const int N_CROSSHAIRS = 4;
	float posReadDelay = 0.0001f;
	private bool init = true;
	private Vector3 initPos;
	private float movx;
	private float movy;
	bool canMove = true;
	public Vector3 crosshairPosition;
	private Vector3 oldCrosshairPosition;
	private Vector3 crosshairPositionTmp;
	float oldAccel, newAccel;
    GameObject[] crosshairs;

	// Use this for initialization
	void Start ()
    {
        GameObject crosshairContainer = GameObject.Find("Crosshairs");

        crosshairs = new GameObject[4];

        // Find crosshairs
        for(int i = 0; i < 4; ++i)
        {
            crosshairs[i] = crosshairContainer.transform.GetChild(i).gameObject;
        }


		StartCoroutine(FindRemotes());
	}
	
	// Update is called once per frame
    void Update()
    {
        // Check to see if any of the crosshair keys have been pressed
		for (int i = 1; i <= N_CROSSHAIRS; i++) 
		{
			if (Input.GetKeyDown (i.ToString ())) 
			{
				controlling = i-1;
				Debug.Log ("Controlling " + controlling);
			}
		}
		Transform selectedCrosshair = crosshairs[controlling].transform;

		//time delta time
		if(crosshairPositionTmp == oldCrosshairPosition) 
		{
			if(Math.Abs(selectedCrosshair.position.x) < Math.Abs(crosshairPosition.x) &&
			   Math.Abs(selectedCrosshair.position.y) < Math.Abs(crosshairPosition.y)) 
			{
				selectedCrosshair.position = selectedCrosshair.position + (crosshairPosition/50);
			}
		} 
		else 
		{
			selectedCrosshair.position = oldCrosshairPosition;
			crosshairPositionTmp = oldCrosshairPosition;
		}

    }

	void FixedUpdate ()
    {
        // Get the currently controlled crosshair
        Transform selectedCrosshair = crosshairs[controlling].transform;

		// Control crosshair by mouse if there is no wii remote connected.
		if (WiimoteManager.Wiimotes.Count < 1) 
		{
			// Update its position to the current mouse position
			this.crosshairPosition = selectedCrosshair.position;
			this.crosshairPosition.x = Input.mousePosition.x;
			this.crosshairPosition.y = Input.mousePosition.y;
			selectedCrosshair.position = this.crosshairPosition;
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

					// If not able to move, run smoothing
					if(!canMove) 
					{
						
					} 
					else 
					{
						if(pointer[0] != -1 && pointer[1] != -1) 
						{
							

							oldAccel = newAccel;
							newAccel = remote.Accel.GetCalibratedAccelData()[1] + remote.Accel.GetCalibratedAccelData()[2];
							// If there is little movement, don't bother doing this. (Should stop shaking)
							if(Math.Abs(newAccel - oldAccel) > 0.03) 
							{
								oldCrosshairPosition = crosshairPosition;
								Vector3 position = selectedCrosshair.position;
								position.x = pointer[0] * Screen.width;
								position.y = pointer[1] * Screen.height;
								crosshairPosition = position;
								//Debug.Log(oldCrosshairPosition-crosshairPosition);
								canMove = false;
								StartCoroutine("Delay");
							}
						}
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

	IEnumerator Delay()
	{
		yield return new WaitForSeconds(posReadDelay);
		canMove = true;
	}

}
