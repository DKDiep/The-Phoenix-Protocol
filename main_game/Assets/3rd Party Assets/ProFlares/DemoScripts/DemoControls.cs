/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


using UnityEngine;
using System.Collections;

public class DemoControls : MonoBehaviour {
	
	public GameObject Setup1;
	
	public GameObject Setup1Extra;
	public Color Setup1Ambient;
	
	public GameObject Setup2;
	
	public GameObject Setup2Extra;
	public Color Setup2Ambient;
	public GameObject Setup3;
 	
	public GameObject Setup3Extra;
	public Color Setup3Ambient; 
	public bool Toggle;
	
	
	
	// Use this for initialization
	void Start () {
		 Swap(1);
	}
	 
	 
	void Swap(int number){
		switch(number){
		
			case(1):{
				Setup1.SetActive(true);
				Setup2.SetActive(false);
				if(Setup3)Setup3.SetActive(false);
			
				if(Setup1Extra) Setup1Extra.SetActive(true);
				if(Setup2Extra) Setup2Extra.SetActive(false);
				if(Setup3Extra) Setup3Extra.SetActive(false);
			
				RenderSettings.ambientLight = Setup1Ambient;
			}break;
		
			case(2):{
				Setup1.SetActive(false);
				Setup2.SetActive(true);
				if(Setup3)Setup3.SetActive(false);
			
				if(Setup1Extra) Setup1Extra.SetActive(false);
				if(Setup2Extra) Setup2Extra.SetActive(true);
				if(Setup3Extra) Setup3Extra.SetActive(false);
			
				RenderSettings.ambientLight = Setup2Ambient;
				
			}break;
		
			case(3):{
				Setup1.SetActive(false);
				Setup2.SetActive(false);
				if(Setup3)Setup3.SetActive(true);
			
				if(Setup1Extra) Setup1Extra.SetActive(false);
				if(Setup2Extra) Setup2Extra.SetActive(false);
				if(Setup3Extra) Setup3Extra.SetActive(true);
			
				RenderSettings.ambientLight = Setup3Ambient;
				
			}break;
		}
	}
	
	public bool hideGUI;
	
	public ProFlareBatch batchLeft;
	public ProFlareBatch batchRight;
	
