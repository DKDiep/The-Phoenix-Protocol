using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DisableSpawnEffect : NetworkBehaviour {

	public void DisableParticles()
    {
        RpcDisable();
    }

    [ClientRpc]
    private void RpcDisable()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        for(int i = 0; i < particles.Length; i++)
        {
            particles[i].enableEmission = false;
        }
    }
}
