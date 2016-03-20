using UnityEngine;
using System.Collections;

public abstract class CommanderAbility : MonoBehaviour {

    internal float cooldown;
    internal bool ready = true;

    internal abstract void AbilityEffect();

    internal IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(cooldown);
        ready = true;
    }

    internal void UseAbility()
    {
        if(ready)
        {
            ready = false;
            StartCoroutine("CoolDown");
            AbilityEffect();
        }
     }
	
}
