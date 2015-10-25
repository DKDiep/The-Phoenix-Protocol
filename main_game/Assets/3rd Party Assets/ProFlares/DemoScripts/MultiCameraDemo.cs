/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


using UnityEngine;
using System.Collections;

public class MultiCameraDemo : MonoBehaviour {
	
	public Camera camera1;
	public Camera camera2;
	public Camera camera3;
	public Camera camera4;
	
	int count;
	
	public ProFlareBatch batch;
	void Start(){
		camera1.enabled = true;
		camera2.enabled = false;
		camera3.enabled = false;
		camera4.enabled = false;
		batch.SwitchCamera(camera1);
	}
	void Update () {
		if(Input.GetKeyUp(KeyCode.Space)){
			count++;
			if(count == 4)
				count = 0;
			
			if(count == 0){
				 
				camera1.enabled = true;
				camera2.enabled = false;
				camera3.enabled = false;
				camera4.enabled = false;
				
				batch.SwitchCamera(camera1);
			}
			if(count == 1){
				 
				camera1.enabled = false;
				camera2.enabled = true;
				camera3.enabled = false; 
				camera4.enabled = false;
				
				batch.SwitchCamera(camera2);
			}
			if(count == 2){
				 
				camera1.enabled = false;
				camera2.enabled = false;
				camera3.enabled = true; 
				camera4.enabled = false;
				
				batch.SwitchCamera(camera3);
			}
			if(count == 3){
				 
				camera1.enabled = false;
				camera2.enabled = false;
				camera3.enabled = false; 
				camera4.enabled = true;
				
				batch.SwitchCamera(camera4);
			}
		}
	}
	
	
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
		
		if(GUI.Button(new Rect((camera1.pixelRect.width*0.5f)-(Info.width*0.5f),camera1.pixelRect.height-Info.height,Info.width,Info.height),"",styleInfo)){
			//Application.OpenURL("http://proflares.com/store");
		}
	}
}
