
/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com



using UnityEngine;
using System.Collections;

public class SplitScreenDemo : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public Camera guiCamera;
	public Texture2D Logo;
	
	public Texture2D Info;
	
	void OnGUI(){

		GUI.color = Color.white;
		
		GUIStyle LogoStyle = new GUIStyle();
		LogoStyle.active.background = Logo;
		
		LogoStyle.normal.background = Logo;
		LogoStyle.richText = true;
		LogoStyle.alignment = TextAnchor.MiddleCenter;
		LogoStyle.normal.textColor = Color.white;
		
		if(GUI.Button(new Rect(10,0,Logo.width,Logo.height),"",LogoStyle)){
			Application.OpenURL("http://proflares.com/store");
		}
		
		GUIStyle styleInfo = new GUIStyle();
		styleInfo.active.background = Info;
		styleInfo.normal.background = Info;
		styleInfo.richText = true;
		styleInfo.alignment = TextAnchor.MiddleCenter;
		styleInfo.normal.textColor = Color.white;
		
		if(GUI.Button(new Rect((guiCamera.pixelRect.width*0.5f)-(Info.width*0.5f),guiCamera.pixelRect.height*2-Info.height,Info.width,Info.height),"",styleInfo)){
			//Application.OpenURL("http://proflares.com/store");
		}
	}
}
