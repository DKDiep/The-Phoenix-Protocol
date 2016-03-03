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
        if (server != null)
        {
            serverManager = server.GetComponent<ServerManager>();
            goButton.onClick.AddListener(() => OnClickStartButton());
        }
    }

    public void OnClickStartButton()
    {
        // Start the game
        serverManager.StartGame();
        // Disable self until restart
        gameObject.SetActive(false);
    }
}
