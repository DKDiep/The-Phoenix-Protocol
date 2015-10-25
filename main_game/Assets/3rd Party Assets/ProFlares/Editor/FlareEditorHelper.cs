/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com

/// <summary>
/// FlareEditorHelper.cs
/// Helper class that contains a number of functions that other Editor Scripts use.
/// </summary>

using UnityEngine;
using UnityEditor;
using System.Collections;

public class FlareEditorHelper : MonoBehaviour {
    
	const int menuPos = 10000;
	
	//public static bool ProFlaresDebugMessages = false;
	
	[MenuItem ("Window/ProFlares/Create Setup",false,menuPos+0)]
	static void CreateSetup () {
		GameObject rootGO = new GameObject("ProFlareSetup");
		rootGO.layer = 8;
		GameObject cameraGO = new GameObject("ProFlareCamera");
		cameraGO.layer = 8;
		Camera camera = cameraGO.AddComponent<Camera>();
		
		cameraGO.transform.parent = rootGO.transform;
		
		camera.clearFlags = CameraClearFlags.Depth;
		
		camera.orthographic = true;
		
		camera.orthographicSize = 1;
		
		camera.farClipPlane = 2;
		
		camera.nearClipPlane = -2;
		
		camera.depth++;
		
		camera.cullingMask = 1 << 8;
		
		GameObject batchGO = new GameObject("ProFlareBatch");
		
		batchGO.layer = 8;
		
		batchGO.transform.parent = cameraGO.transform;
		
		ProFlareBatch batch = batchGO.AddComponent<ProFlareBatch>();
		
		batch.FlareCamera = camera;
		
		GameObject MainCameraGo = GameObject.FindWithTag("MainCamera");
		if(MainCameraGo){

#if UNITY_5
			batch.GameCamera = MainCameraGo.GetComponent<Camera>();
		
			batch.GameCameraTrans = MainCameraGo.transform;

#else
			batch.GameCamera = MainCameraGo.camera;
			
			batch.GameCameraTrans = MainCameraGo.transform;

#endif
		}
		Selection.activeGameObject = batchGO;
		
	}
	
	[MenuItem ("Window/ProFlares/Create Setup On Selected Camera",false,menuPos+0)]
	static void CreateSetupOnExisting () {
		
		GameObject cameraGO = null;
		Camera camera = null;
        
		if(Selection.activeGameObject){
            
			camera = Selection.activeGameObject.GetComponent<Camera>();
			if(camera == null){
				Debug.LogError("ProFlares - No Camera Selected");
				return;
			}else{
				cameraGO = Selection.activeGameObject;
			}
		}else{
			Debug.LogError("ProFlares - Nothing Selected");
			return;
		}
		
		Vector3 worldScale = cameraGO.transform.localScale;
        
        Transform parent = cameraGO.transform.parent;
		
        while (parent != null)
		{
            worldScale = Vector3.Scale(worldScale,parent.localScale);
            parent = parent.parent;
        }
		
	//	Debug.LogError(worldScale.x + " " + worldScale.y + " " + worldScale.z);
        
	//	Debug.LogError(1f/worldScale.x + " " + 1f/worldScale.y + " " + 1f/worldScale.z);
	
		int layerNumber = cameraGO.layer;
		
		GameObject batchGO = new GameObject("ProFlareBatch");
		
		batchGO.layer = layerNumber;
		
		batchGO.transform.parent = cameraGO.transform;
		
		batchGO.transform.localPosition = Vector3.zero;
		
		batchGO.transform.localScale = new Vector3(1f/worldScale.x,1f/worldScale.y,1f/worldScale.z);
		
		ProFlareBatch batch = batchGO.AddComponent<ProFlareBatch>();
		
		batch.FlareCamera = camera;
		batch.FlareCameraTrans = camera.transform;
		
		GameObject MainCameraGo = GameObject.FindWithTag("MainCamera");
		
		if(MainCameraGo){
#if UNITY_5
			batch.GameCamera = MainCameraGo.GetComponent<Camera>();
			batch.GameCameraTrans = MainCameraGo.transform;
#else
			batch.GameCamera = MainCameraGo.camera;
			batch.GameCameraTrans = MainCameraGo.transform;
#endif
		}
		
		Selection.activeGameObject = batchGO;
	}
	
