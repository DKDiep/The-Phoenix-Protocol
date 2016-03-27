using UnityEngine;
using System.Collections;

public class StratMap : MonoBehaviour {

    private GameObject playerIcon;
    private Transform shipTransform;
    private RectTransform playerIconTransform;
    private float panelHeight;
    private float panelWidth;

    private Vector3 offset = new Vector3(0, 200, 0);
	// Use this for initialization
	void Start () {
        var panel = this;
        shipTransform = GameObject.Find("PlayerShip(Clone)").transform;
        playerIcon = Instantiate(Resources.Load("Prefabs/IndicatorArrow", typeof(GameObject))) as GameObject;
        playerIcon.transform.SetParent(panel.transform, false);
        playerIcon.transform.SetParent(panel.transform, false);
        playerIconTransform = (RectTransform)playerIcon.transform;
        RectTransform panelRectTransform = (RectTransform)panel.transform;
        panelHeight = panelRectTransform.sizeDelta.y;
        panelWidth = panelRectTransform.sizeDelta.x;
        print("panel height = " + panelHeight + "panel width = " + panelWidth);
    }

    public void NewOutpost(GameObject outpost)
    {
        var panel = this;
        if (panel != null)  // make sure you actually found it!
        {
            GameObject outpostSymbol = Instantiate(Resources.Load("Prefabs/OutpostIcon", typeof(GameObject))) as GameObject;
            outpostSymbol.transform.SetParent(panel.transform, false);
            RectTransform arrowRectTransform = (RectTransform)outpostSymbol.transform;
            Vector3 screenPos = new Vector3(outpost.transform.position.x/15, outpost.transform.position.z/15,0);
            arrowRectTransform.anchoredPosition = screenPos;
            if (WithinBounds(screenPos))
                outpostSymbol.SetActive(true);
            else outpostSymbol.SetActive(false);
        }
    }

    private bool WithinBounds(Vector3 screenPos)
    {
        if (screenPos.x > -200 && screenPos.x < 200 &&
            screenPos.y > -200 && screenPos.y < 200 &&
            screenPos.z > -200 && screenPos.z < 200
        ) return true;
        else return false;
    }

	// Update is called once per frame
	void Update () {
        Vector3 screenPos = new Vector3(shipTransform.position.x / 15, shipTransform.position.z / 15, 0);
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
