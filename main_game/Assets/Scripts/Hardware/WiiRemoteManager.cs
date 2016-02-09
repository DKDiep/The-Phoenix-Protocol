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
			StartCoroutine("DelayRumble", rumbleTime);
		}
	 
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


