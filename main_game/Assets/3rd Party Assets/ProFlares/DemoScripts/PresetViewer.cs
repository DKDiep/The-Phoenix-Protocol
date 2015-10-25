/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


using UnityEngine;
using System.Collections;

public class PresetViewer : MonoBehaviour {
	
	public Transform PresetParent;
	
	public Camera MainCamera;
	
	public Texture2D Logo;
	
	public Texture2D Info;
	
	public Texture2D Black;
	
	int currentFlare = 0;
	public GameObject[] Flares;
	
	
	void Start () {
		//return;
		
		for(int i = 0;  i < Flares.Length;i++){
				Flares[i].SetActive(false);
			}
		Flares[currentFlare].SetActive(true);	
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow)){
			for(int i = 0;  i < Flares.Length;i++){
				Flares[i].SetActive(false);
			}
			
			if(Input.GetKeyUp(KeyCode.LeftArrow))
				currentFlare--;
			else
				currentFlare++;
			
			if(currentFlare < 0) currentFlare = Flares.Length-1;
			if(currentFlare > Flares.Length-1) currentFlare = 0;
			
			Flares[currentFlare].SetActive(true);
		}
		
		//if(!hideGui)
		if(Input.GetMouseButton(0)){
			float extra = 1.2f;
			
			Ray ray = MainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
       		Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit)){
				PresetParent.position = hit.point*extra;
			}
		}
	}
	public bool hideGui = false;
	void OnGUI(){
		
		if(hideGui)
			return;
		
		GUIStyle LogoStyle = new GUIStyle();
		LogoStyle.active.background = Logo;		
		LogoStyle.normal.background = Logo;
		LogoStyle.richText = true;
		LogoStyle.alignment = TextAnchor.MiddleCenter;
		LogoStyle.normal.textColor = Color.white;

		
		GUIStyle styleInfo = new GUIStyle();
		styleInfo.active.background = Info;
		styleInfo.normal.background = Info;
		styleInfo.richText = true;
		styleInfo.alignment = TextAnchor.MiddleCenter;
		styleInfo.normal.textColor = Color.white;
		
		if(GUI.Button(new Rect(10,0,Logo.width,Logo.height),"",LogoStyle)){
			Application.OpenURL("http://proflares.com/store");
		}
		
		if(GUI.Button(new Rect((MainCamera.pixelRect.width*0.5f)-(Info.width*0.5f),MainCamera.pixelRect.height-Info.height,Info.width,Info.height),"",styleInfo)){}
		
	}
	
	void drawTexture(float x, float y, Texture2D texture) {
		if(texture != null){
			Rect texRect = new Rect(x,y,texture.width,texture.height);
			GUI.color = Color.white;
			GUI.DrawTexture(texRect, texture);
		}
	}
}
