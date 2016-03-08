using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OutpostManager : MonoBehaviour {

    private GameState gameState;
    private PlayerController playerController;
    private float timeSinceLastEvent = 0;
    private GameObject testOutpost;
    private GameObject canvas;
    private List<GameObject> arrowList = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        GameObject playerControllerObject = GameObject.Find("PlayerController(Clone)");
        playerController = playerControllerObject.GetComponent<PlayerController>();
        canvas = GameObject.Find("CrosshairCanvas(Clone)");
    }

    // Update is called once per frame
    void Update () {
        timeSinceLastEvent += Time.deltaTime;
        List<GameObject> outpostList = gameState.GetOutpostList();
        bool arrowListInstantiated = false;
        if (timeSinceLastEvent > 10)
        {
            if (outpostList != null && outpostList.Count > 0)
            {
                playerController.RpcOutpostNotification("Outpost found");
                testOutpost = outpostList[1];
            }
            timeSinceLastEvent = 0;
        }
        if(canvas == null) canvas = GameObject.Find("CrosshairCanvas(Clone)");
        //note canvas might still be null if CrosshairCanvas isn't created yet
        if (outpostList != null && canvas != null && outpostList.Count > 0)
        {
            if (!arrowListInstantiated)
            {
                for (int i = 0; i < outpostList.Count; i++) arrowList.Add(Instantiate(Resources.Load("Prefabs/IndicatorArrow", typeof(GameObject))) as GameObject);
                arrowListInstantiated = true;
            }
            for (int index = 0; index < outpostList.Count; index++)
            {
                Indicator(outpostList[index], index);
            }
        }
    }

    public void giveGameStateReference(GameState newGameState)
    {
        gameState = newGameState;
    }

    private void Indicator(GameObject outpost, int index)
    {
        Vector3 screenpos = Camera.main.WorldToScreenPoint(outpost.transform.position);
        if (screenpos.z > 0 &&
            screenpos.x > 0 && screenpos.x < Screen.width &&
            screenpos.y > 0 && screenpos.y < Screen.height)
        {
            arrowList[index].SetActive(false);
        }
        else
        {   
            if (screenpos.z < 0)
            {
                screenpos *= -1;
            }
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
            //make (0,0,z) the center of the screen as opposed to bottom left
            screenpos -= screenCenter;

            //find angle from center of screen to object position
            float angle = Mathf.Atan2(screenpos.y, screenpos.x);
            angle -= 90 * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = -Mathf.Sin(angle);

            screenpos = screenCenter + new Vector3(sin * 150, cos * 150, 0);

            //y = mx + b format
            float m = cos / sin;

            //this determines how far away from the edge of the sceen the indicators lie
            Vector3 screenBounds = screenCenter * 0.9f;
            //checks if above the center of screen
            if (cos > 0)
            {
                screenpos = new Vector3(screenBounds.y / m, screenBounds.y, 0);
            }
            else
            {
                screenpos = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);
            }
            if (screenpos.x > screenBounds.x)
            {//out of bounds to the right
                screenpos = new Vector3(screenBounds.x, screenBounds.x * m, 0);
            }
            else if (screenpos.x < -screenBounds.x)
            { //out of bounds to the left
                screenpos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
            }
                RectTransform arrowRectTransform = (RectTransform)arrowList[index].transform;
            //remove coordinate translation
            //screenpos += screenCenter;
            arrowList[index].transform.SetParent(canvas.transform);
            arrowRectTransform.anchoredPosition = screenpos;
            arrowRectTransform.localRotation= Quaternion.Euler(0, 0, angle * Mathf.Deg2Rad);
            arrowList[index].SetActive(true);
        }
    }
}