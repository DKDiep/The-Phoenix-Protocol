using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class LoadingText : NetworkBehaviour
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Texture2D text;
	#pragma warning restore 0649

	private bool fadeSound = false;
    private bool gameStarted = false;

	private PlayerShooting[] shooting;
	private MissionManager missionManager;

	// Use this for initialization
	public void Play () 
    {
		if(shooting == null)
        {
            shooting = new PlayerShooting[4];
            for(int i = 0; i < 4; i++)
            {
                shooting[i] = GameObject.Find("PlayerShooting"+i).GetComponent<PlayerShooting>();
            }
        }


		if (missionManager == null)
			missionManager = GameObject.Find("MissionManager(Clone)").GetComponent<MissionManager>();

        StartCoroutine(Loaded());
	}

    public void Reset()
    {
        fadeSound = false;
        RpcReset();
    }

    [ClientRpc]
    void RpcReset()
    {
        fadeSound = false;
    }

    public void MuteAudio()
    {
        AudioListener.volume = 0;
        RpcMuteClientAudio();
    }

    [ClientRpc]
    void RpcMuteClientAudio()
    {
        AudioListener.volume = 0;
    }

    IEnumerator Loaded()
    {
        gameStarted = true;
        for(int i = 0; i < 4; i++)
		    shooting[i].SetShootingEnabled(false);

        yield return new WaitForSeconds(3f);
        
		fadeSound = true;
        GameObject.Find("MusicManager(Clone)").GetComponent<MusicManager>().PlayMusic(1);
        for(int i = 0; i < 4; i++)
		    shooting[i].SetShootingEnabled(true);
		missionManager.StartTimer();
        //Destroy(this, 3f);
    }

    void Update()
    {
        if(AudioListener.volume < 1f && fadeSound)
        {
            AudioListener.volume += 10f * Time.deltaTime;
        }
    }
	
    void OnGUI()
    {
        if(gameStarted && !fadeSound) 
			GUI.DrawTexture (new Rect (Screen.width - 200, Screen.height - 80, 195, 66), text, ScaleMode.ScaleToFit);
    }
}
