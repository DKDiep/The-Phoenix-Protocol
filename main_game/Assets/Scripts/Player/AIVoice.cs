using UnityEngine;
using System.Collections;

public class AIVoice : MonoBehaviour {

    /*
       Command List
       0 - 3: Enemy ship destroyed
        4: Missiles launched
        5: Boosting
        6: Shield Overdrive
        7: Stun activated
        8: Ability on cooldown
        9: Mission accomplished
        10: Shield fully charged
        11: Shield failure
        12: Repair complete
        13: Upgrade complete
        14: Structural integrity failing
        15: Life support systems failing
        16: System failure imminent
        17: All systems functional
        18: Purchased system
        19 - 21: Enemies incoming
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
