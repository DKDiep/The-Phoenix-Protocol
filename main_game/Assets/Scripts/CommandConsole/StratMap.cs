using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class StratMap : MonoBehaviour {

    private GameObject playerIcon;
    private GameObject objectiveIcon;
    private RectTransform objectiveIconRectTransform;
    private Transform shipTransform;
    private RectTransform playerIconTransform;
    private float panelHeight;
    private float panelWidth;
    private int objective;
    private int scaleFactor = 40;
    private Vector3 mapDisplacement = new Vector3(2000, 0, 0);         //Only edit the first two arguments (x and y). 
    Dictionary<int,GameObject> outpostIconDict;
    public GameObject Portal { get; set; }
    private Sprite savedOutpostSprite;

	// Use this for initialization
	void Start () {
        var panel = this;
        shipTransform = GameObject.Find("PlayerShip(Clone)").transform;
        playerIcon = Instantiate(Resources.Load("Prefabs/ShipIcon", typeof(GameObject))) as GameObject;
        playerIcon.transform.SetParent(panel.transform, false);
        playerIcon.transform.SetParent(panel.transform, false);
        playerIconTransform = (RectTransform)playerIcon.transform;
        objectiveIcon  = Instantiate(Resources.Load("Prefabs/ObjectiveIcon", typeof(GameObject))) as GameObject;
        objectiveIcon.SetActive(false);
        objectiveIcon.transform.SetParent(panel.transform, false);
        objectiveIconRectTransform = (RectTransform)objectiveIcon.transform;
        RectTransform panelRectTransform = (RectTransform)panel.transform;
        panelHeight = panelRectTransform.sizeDelta.y;
        panelWidth = panelRectTransform.sizeDelta.x;
        PortalInit();
        outpostIconDict = new Dictionary<int,GameObject>();
        savedOutpostSprite = Resources.Load("Sprites/savedOutpost", typeof(Sprite)) as Sprite;
    }

    public void Reset()
    {
        outpostIconDict = new Dictionary<int, GameObject>();
        //Clear the icons
        for (int i = 0; i < this.transform.childCount; i++)
        {
            // Remove scripts that depend on object first
            /*Image img = this.transform.GetChild(i).gameObject.GetComponent<Image>();
            if (img != null)
                Destroy(img);*/
            if (this.transform.GetChild(i).name == "OutpostIcon(Clone)")
            {
                Debug.Log("rmv");
                Destroy(this.transform.GetChild(i).gameObject);
            }
        }
        objectiveIcon.SetActive(false);
        objective = -2;
    }

    public void NewOutpost(GameObject outpost, int id, int difficulty)
    {
        var panel = this;
        if (panel != null)  // make sure you actually found it!
        {
            GameObject outpostIcon = Instantiate(Resources.Load("Prefabs/OutpostIcon", typeof(GameObject))) as GameObject;
            outpostIcon.transform.SetParent(panel.transform, false);
            RectTransform outpostRectTransform = (RectTransform)outpostIcon.transform;
            Vector3 screenPos = new Vector3(outpost.transform.position.x/scaleFactor, outpost.transform.position.z/scaleFactor,0) + mapDisplacement/scaleFactor;
            outpostRectTransform.anchoredPosition = screenPos;
            if (WithinBounds(screenPos))
                outpostIcon.SetActive(true);
            else outpostIcon.SetActive(false);
            Image outpostImage = outpostIcon.GetComponent<Image>();
            outpostIconDict.Add(id, outpostIcon);
            if (id == objective) startMission(id); //This should only happen if an objective is set before an outpost is found
        }
    }

    public void PortalInit()
    {
        var panel = this;
        if (panel != null)  // make sure you actually found it!
        {
            GameObject portalSymbol = Instantiate(Resources.Load("Prefabs/PortalIcon", typeof(GameObject))) as GameObject;
            portalSymbol.transform.SetParent(panel.transform, false);
            RectTransform portalRectTransform = (RectTransform)portalSymbol.transform;
            Vector3 screenPos = new Vector3(Portal.transform.position.x / scaleFactor, Portal.transform.position.z / scaleFactor, 0) + mapDisplacement / scaleFactor;
            portalRectTransform.anchoredPosition = screenPos;
            if (WithinBounds(screenPos))
                portalSymbol.SetActive(true);
            else portalSymbol.SetActive(false);
        }
    }

    private bool WithinBounds(Vector3 screenPos)
    {
        if (screenPos.x > -(panelWidth/2) && screenPos.x < (panelWidth/2) && 
            screenPos.y > -(panelHeight/2) && screenPos.y < (panelHeight/2)) 
            return true;
        return false;
    }

    public void outpostVisitNotify(int id)
    {
        if (outpostIconDict[id] != null) outpostIconDict[id].GetComponent<Image>().sprite = savedOutpostSprite;
    }

    public void startMission(int id)
    {
        objective = id; 
        if (!outpostIconDict.ContainsKey(id)) print("outpost id : " + id + "searched for but not in stratmap dictionary");
        else {
            if (outpostIconDict[id] == null) print("outpostIconDict[id] == null");
            else
            {
                RectTransform outpostRectTransform = (RectTransform)outpostIconDict[id].transform;
                objectiveIconRectTransform.anchoredPosition = outpostRectTransform.anchoredPosition;
                objectiveIconRectTransform.sizeDelta = outpostRectTransform.sizeDelta;
                if(WithinBounds(objectiveIconRectTransform.position))
                    objectiveIcon.SetActive(true);
                else
                    objectiveIcon.SetActive(false);
            }
        }
    }

    public void endMission(int id)
    {
        //outpostIconDict[id].GetComponent<Image>().color = Color.green;
        objectiveIcon.SetActive(false);
    }

	// Update is called once per frame
	void Update () {
        Vector3 screenPos = new Vector3(shipTransform.position.x / scaleFactor, shipTransform.position.z / scaleFactor, 0) + mapDisplacement / scaleFactor;
        playerIconTransform.anchoredPosition = screenPos;
        Quaternion shipRotation = shipTransform.rotation;
        Vector3 eulerRotation = shipRotation.eulerAngles;
        Quaternion newRotation = Quaternion.identity;
        newRotation.eulerAngles = new Vector3(0, 0, -eulerRotation.y);
        playerIconTransform.localRotation = newRotation;
        if (WithinBounds(screenPos))
            playerIcon.SetActive(true);
        else playerIcon.SetActive(false);
    }
}
