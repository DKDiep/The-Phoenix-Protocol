using UnityEngine;
using System.Collections;

public class StratMap : MonoBehaviour {

    private GameObject playerIcon;
    private Transform shipTransform;
    private RectTransform playerIconTransform;
    private float panelHeight;
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
    }

    public void NewOutpost(GameObject outpost)
    {
        var panel = this;
        if (panel != null)  // make sure you actually found it!
        {
            GameObject outPostSymbol = Instantiate(Resources.Load("Prefabs/OutpostIcon", typeof(GameObject))) as GameObject;
            outPostSymbol.transform.SetParent(panel.transform, false);
            RectTransform arrowRectTransform = (RectTransform)outPostSymbol.transform;
            Vector3 screenPos = new Vector3(outpost.transform.position.x/6, outpost.transform.position.z/6 + panelHeight*0.3f,0);
            arrowRectTransform.anchoredPosition = screenPos;
        }
     }

	// Update is called once per frame
	void Update () {
        playerIconTransform.anchoredPosition = new Vector3(shipTransform.position.x / 6, shipTransform.position.z/6+panelHeight*0.3f,0);
        Quaternion shipRotation = shipTransform.rotation;
        Vector3 eulerRotation = shipRotation.eulerAngles;
        Quaternion newRotation = Quaternion.identity;
        newRotation.eulerAngles = new Vector3(0, 0, -eulerRotation.y);
        playerIconTransform.localRotation = newRotation;
    }
}
