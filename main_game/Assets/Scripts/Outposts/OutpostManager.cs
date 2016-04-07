using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//Also manages the portal!
public class OutpostManager : MonoBehaviour {

    private GameState gameState;
    private PlayerController playerController;
    private float timeSinceLastEvent = 0;
    private GameObject canvas;
    private GameObject portal;
    private GameObject portalArrow;
    private List<GameObject> arrowList = new List<GameObject>();
    private List<GameObject> outpostList;
    private List <OutpostLogic> outpostLogic = new List<OutpostLogic>();
    private List<int> objectiveIds;
    private List<int> completedObjectiveIds;
    private int arrowsRequired = 0;
    public bool outpostSpawned = false;
    public bool portalArrowSpawned = false;
    Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
    Vector3 screenBounds;
    private Color darkRed;
    private Color darkYellow;
    private Color darkGreen;
    private Color darkPurple;
	private Camera mainCamera;

    void Start()
    {
        GameObject playerControllerObject = GameObject.Find("PlayerController(Clone)");
        playerController = playerControllerObject.GetComponent<PlayerController>();
        canvas = GameObject.Find("CrosshairCanvas(Clone)");
        screenBounds = screenCenter * 0.9f;
        darkRed = new Color(0.62f,0,0,0.4f);
        darkYellow = new Color(0.62f,0.57f,0,0.4f);
        darkGreen = new Color(0,0.62f,0,0.4f);
        darkPurple = new Color(0.62f, 0, 0.62f, 0.4f);
		mainCamera = Camera.main;
        StartCoroutine(UpdateOutposts());
    }

    public void Reset()
    {
        for (int i = arrowList.Count - 1; i >= 0; i--)
        {
            Destroy(arrowList[i]);
        }
        Destroy(portalArrow);
        portalArrowSpawned = false;
        arrowList = new List<GameObject>();
        Canvas.ForceUpdateCanvases();
        outpostLogic = new List<OutpostLogic>();
        arrowsRequired = 0;
    }

    private IEnumerator UpdateOutposts()
    {
        outpostList = gameState.GetOutpostList();
        if(outpostList.Count != 0 && outpostList != null)
        {
            SpawnOutpostArrows();
            DiscoverOutposts(); 
            UpdateOutpostArrows(); 
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(UpdateOutposts());
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
                        Indicator(outpostList[index], index, false);
                    else
                        arrowList[index].SetActive(false);
                }
                if (portalArrowSpawned) Indicator(portal, 0, true);
            }
        }
    }

    /// <summary>
    /// Discovers the outposts based on time and distance.
    /// </summary>
    private void DiscoverOutposts()
    {
        timeSinceLastEvent += 1f;

        if (timeSinceLastEvent > 10)
        {
            timeSinceLastEvent = 0;
            for(int i = 0; i < outpostList.Count; i++)
            {
                if (outpostLogic[i].discovered == false)
                {
                    if (Vector3.Distance(outpostList[i].transform.position, mainCamera.transform.position) < 2000)
                    {
                        outpostLogic[i].discovered = true;
                        if(outpostList[i] != null)
                            playerController.RpcOutpostNotification(outpostList[i], outpostLogic[i].id, outpostLogic[i].GetDifficulty());
                        // Show the outpost target
                        outpostList[i].GetComponent<OutpostTarget>().ShowTarget();
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
                arrowList[i].GetComponent<Image>().color = darkRed;
                outpostLogic.Add(outpostList[i].GetComponentInChildren<OutpostLogic>());
            }
            //this probably won't be needed but I'm keeping it here for now just in case this turns out to be an issue - luke 06/04
            /*  Outpost targets can be set and (in theory) completed before the arrowlist is spawned. If they are, then the ids 
                of the arrows that should be marked as objectives/completedobjectives are added to objectiveIds/completedObjectiveIds.
                The colors of the arrows are changed here accordingly, then the ids are removed from the lists 
            for (int id = objectiveIds.Count - 1; id >= 0; id--)
            {
                if (id < outpostList.Count)
                    arrowList[id].GetComponent<Image>().color = darkYellow;
                    arrowList.RemoveAt(id);
            }
            for (int id = completedObjectiveIds.Count - 1; id >= 0; id--)
            {
                if (id < outpostList.Count)
                    arrowList[id].GetComponent<Image>().color = darkGreen;
                arrowList.RemoveAt(id);
            }*/
            if (!portalArrowSpawned)
            {
                portal = gameState.Portal;
                portalArrow = Instantiate(Resources.Load("Prefabs/IndicatorArrow", typeof(GameObject))) as GameObject;
                portalArrow.GetComponent<Image>().color = darkPurple;
                portalArrowSpawned = true;
            }
            arrowsRequired = outpostList.Count;
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
            distance = (int)Vector3.Distance(outpostList[i].transform.position, mainCamera.transform.position);
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
            distance = (int)Vector3.Distance(outpostList[i].transform.position, mainCamera.transform.position);
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
        if (id < arrowList.Count)
        {
            print("id = " + id + ", arrowList.Count = " + arrowList.Count);
            Image arrowImage = arrowList[id].GetComponent<Image>();
            arrowImage.color = darkYellow;
        }
        //else objectiveIds.Add(id);      //if the arrow hasn't been instantiated yet, add the id to a list, so the color can be changed when the arrows are spawned
        outpostList[id].GetComponent<OutpostTarget>().StartMission();
    }

    public void endMission(int id)
    {
        if (id < arrowList.Count)
        {
            Image arrowImage = arrowList[id].GetComponent<Image>();
            arrowImage.color = darkGreen;
        }
        else
        {
            completedObjectiveIds.Add(id);
        }
        outpostList[id].GetComponent<OutpostTarget>().EndMission();
    }

    private void Indicator(GameObject targetObject, int index, bool forPortal) //Second parameter doesn't matter if it's portal
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetObject.transform.position);
        GameObject arrow;
        if(!forPortal) arrow = arrowList[index];
        else arrow = portalArrow;
        //If the object is on-screen then set its arrow to be inactive 
        if (screenPos.z > 0 &&
            screenPos.x > 0 && screenPos.x < Screen.width &&
            screenPos.y > 0 && screenPos.y < Screen.height)
        {
            arrow.SetActive(false);    
        }
        else
        {   
            if (screenPos.z < 0)
                screenPos *= -1;

            //make (0,0,z) the center of the screen as opposed to bottom left
            screenPos -= screenCenter;

            //find angle from center of screen to object position
            float angle = Mathf.Atan2(screenPos.y, screenPos.x);
            angle -= 90 * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = -Mathf.Sin(angle);

            screenPos = screenCenter + new Vector3(sin * 150f, cos * 150f, 0);

            //y = mx + b format
            float m = cos / sin;


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
            RectTransform arrowRectTransform = (RectTransform)arrow.transform;

            arrowRectTransform.anchoredPosition = screenPos;
            arrow.transform.SetParent(canvas.transform);
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            arrow.SetActive(true);
        }
    }
}