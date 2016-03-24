using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReadyScreen : MonoBehaviour
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Button goButton;
	#pragma warning restore 0649

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
