using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerTokenController : MonoBehaviour
{
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields    
	[SerializeField] private Scrollbar scrollbar;
	#pragma warning restore 0649

    private PlayerController playerController;

    void Start ()
    {
	
	}

    public void SetPlayerController(GameObject playerObject)
    {
        playerController = playerObject.GetComponent<PlayerController>();
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }

    public void OnScrollBar(float value)
    {
        if (playerController != null)
        {
            if (value > 0.5)
            {
                Debug.Log("Right");
            }
            else
            {
                Debug.Log("Left");
            }
        }
    }

    public void OnDrag()
    {
        transform.position = Input.mousePosition;
    }
}
