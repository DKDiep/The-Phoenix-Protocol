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
    private int lastCommand = -1;
    public static AIVoice aiObject;
    private bool minDelay = false;
    private GameState state;

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

        if(state == null)
        {
            state = GameObject.Find("GameManager").GetComponent<GameState>();
        }
        if (state.Status != GameState.GameStatus.Started)
            return;

        if(!mySource.isPlaying && id != lastCommand && !minDelay)
        {
            mySource.clip = aiClips[id];
            mySource.Play();
            lastCommand = id;
            minDelay = true;
            StartCoroutine(ResetDelay(lastCommand));
        }

    }

    // If the AI hasn't said anything for a while seconds, allow them to say the same thing again
    IEnumerator ResetDelay(int lastId)
    {
        yield return new WaitForSeconds(3f);
        minDelay = false;
        yield return new WaitForSeconds(3f);
        if(lastId == lastCommand)
            lastCommand = -1;
    }
}
