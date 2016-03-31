using UnityEngine;
using System.Collections;
using WiimoteApi;
using System;
using System.Threading;

public class WiiRemoteManager : MonoBehaviour {
	
	public void RumbleAll(float rumbleTime) 
	{
		if (WiimoteManager.Wiimotes.Count > 0) 
		{
			// Start rumble on all wii remotes
			foreach(Wiimote remote in WiimoteManager.Wiimotes) 
			{
				remote.RumbleOn = true; 
				remote.SendStatusInfoRequest(); 
			}

			// `It's rumble time baby`
			StartCoroutine(DelayRumble(rumbleTime));
		}
	 
	}

    public void SetPlayerLeds(Wiimote remote, int remoteId) 
    {
        // Set the LEDs on each wii remote to indicate which player is which
        if(remoteId == 0) remote.SendPlayerLED (true, false, false, false);
        if(remoteId == 1) remote.SendPlayerLED (false, true, false, false);
        if(remoteId == 2) remote.SendPlayerLED (false, false, true, false);
        if(remoteId == 3) remote.SendPlayerLED (false, false, false, true);
    }
	public int GetNumberOfRemotes() 
	{
		return WiimoteManager.Wiimotes.Count;
	}

	IEnumerator DelayRumble(float delay)
	{
		yield return new WaitForSeconds(delay/1000);

		foreach(Wiimote remote in WiimoteManager.Wiimotes) 
		{
			remote.RumbleOn = false; 
			remote.SendStatusInfoRequest();
		}
	}

}


