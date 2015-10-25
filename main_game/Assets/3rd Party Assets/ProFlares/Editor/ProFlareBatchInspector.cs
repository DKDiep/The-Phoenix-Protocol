
/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


/// <summary>
/// ProFlareBatchInspector.cs
/// Custom inspector for the ProFlareBatch
/// </summary>

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ProFlareBatch))]
public class ProFlareBatchInspector : Editor {
	
	ProFlareBatch _ProFlareBatch;
	// Use this for initialization
	bool Updated;
	string	occluded = "Occluded";
	public override void OnInspectorGUI () {
        
		GUIStyle title = FlareEditorHelper.TitleStyle();
		
		FlareEditorHelper.DrawGuiDivider();
		
		EditorGUILayout.LabelField("Pro Flare Batch :",title);
		
		GUILayout.Space(10f);
		
		_ProFlareBatch = target as ProFlareBatch;
		//base.DrawDefaultInspector();
		_ProFlareBatch.debugMessages = EditorGUILayout.Toggle("Debug Messages",_ProFlareBatch.debugMessages);

		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.LabelField("Mode",GUILayout.MaxWidth(100));

		ProFlareBatch.Mode _mode = (ProFlareBatch.Mode)EditorGUILayout.EnumPopup(_ProFlareBatch.mode);
		
		if(_mode != _ProFlareBatch.mode){
			
			_ProFlareBatch.mode = _mode;
			
			switch(_mode){
				case(ProFlareBatch.Mode.Standard):{
					_ProFlareBatch.SingleCamera_Mode = false;
					_ProFlareBatch.VR_Mode = false;
				}break;
				case(ProFlareBatch.Mode.SingleCamera):{
					_ProFlareBatch.SingleCamera_Mode = true;
					_ProFlareBatch.VR_Mode = false;
				}break;
				case(ProFlareBatch.Mode.VR):{
					_ProFlareBatch.SingleCamera_Mode = false;
					_ProFlareBatch.VR_Mode = true;
				}break;
			}
			
			Updated = true;
			
		}
		
		
				EditorGUILayout.EndHorizontal();

		ProFlareAtlas _atlas = EditorGUILayout.ObjectField("Flare Atlas", _ProFlareBatch._atlas, typeof(ProFlareAtlas), false) as ProFlareAtlas;
		
		if(!_ProFlareBatch._atlas){
			EditorGUILayout.HelpBox("Assign a Flare Atlas.", MessageType.Error,false);
		}
		
		
		Camera  _camera = EditorGUILayout.ObjectField("Game Camera", _ProFlareBatch.GameCamera, typeof(Camera), true) as Camera;
		
		if(_camera != _ProFlareBatch.GameCamera){
			
			Updated = true;
			
			_ProFlareBatch.GameCamera = _camera;
			
			if(_ProFlareBatch.GameCamera)
				_ProFlareBatch.GameCameraTrans = _camera.transform;		
		}
		
		if(_ProFlareBatch.GameCamera == null)
			EditorGUILayout.HelpBox("Assign Game Camera.", MessageType.Warning,false);
		
		_ProFlareBatch.FlareCamera = EditorGUILayout.ObjectField("Flare Camera", _ProFlareBatch.FlareCamera, typeof(Camera), true) as Camera;
		
		Texture2D temp2D = null;
		
		if (_atlas != _ProFlareBatch._atlas)
		{   
			if(_atlas == null)
				_ProFlareBatch._atlas = null;
			else
			if(_atlas.texture != null){
				if(_ProFlareBatch.VR_Mode)
					_ProFlareBatch.name = "ProFlareBatch_VR ("+_atlas.gameObject.name+")";
				else
					_ProFlareBatch.name = "ProFlareBatch ("+_atlas.gameObject.name+")";
				
				_ProFlareBatch._atlas = _atlas;
				
				_ProFlareBatch.ForceRefresh();
				
				Updated = true;
				
				_ProFlareBatch.mat.mainTexture = _ProFlareBatch._atlas.texture;
				
				_ProFlareBatch.dirty = true;
				
				ProFlare[] flares = GameObject.FindObjectsOfType(typeof(ProFlare)) as ProFlare[];
				
				foreach(ProFlare flare in flares){
					flare.ReInitialise();
				}
				
			}else{
				Debug.LogError("ProFlares - Atlas missing texture, Atlas not assigned.");	
			}
		}
		
		if(_ProFlareBatch.mode == ProFlareBatch.Mode.VR){
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.LabelField("VR Flare Depth");
			
			_ProFlareBatch.VR_Depth = EditorGUILayout.Slider(_ProFlareBatch.VR_Depth,0f,1f);
			
			EditorGUILayout.EndHorizontal();
		}
		
		if (_ProFlareBatch._atlas)
		{ 
			if(_ProFlareBatch.mat)
			if(Application.isPlaying || (_ProFlareBatch.mat.mainTexture == null))
				if(_atlas.texture != null)
					_ProFlareBatch.mat.mainTexture = _atlas.texture;
			
			
			FlareEditorHelper.DrawGuiDivider();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Connected Flares :",title);
			if(GUILayout.Button("Force Refresh",GUILayout.MaxWidth(120))){
				
				_ProFlareBatch.ForceRefresh();
				
				ProFlare[] flares = GameObject.FindObjectsOfType(typeof(ProFlare)) as ProFlare[];
				
				foreach(ProFlare flare in flares){
					flare.ReInitialise();
				}
				
				Updated = true;
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(9f);
//			if(_ProFlareBatch.Flares.Count != ProFlareBatch.FlaresList.Count){


//			}

			if(_ProFlareBatch.FlaresList.Count < 1)
				EditorGUILayout.LabelField("No Connected flares");
			else{
				EditorGUILayout.LabelField(_ProFlareBatch.FlaresList.Count.ToString()+" Flares Connected");

				if(_ProFlareBatch.FlaresList.Count < 10)
					_ProFlareBatch.showAllConnectedFlares = EditorGUILayout.Toggle("Show All Connected Flares",_ProFlareBatch.showAllConnectedFlares);
				for(int i = 0; i < _ProFlareBatch.FlaresList.Count; i++){
					
					if(_ProFlareBatch.FlaresList[i].flare == null)
						continue;
					
					EditorGUILayout.BeginHorizontal();
					 
					if(_ProFlareBatch.FlaresList[i].occlusion == null)
						continue;
					
					if(!_ProFlareBatch.FlaresList[i].occlusion.occluded)
					 
            			EditorGUILayout.LabelField((i+1).ToString()+" - "+_ProFlareBatch.FlaresList[i].flare.gameObject.name+" - "+_ProFlareBatch.FlaresList[i].occlusion._CullingState.ToString()); 
					else
						EditorGUILayout.LabelField((i+1).ToString()+" - "+_ProFlareBatch.FlaresList[i].flare.gameObject.name+" - "+_ProFlareBatch.FlaresList[i].occlusion._CullingState.ToString()+" - "+occluded); 
						
						if(GUILayout.Button("Select",GUILayout.Width(60)))
						{
							Selection.activeGameObject = _ProFlareBatch.FlaresList[i].flare.gameObject;
						}
					
				
					EditorGUILayout.EndHorizontal();
					if(i > 10){
						
						if(!_ProFlareBatch.showAllConnectedFlares)
							break;
					}
				}
			}
			
			FlareEditorHelper.DrawGuiDivider();
			EditorGUILayout.LabelField("Settings :",title);
			GUILayout.Space(9f);
			_ProFlareBatch.zPos = EditorGUILayout.FloatField("Z Position",_ProFlareBatch.zPos);
			FlareEditorHelper.DrawGuiDivider();
			EditorGUILayout.LabelField("Optimizations :",title);
			
             EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use Flare Culling");
            _ProFlareBatch.useCulling = EditorGUILayout.Toggle(_ProFlareBatch.useCulling);
			 GUI.enabled = _ProFlareBatch.useCulling;
			EditorGUILayout.EndHorizontal();

			
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Cull Flares After Seconds ");
            	_ProFlareBatch.cullFlaresAfterTime = EditorGUILayout.IntField(_ProFlareBatch.cullFlaresAfterTime);
 			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Cull Flares when can cull # Flares ");
            	_ProFlareBatch.cullFlaresAfterCount = EditorGUILayout.IntField(_ProFlareBatch.cullFlaresAfterCount);
 			EditorGUILayout.EndHorizontal();
			 
			GUI.enabled = true;
			GUILayout.Space(8f);
			EditorGUILayout.BeginVertical("box");
             EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use Brightness Culling");
            _ProFlareBatch.useBrightnessThreshold = EditorGUILayout.Toggle(_ProFlareBatch.useBrightnessThreshold);
			EditorGUILayout.EndHorizontal();
            GUI.enabled = _ProFlareBatch.useBrightnessThreshold;
            _ProFlareBatch.BrightnessThreshold = Mathf.Clamp(EditorGUILayout.IntField("   Minimum Brightness",_ProFlareBatch.BrightnessThreshold),0,255);
            GUI.enabled = true;
             
 			EditorGUILayout.EndVertical();


			if(Application.isPlaying)
				GUI.enabled = false;
			else
				GUI.enabled = true;
			
			FlareEditorHelper.DrawGuiDivider();
			
			EditorGUILayout.LabelField("Debug :",title);
			GUILayout.Space(8f);
			
			EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Flare Count : " + _ProFlareBatch.FlaresList.Count);
            EditorGUILayout.LabelField("Flare Elements : " + _ProFlareBatch.FlareElements.Count);
			//if(_ProFlareBatch.meshFilter){
  			//	EditorGUILayout.LabelField("Triangle Count : " + (_ProFlareBatch.meshFilter.sharedMesh.triangles.Length/3).ToString());
 			//	EditorGUILayout.LabelField("Vertex Count : " + _ProFlareBatch.meshFilter.sharedMesh.vertexCount.ToString());
			//}
			EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Show Overdraw",GUILayout.MaxWidth(160));
            //_ProFlareBatch.useBrightnessThreshold = EditorGUILayout.Toggle(_ProFlareBatch.useBrightnessThreshold);
            bool overdraw = EditorGUILayout.Toggle(_ProFlareBatch.overdrawDebug);
            EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			
			
			
			if(overdraw != _ProFlareBatch.overdrawDebug){
                
				_ProFlareBatch.overdrawDebug = overdraw;
				
				if(overdraw){
					
					temp2D = new Texture2D(1, 16);
					temp2D.name = "[Generated] Debug";
					temp2D.hideFlags = HideFlags.DontSave;
					
					for (int i = 0; i < 16; ++i)
						temp2D.SetPixel(0, i, Color.white);
					
					_ProFlareBatch.mat.mainTexture = temp2D;
					
				}else{
					if(_atlas.texture != null)
						_ProFlareBatch.mat.mainTexture = _atlas.texture;
					
					if(temp2D != null)
						Destroy(temp2D);
				}
				
			}
			FlareEditorHelper.DrawGuiDivider();
			
			if (GUI.changed||Updated)
	        {
				Updated = false;
				EditorUtility.SetDirty (target); 
	        }
		}
	}
}