	void Update(){
	
		if(Input.GetKeyUp("1")){
			Swap(1);	
		} 
		if(Input.GetKeyUp("2")){
			Swap(2);	
		} 
		 
		
		if(batchLeft&&batchRight){
 			if(Input.GetKeyUp(KeyCode.M)){
				batchLeft.VR_Depth =	batchLeft.VR_Depth+0.05f;
				batchLeft.VR_Depth = Mathf.Clamp01(batchLeft.VR_Depth);
				batchRight.VR_Depth = batchLeft.VR_Depth;
			} 
			
			if(Input.GetKeyUp(KeyCode.N)){
				batchLeft.VR_Depth =	batchLeft.VR_Depth-0.05f;
				batchLeft.VR_Depth = Mathf.Clamp01(batchLeft.VR_Depth);
				batchRight.VR_Depth = batchLeft.VR_Depth;
			}
 		}
		
	}
	void OnGUI(){
	
		if(hideGUI)
			return;
#if UNITY_5		 

		Rect texRect = new Rect(0,GetComponent<Camera>().pixelRect.height-120,GetComponent<Camera>().pixelRect.width,120);
		
		GUI.color = Color.white;
		
		GUI.DrawTexture(texRect, Black);
		
		GUIStyle LogoStyle = new GUIStyle();
		LogoStyle.active.background = Logo;
		
		LogoStyle.normal.background = Logo;
		LogoStyle.richText = true;
		LogoStyle.alignment = TextAnchor.MiddleCenter;
		LogoStyle.normal.textColor = Color.white;
		
		GUIStyle style = new GUIStyle();
		style.active.background = Button1;
		style.normal.background = Button1;
		style.richText = true;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.white;
		
		GUIStyle style2 = new GUIStyle();
		style2.active.background = Button2;
		style2.normal.background = Button2;
		style2.richText = true;
		style2.alignment = TextAnchor.MiddleCenter;
		style2.normal.textColor = Color.white;
		
		GUIStyle style3 = new GUIStyle();
		style3.active.background = Button3;
		style3.normal.background = Button3;
		style3.richText = true;
		style3.alignment = TextAnchor.MiddleCenter;
		style3.normal.textColor = Color.white;
		
		GUIStyle style4 = new GUIStyle();
		style4.active.background = INFO;
		style4.normal.background = INFO;
		style4.richText = true;
		style4.alignment = TextAnchor.MiddleCenter;
		style4.normal.textColor = Color.white;
		
		if(GUI.Button(new Rect(20,GetComponent<Camera>().pixelRect.height-Button1.height,Button1.width,Button1.height),"",LogoStyle)){
			Application.OpenURL("http://proflares.com/store");
		}
		
		if(GUI.Button(new Rect(20+Button1.width,GetComponent<Camera>().pixelRect.height-Button1.height,Button1.width,Button1.height),"",style)){
			Swap(1);
		}
		
		if(GUI.Button(new Rect(0+Button1.width*2,GetComponent<Camera>().pixelRect.height-Button1.height,Button1.width,Button1.height),"",style2)){
			Swap(2);
		}
		
		if(GUI.Button(new Rect(0+Button1.width*3,GetComponent<Camera>().pixelRect.height-Button1.height,Button1.width,Button1.height),"",style3)){
			Swap(3);
		}
		
		if(GUI.Button(new Rect(0+Button1.width*4,GetComponent<Camera>().pixelRect.height-Button1.height,Button1.width,Button1.height),"",style4)){
		}

#else		 
		
		Rect texRect = new Rect(0,camera.pixelRect.height-120,camera.pixelRect.width,120);
		
		GUI.color = Color.white;
		
		GUI.DrawTexture(texRect, Black);
		
		GUIStyle LogoStyle = new GUIStyle();
		LogoStyle.active.background = Logo;
		
		LogoStyle.normal.background = Logo;
		LogoStyle.richText = true;
		LogoStyle.alignment = TextAnchor.MiddleCenter;
		LogoStyle.normal.textColor = Color.white;
		
		GUIStyle style = new GUIStyle();
		style.active.background = Button1;
		style.normal.background = Button1;
		style.richText = true;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.white;
		
		GUIStyle style2 = new GUIStyle();
		style2.active.background = Button2;
		style2.normal.background = Button2;
		style2.richText = true;
		style2.alignment = TextAnchor.MiddleCenter;
		style2.normal.textColor = Color.white;
		
		GUIStyle style3 = new GUIStyle();
		style3.active.background = Button3;
		style3.normal.background = Button3;
		style3.richText = true;
		style3.alignment = TextAnchor.MiddleCenter;
		style3.normal.textColor = Color.white;
		
		GUIStyle style4 = new GUIStyle();
		style4.active.background = INFO;
		style4.normal.background = INFO;
		style4.richText = true;
		style4.alignment = TextAnchor.MiddleCenter;
		style4.normal.textColor = Color.white;
		
		if(GUI.Button(new Rect(20,camera.pixelRect.height-Button1.height,Button1.width,Button1.height),"",LogoStyle)){
			Application.OpenURL("http://proflares.com/store");
		}
		
		if(GUI.Button(new Rect(20+Button1.width,camera.pixelRect.height-Button1.height,Button1.width,Button1.height),"",style)){
			Swap(1);
		}
		
		if(GUI.Button(new Rect(0+Button1.width*2,camera.pixelRect.height-Button1.height,Button1.width,Button1.height),"",style2)){
			Swap(2);
		}
		
		if(GUI.Button(new Rect(0+Button1.width*3,camera.pixelRect.height-Button1.height,Button1.width,Button1.height),"",style3)){
			Swap(3);
		}
		
		if(GUI.Button(new Rect(0+Button1.width*4,camera.pixelRect.height-Button1.height,Button1.width,Button1.height),"",style4)){
		}	

#endif

	}
	
	public Texture2D Black;
	public Texture2D Logo;
	public Texture2D Button1;
	public Texture2D Button2;
	public Texture2D Button3;
	public Texture2D INFO;
	
	void drawTexture(float x, float y, Texture2D texture) {
		if(texture != null){
			Rect texRect = new Rect(x,y,texture.width,texture.height);
			GUI.color = Color.white;
			GUI.DrawTexture(texRect, texture);
		}
	}
}
