using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReadyScreen : MonoBehaviour {

    [SerializeField]
    private Button goButton;

    private ServerManager serverManager;

    void Awake()
    {
        GameObject server = GameObject.Find("GameManager");
        AudioListener.volume = 0;
        if (server != null)
        {
            serverManager = server.GetComponent<ServerManager>();
            goButton.onClick.AddListener(() => OnClickStartButton());
        }
    }

    public void OnClickStartButton()
    {
        // Start the game
        serverManager.Play();
        serverManager.cutsceneManager.GetComponent<LoadingText>().Play();
        serverManager.cutsceneManager.GetComponent<FadeTexture>().Play();
        AudioListener.volume = 1;
        // Disable self until restart
        gameObject.SetActive(false);
    }
}
