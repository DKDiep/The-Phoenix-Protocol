using UnityEngine;
using System.Collections;

public abstract class CommanderAbility : MonoBehaviour {

    internal float cooldown;
    internal float duration;
    internal bool ready = true;
    internal GameSettings settings;
    internal GameState state;
    internal AudioSource audioSource;
    public AudioClip soundEffect;

    internal abstract void ActivateAbility();
    internal abstract void DeactivateAbility();

    internal IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(cooldown);
        ready = true;
    }

    internal IEnumerator DeactivateTimer()
    {
        yield return new WaitForSeconds(duration);
        DeactivateAbility();
        StartCoroutine(CoolDown());
    }

    internal void UseAbility()
    {
		// Do not fire an ability unless the game is still in progress
		if (state.Status != GameState.GameStatus.Started)
			return;

        if(ready)
        {
            if(audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if(soundEffect != null)
                audioSource.PlayOneShot(soundEffect);
            ready = false;
            StartCoroutine(DeactivateTimer());
            ActivateAbility();
        }
        else
        {
            AIVoice.SendCommand(8);
        }
     }
	
}
