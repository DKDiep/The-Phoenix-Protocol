using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class OutpostManager : MonoBehaviour {

    private GameState gameState;
    private PlayerController playerController;
    private float timeSinceLastEvent = 0;
    private GameObject canvas;
    private List<GameObject> arrowList = new List<GameObject>();
    private List<GameObject> outpostList;
    private List <OutpostLogic> outpostLogic = new List<OutpostLogic>();
    private int arrowsRequired = 0;
    public bool outpostSpawned = false;
    void Start()
    {
        GameObject playerControllerObject = GameObject.Find("PlayerController(Clone)");
        playerController = playerControllerObject.GetComponent<PlayerController>();
        canvas = GameObject.Find("CrosshairCanvas(Clone)");

    }

    void Update () 
    {
        outpostList = gameState.GetOutpostList();
        if(outpostList.Count != 0 && outpostList != null)
        {
            SpawnOutpostArrows();
            DiscoverOutposts();
            UpdateOutpostArrows(); 
        }
    }

    /// <summary>
    /// Updates the outpost indicator arrows.
    /// </summary>
    private void UpdateOutpostArrows()
    {
        if(gameState.Status == GameState.GameStatus.Started) 
        {
            if(canvas == null) 
                canvas = GameObject.Find("CrosshairCanvas(Clone)");
            //note canvas might still be null if CrosshairCanvas isn't created yet
            if (outpostList != null && canvas != null && outpostList.Count > 0)
            {
                /*A list of arrows is instantiated such that the index of each arrow is 
                    the same as the index of the outpostList object it tracks. Note that 
                    this means that outposts added to the list part-way through execution will not be tracked*/

                for (int index = 0; index < outpostList.Count; index++)
                {
                    if (outpostLogic[index].discovered && !outpostLogic[index].resourcesCollected && 
                        !outpostLogic[index].civiliansCollected && outpostList[index] != null
                    )
                        Indicator(outpostList[index], index);
                    else
                        arrowList[index].SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Discovers the outposts based on time and distance.
    /// </summary>
    private void DiscoverOutposts()
    {
        timeSinceLastEvent += Time.deltaTime;

        if (timeSinceLastEvent > 10)
        {
            timeSinceLastEvent = 0;
            for(int i = 0; i < outpostList.Count; i++)
            {
                if (outpostLogic[i].discovered == false)
                {
                    if (Vector3.Distance(outpostList[i].transform.position, Camera.main.transform.position) < 2000)
                    {
                        outpostLogic[i].discovered = true;
                        if(outpostList[i] != null)
                            playerController.RpcOutpostNotification(outpostList[i], outpostLogic[i].id, outpostLogic[i].GetDifficulty());
                        // Show the outpost target
                        outpostList[i].GetComponentsInChildren<OutpostTarget>()[0].ShowTarget();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Spawns the outpost arrows.
    /// </summary>
    private void SpawnOutpostArrows()
    {
        if(arrowsRequired < outpostList.Count)
        {
            for (int i = arrowsRequired; i < outpostList.Count; i++)
            {
                arrowList.Add(Instantiate(Resources.Load("Prefabs/IndicatorArrow", typeof(GameObject))) as GameObject);
                outpostLogic.Add(outpostList[i].GetComponentInChildren<OutpostLogic>());
                arrowsRequired++;
            }
        }
    }

    /// <summary>
    /// Gets the closest outpost distance.
    /// </summary>
    /// <returns>The closest outpost distance.</returns>
    public int GetClosestOutpostDistance()
    {
        int minDistance = -1;
        int distance;

        for(int i = 0; i < outpostList.Count; i++)
        {
            distance = (int)Vector3.Distance(outpostList[i].transform.position, Camera.main.transform.position);
            if(distance < minDistance || minDistance == -1)
            {
                minDistance = distance; 
            }
        }
        return minDistance;
    }

    /// <summary>
    /// Gets the closest outpost.
    /// </summary>
    /// <returns>The closest outpost.</returns>
    public int GetClosestOutpost()
    {
        if(!outpostSpawned)
            return -1;
        
        int minDistance = -1;
        int minId = -1;
        int distance;

        for(int i = 0; i < outpostList.Count; i++)
        {
            distance = (int)Vector3.Distance(outpostList[i].transform.position, Camera.main.transform.position);
            if(distance < minDistance || minDistance == -1)
            {
                minDistance = distance; 
                minId = i;
            }
        }
        return minId;
    }

    public void giveGameStateReference(GameState newGameState)
    {
        gameState = newGameState;
    }

    public void setMissionTarget(int id)
    {
        Image arrowImage = arrowList[id].GetComponent<Image>();
        arrowImage.color = Color.yellow;
        outpostList[id].GetComponentsInChildren<OutpostTarget>()[0].StartMission();
    }

    public void endMission(int id)
    {
        Image arrowImage = arrowList[id].GetComponent<Image>();
        arrowImage.color = Color.red;
        outpostList[id].GetComponentsInChildren<OutpostTarget>()[0].EndMission();
    }

    private void Indicator(GameObject outpost, int index)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(outpost.transform.position);
        //If the object is on-screen then set its arrow to be inactive 
        if (screenPos.z > 0 &&
            screenPos.x > 0 && screenPos.x < Screen.width &&
            screenPos.y > 0 && screenPos.y < Screen.height)
        {
            arrowList[index].SetActive(false);
        }
        else
        {   
            if (screenPos.z < 0)
            {
                screenPos *= -1;
            }
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
            //make (0,0,z) the center of the screen as opposed to bottom left
            screenPos -= screenCenter;

            //find angle from center of screen to object position
            float angle = Mathf.Atan2(screenPos.y, screenPos.x);
            angle -= 90 * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = -Mathf.Sin(angle);

            screenPos = screenCenter + new Vector3(sin * 150, cos * 150, 0);

            //y = mx + b format
            float m = cos / sin;

            //this determines how far away from the edge of the sceen the indicators lie
            Vector3 screenBounds = screenCenter * 0.9f;
            //checks if above the center of screen
            if (cos > 0)
            {
                screenPos = new Vector3(screenBounds.y / m, screenBounds.y, 0);
            }
            else
            {
                screenPos = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);
            }
            if (screenPos.x > screenBounds.x)
            {//out of bounds to the right
                screenPos = new Vector3(screenBounds.x, screenBounds.x * m, 0);
            }
            else if (screenPos.x < -screenBounds.x)
            { //out of bounds to the left
                screenPos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
            }
            RectTransform arrowRectTransform = (RectTransform)arrowList[index].transform;
            arrowList[index].transform.SetParent(canvas.transform);
            arrowRectTransform.anchoredPosition = screenPos;
            arrowList[index].transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            arrowList[index].SetActive(true);
        }
    }
}