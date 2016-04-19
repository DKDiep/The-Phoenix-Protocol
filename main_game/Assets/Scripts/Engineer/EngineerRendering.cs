using UnityEngine;
using System.Collections;

public class EngineerRendering : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
        GameObject.Find("TargetCamera").SetActive(false);
        Camera.main.gameObject.name = "CommanderCamera";
	}
}
