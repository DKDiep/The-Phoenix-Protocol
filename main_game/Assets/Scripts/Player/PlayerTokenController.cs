using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerTokenController : MonoBehaviour {

    [SerializeField]
    private Scrollbar scrollbar;
    private PlayerController playerController;

    void Start ()
    {
	
	}

    public void SetPlayerController(GameObject playerObject)
    {
        playerController = playerObject.GetComponent<PlayerController>();
    }

    public void onScrollBar(float value)
    {
        if (playerController != null)
        {
            if (value > 0.5)
            {
                playerController.SetOrientation(1);
                Debug.Log("Right");
            }
            else
            {
                playerController.SetOrientation(-1);
                Debug.Log("Left");
            }
        }
    }
}