	[MenuItem ("Window/ProFlares/Create Single Camera Setup On Selected Main Camera",false,menuPos+1)]
	static void CreateSetupOnExistingGameCamera () {
		
		GameObject cameraGO = null;
		Camera camera = null;
        
		if(Selection.activeGameObject){
			camera = Selection.activeGameObject.GetComponent<Camera>();
			if(camera == null){
				Debug.LogError("ProFlares - No Camera Selected");
				return;
			}else{
				cameraGO = Selection.activeGameObject;
			}
		}else{
			Debug.LogError("ProFlares - Nothing Selected");
			return;
		}
		
	//	Vector3 worldScale = cameraGO.transform.localScale;
        
       // Transform parent = cameraGO.transform.parent;
	
		int layerNumber = cameraGO.layer;
		
		GameObject batchGO = new GameObject("ProFlareBatch");
		
		batchGO.layer = layerNumber;
		
		batchGO.transform.parent = cameraGO.transform;
		
		batchGO.transform.localPosition = Vector3.zero;

		batchGO.transform.localRotation = Quaternion.identity;
		
		batchGO.transform.localScale = Vector3.one;
		
		ProFlareBatch batch = batchGO.AddComponent<ProFlareBatch>();
		
		batch.FlareCamera = camera;
		
		batch.GameCamera = camera;
		
		batch.FlareCameraTrans = camera.transform;
		
		batch.mode = ProFlareBatch.Mode.SingleCamera;
		
		batch.SingleCamera_Mode = true;
		
		batch.zPos = 1;
		/*
		GameObject MainCameraGo = GameObject.FindWithTag("MainCamera");
		
		if(MainCameraGo){
			batch.GameCamera = MainCameraGo.camera;
			batch.GameCameraTrans = MainCameraGo.transform;
		}*/
		
		Selection.activeGameObject = batchGO;
		
	}
	
	[MenuItem ("Window/ProFlares/Create Flare",false,menuPos+12)]
	static void CreateFlare () {
		
		GameObject flareGO = new GameObject("Flare");
		flareGO.layer = 8;
		flareGO.AddComponent<ProFlare>();
		Selection.activeGameObject = flareGO;
	}
	
	
 
	
	[MenuItem ("Window/ProFlares/Help",false,menuPos+71)]
	static void ProFlareHelp () {
		Application.OpenURL("http://www.proflares.com/help");
	}
	
	public static void DrawGuiInBoxDivider(){
		
		GUILayout.Space(8f);
        
		if (Event.current.type == EventType.Repaint)
		{

			int extra = 0;
			#if UNITY_4_3
			extra = 10;
			#endif

			Texture2D tex = EditorGUIUtility.whiteTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
			GUI.DrawTexture(new Rect(5f+extra, rect.yMin + 5f, Screen.width-11, 1f), tex);
			GUI.color = Color.white;
		}
	}
	
	
	public static void DrawGuiDivider(){
		
		GUILayout.Space(12f);
        
		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = EditorGUIUtility.whiteTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}
	
	public static GUIStyle TitleStyle(){
		GUIStyle title = new GUIStyle(EditorStyles.largeLabel);
		
		title.fontSize = 16;
		
		title.clipping = TextClipping.Overflow;
		
		return title;
	}
	
	public static GUIStyle ThinButtonStyle(){
		GUIStyle thinButton = new GUIStyle(EditorStyles.toolbarButton);
		thinButton.fontStyle = FontStyle.Bold;
		thinButton.fixedHeight = 24f;
		return thinButton;
	}
	
	public static GUIStyle ThinButtonRedStyle(){
		GUIStyle thinButtonRed = new GUIStyle(EditorStyles.toolbarButton);
		thinButtonRed.fontStyle = FontStyle.Bold;
		thinButtonRed.fixedHeight = 24f;
		thinButtonRed.normal.textColor = Color.red;
		return thinButtonRed;
	}
	
	public static GUIStyle ThinButtonPressedStyle(){
		GUIStyle thinButtonPressed = new GUIStyle(EditorStyles.toolbarButton);
		thinButtonPressed.fontStyle = FontStyle.Bold;
		thinButtonPressed.fixedHeight = 24f;
		return thinButtonPressed;
	}
	
	public static GUIStyle DropDownButtonStyle(){
		GUIStyle dropDownButton = new GUIStyle(EditorStyles.toolbarDropDown);
		dropDownButton.fontStyle = FontStyle.Bold;
		dropDownButton.fixedHeight = 20f;
		return dropDownButton;
	}
	
	public static GUIStyle EnumStyleButton(){
		GUIStyle enumStyleButton = new GUIStyle(EditorStyles.toolbarDropDown);
		enumStyleButton.onActive.background = ThinButtonStyle().onActive.background;
		enumStyleButton.fixedHeight = 24f;
		return enumStyleButton;
	}
	
	public static GUIStyle FoldOutButtonStyle(){
		GUIStyle foldOutButton = new GUIStyle(EditorStyles.foldout);
		foldOutButton.fontStyle = FontStyle.Bold;
		return foldOutButton;
	}
    
}