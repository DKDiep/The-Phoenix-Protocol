using UnityEngine;
using System.Collections;

public class AIVoice : MonoBehaviour {

    /*
       Command List
       1 - 4: Enemy ship destroyed
        5: Missiles launched
        6: Boosting
        7: Shield Overdrive
        8: Stun activated
        9: Ability on cooldown
        10: Mission accomplished
        11: Shield fully charged
        12: Shield failure
        13: Repair complete
        14: Upgrade complete
        15: Structural integrity failing
        16: Life support systems failing
        17: System failure imminent
        18: All systems functional
        19: Purchased system
        20 - 22: Enemies incoming
    */

    public AudioClip[] aiClips;
    private AudioSource mySource;
    public static AIVoice aiObject;

	public static void SendCommand(int id)
    {
        if(aiObject == null)
            aiObject = Camera.main.gameObject.GetComponent<AIVoice>();
        aiObject.PlaySound(id);
    }

    public void PlaySound(int id)
    {
        if(mySource == null)
            mySource = GetComponent<AudioSource>(); 
        if(!mySource.isPlaying)
        {
            mySource.clip = aiClips[id];
            mySource.Play();
        }

    }
}
