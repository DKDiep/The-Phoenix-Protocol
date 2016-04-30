using UnityEngine;
using System.Collections;

public class CommanderVoice : MonoBehaviour {

    /*
       Command List
       0 - Intro
       1 - Outpost mission start
       2 - Commander ability start
       3 - Repair mission start
       4 - Upgrade mission start
       5 - Mothership spawn
       6 - Death
       7 - Victory
    */

    public AudioClip[] aiClips;
    private AudioSource mySource;
    public static CommanderVoice aiObject;
    private GameState state;

    public static void SendCommand(int id)
    {
        if(aiObject == null)
            aiObject = Camera.main.gameObject.GetComponent<CommanderVoice>();
        aiObject.PlaySound(id);
    }

    public static bool IsPlaying()
    {
        if(aiObject != null)
            return aiObject.mySource.isPlaying;
        else 
            return false;
    }

    public void PlaySound(int id)
    {
        if(mySource == null)
            mySource = GetComponent<AudioSource>(); 

            Debug.Log("Commander voice command received: " + id);

        if(state == null)
        {
            state = GameObject.Find("GameManager").GetComponent<GameState>();
        }

        if(state.Status == GameState.GameStatus.Started)
        {
            mySource.clip = aiClips[id];
            mySource.Play();
        }
        else if(id == 6 || id == 7)
        {
            StartCoroutine(DelayedSpeech(id));
        }


    }

    public IEnumerator DelayedSpeech(int id)
    {
        yield return new WaitForSeconds(2f);
        mySource.clip = aiClips[id];
        mySource.Play();
    }
}
