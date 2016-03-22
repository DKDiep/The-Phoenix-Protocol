/*
    Disables rendering of 3D objects on the command console except those on the UI layer
*/

using UnityEngine;
using System.Collections;

public class CommanderRendering : MonoBehaviour {

	void Start () 
    {
        Camera.main.cullingMask = 1 << LayerMask.NameToLayer("UI");
        GameObject.Find("TargetCamera").SetActive(false);
    }
}
