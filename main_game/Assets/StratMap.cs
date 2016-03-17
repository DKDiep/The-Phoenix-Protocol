using UnityEngine;
using System.Collections;

public class StratMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var panel = this;
        if (panel != null)  // make sure you actually found it!
        {
            GameObject outPostSymbol = Instantiate(Resources.Load("Prefabs/IndicatorArrow", typeof(GameObject))) as GameObject;
            outPostSymbol.transform.SetParent(panel.transform, false);
            RectTransform arrowRectTransform = (RectTransform)outPostSymbol.transform;
            Vector3 screenPos = new Vector3(0, 0,0);
            arrowRectTransform.anchoredPosition = screenPos;
        }
    }
	
	// Update is called once per frame
	void Update () {

	
	}
}
