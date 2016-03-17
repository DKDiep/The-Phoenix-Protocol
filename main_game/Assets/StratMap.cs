using UnityEngine;
using System.Collections;

public class StratMap : MonoBehaviour {

    private GameObject playerIcon;
    private Transform shipTransform;
    private RectTransform playerIconTransform;
	// Use this for initialization
	void Start () {
        var panel = this;
        shipTransform = GameObject.Find("PlayerShip(Clone)").transform;
        playerIcon = Instantiate(Resources.Load("Prefabs/IndicatorArrow", typeof(GameObject))) as GameObject;
                    playerIcon.transform.SetParent(panel.transform, false);
        playerIcon.transform.SetParent(panel.transform, false);
        playerIconTransform = (RectTransform)playerIcon.transform;
    }

    public void NewOutpost(GameObject outpost)
    {
        var panel = this;
        if (panel != null)  // make sure you actually found it!
        {
            GameObject outPostSymbol = Instantiate(Resources.Load("Prefabs/IndicatorArrow", typeof(GameObject))) as GameObject;
            outPostSymbol.transform.SetParent(panel.transform, false);
            RectTransform arrowRectTransform = (RectTransform)outPostSymbol.transform;
            Vector3 screenPos = new Vector3(0, 0, 0);
            arrowRectTransform.anchoredPosition = screenPos;
        }
        print("new outpost location: (" + outpost.transform.position.x + ", " + 
            outpost.transform.position.y + ", " + outpost.transform.position.z + ")");
    }

	// Update is called once per frame
	void Update () {
        playerIconTransform.anchoredPosition = new Vector3(shipTransform.position.x / 3, shipTransform.position.z / 3,0);
        //Quaternion shipRotation = shipTransform.rotation;
        //shipRotation.SetLookRotation(Vector3.up, new Vector3(1, 0, 0));
        //playerIconTransform.localRotation = shipRotation;
    }
}
