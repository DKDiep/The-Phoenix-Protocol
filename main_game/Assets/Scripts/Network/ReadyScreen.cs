using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReadyScreen : MonoBehaviour {

    [SerializeField]
    private Button goButton;

    private ServerManager serverManager;
    private MusicManager musicManager;

    void Awake()
    {
        GameObject server = GameObject.Find("GameManager");

        if (server != null)
        {
            serverManager = server.GetComponent<ServerManager>();
            goButton.onClick.AddListener(() => OnClickStartButton());
        }
    }

    void Start()
    {
        //serverManager.cutsceneManager.GetComponent<LoadingText>().MuteAudio();
        musicManager = GameObject.Find("MusicManager(Clone)").GetComponent<MusicManager>();
        musicManager.PlayMusic(1);
    }

    public void OnClickStartButton()
    {
        // Start the game
        serverManager.Play();
        serverManager.cutsceneManager.GetComponent<LoadingText>().Play();
        serverManager.cutsceneManager.GetComponent<FadeTexture>().Play();
        // Disable self until restart
        gameObject.SetActive(false);
    }
}
