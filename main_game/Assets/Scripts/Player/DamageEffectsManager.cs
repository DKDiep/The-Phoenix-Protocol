using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DamageEffectsManager : NetworkBehaviour {

    private VideoGlitches.VideoGlitchSpectrumOffset lowHealth;

    [ClientRpc]
    public void RpcDamage(float t_amount)
    {
        if(lowHealth == null)
            lowHealth = Camera.main.GetComponent<VideoGlitches.VideoGlitchSpectrumOffset>();
        lowHealth.amount = t_amount;
    }

    [ClientRpc]
    public void RpcDistortion()
    {
        if(lowHealth == null)
            lowHealth = Camera.main.GetComponent<VideoGlitches.VideoGlitchSpectrumOffset>();
        lowHealth.amount = 1.0f;
        StartCoroutine(ReduceEffect());
    }

    [ClientRpc]
    public void RpcReset()
    {
        if(lowHealth == null)
            lowHealth = Camera.main.GetComponent<VideoGlitches.VideoGlitchSpectrumOffset>();
        lowHealth.amount = 0;
    }

    private IEnumerator ReduceEffect()
    {
        lowHealth.amount -= 0.01f;
        yield return new WaitForSeconds(0.1f);
        if(lowHealth.amount > 0)
            StartCoroutine(ReduceEffect()); 
    }
}
