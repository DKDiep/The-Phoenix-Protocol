
/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


/// <summary>
/// ProFlareInspector.cs
/// Custom inspector for the ProFlare
/// </summary> 

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(ProFlare))]
public class ProFlareInspector : Editor {
	
	ProFlare _flare;
	
	GUIStyle title;
	
	GUIStyle thinButton;
	
	GUIStyle thinButtonRed;
	
	GUIStyle dropDownButton;
	
	GUIStyle enumStyleButton;
	
	private static Texture2D visibility_On;
	
	private static Texture2D visibility_Off;
	
	bool listeningForGuiChanges;
	
	bool guiChanged = false;
	
	int selectionCount = 0;
	
	private void CheckUndo()
    {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5  || UNITY_4_6  || UNITY_5_0 

		Event e = Event.current;
		
		if ( e.type == EventType.MouseDown && e.button == 0 || e.type == EventType.KeyUp && ( e.keyCode == KeyCode.Tab ) ) {
			//Debug.Log("record1");
			Undo.RecordObject(target,"ProFlare Undo");

			listeningForGuiChanges = true;
			guiChanged = false;
		}
		
		if ( listeningForGuiChanges && guiChanged ) {
			//Debug.Log("record2");
 
			Undo.RecordObject(target,"ProFlare Undo");
			listeningForGuiChanges = false;
			//TODO - Undo Not Refreshing Flare Batches.
			//for(int i = 0; i < _flare.FlareBatches.Length; i++){}
		}
#else
        Event e = Event.current;
        
        if ( e.type == EventType.MouseDown && e.button == 0 || e.type == EventType.KeyUp && ( e.keyCode == KeyCode.Tab ) ) {
            Undo.SetSnapshotTarget( _flare, "ProFlare Undo" );
            Undo.CreateSnapshot();
            Undo.ClearSnapshotTarget();
            listeningForGuiChanges = true;
            guiChanged = false;
        }
        
        if ( listeningForGuiChanges && guiChanged ) {
            Undo.SetSnapshotTarget( _flare, "ProFlare Undo" );
            Undo.RegisterSnapshot();
            Undo.ClearSnapshotTarget();
            listeningForGuiChanges = false;
			//TODO - Undo Not Refreshing Flare Batches.
			//for(int i = 0; i < _flare.FlareBatches.Length; i++){}
		}
#endif
    }
	
	public List<ProFlare> Flares = new List<ProFlare>();
	
	public override void OnInspectorGUI () {
		
		selectionCount = 0;
		
		Flares.Clear();
		
		foreach(GameObject go in Selection.gameObjects){
		
			ProFlare selectedFlare = go.GetComponent<ProFlare>();
			if(selectedFlare){
				Flares.Add(selectedFlare);
				selectionCount++;
			}
		}
		
		if(selectionCount > 1){
			EditorGUILayout.HelpBox("Multiple Flares selected.", MessageType.Warning,false);
			//EditorGUI.showMixedValue = true;
		}
		
		_flare = target as ProFlare;
		
        CheckUndo();

#if UNITY_4_3
		EditorGUI.BeginChangeCheck();
#endif
		
		FlareEditorHelper.DrawGuiDivider();
		
		bool error = false;
		
		title =  FlareEditorHelper.TitleStyle();
		thinButton = FlareEditorHelper.ThinButtonStyle();
		enumStyleButton = FlareEditorHelper.EnumStyleButton();
		thinButtonRed = FlareEditorHelper.ThinButtonRedStyle();
				
		EditorGUILayout.LabelField("Flare Setup :",title);
		
		GUILayout.Space(10f);
		
		dropDownButton = FlareEditorHelper.DropDownButtonStyle();
		
	 
		if(selectionCount <= 1)
		{
		EditorGUILayout.BeginHorizontal();
		if((!_flare.EditingAtlas)&&(_flare._Atlas != null))
			GUI.enabled = false;
		
		ProFlareAtlas _Atlas = EditorGUILayout.ObjectField("Flare Atlas", _flare._Atlas, typeof(ProFlareAtlas), false) as ProFlareAtlas;
		
		GUI.enabled = true;
		
		if(_flare._Atlas)
			if(GUILayout.Button("EDIT",GUILayout.Width(60))){
				_flare.EditingAtlas = 	_flare.EditingAtlas ? false : true;		
			}
	 
		EditorGUILayout.EndHorizontal();
		
		if((_flare.EditingAtlas)&&(_flare._Atlas != null)){
			
			EditorGUILayout.HelpBox("Changing atlas can cause data loss if elements do NOT exist in the new atlas.", MessageType.Warning,false);
		}
		
		GUI.enabled = true;
		
		if(_flare._Atlas != _Atlas){
            
			_flare._Atlas = _Atlas;
            
			
			ProFlareBatch[] flareBatchs = GameObject.FindObjectsOfType(typeof(ProFlareBatch)) as ProFlareBatch[];
			
			int matchCount = 0;
			foreach(ProFlareBatch flareBatch in flareBatchs){
				if(flareBatch._atlas == _Atlas){
					matchCount++;
				}
			}
			_flare.FlareBatches = new ProFlareBatch[matchCount];
			int count = 0;
			foreach(ProFlareBatch flareBatch in flareBatchs){
				if(flareBatch._atlas == _Atlas){
					_flare.FlareBatches[count] = flareBatch;
					_flare.FlareBatches[count].dirty = true;
					count++;
				}
			}
			if(count != 0){
				if(_flare.Elements.Count == 0){
						ProFlareElement element = new ProFlareElement();
						element.flare = _flare;
						element.SpriteName = _flare._Atlas.elementsList[0].name;
						element.flareAtlas = _flare._Atlas;
						element.position = -1;
						element.Scale = 1;
						_flare.Elements.Add(element);
				}
			}
		}
		
		 
		bool missing = false;
		for(int i = 0; i < _flare.FlareBatches.Length; i++){
		
			if(_flare.FlareBatches[i] == null)
				missing = true;
		}
		if(missing){
		
			ProFlareBatch[] flareBatchs = GameObject.FindObjectsOfType(typeof(ProFlareBatch)) as ProFlareBatch[];
			
			int matchCount = 0;
			foreach(ProFlareBatch flareBatch in flareBatchs){
				if(flareBatch._atlas == _Atlas){
					matchCount++;
				}
			}
			_flare.FlareBatches = new ProFlareBatch[matchCount];
			int count = 0;
			foreach(ProFlareBatch flareBatch in flareBatchs){
				if(flareBatch._atlas == _Atlas){
					_flare.FlareBatches[count] = flareBatch;
					_flare.FlareBatches[count].dirty = true;
					count++;
				}
			}
		}
		
		EditorGUILayout.LabelField("Rendered by : ");
		if(_flare.FlareBatches.Length != 0)
			for(int i = 0; i < _flare.FlareBatches.Length; i++){
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("   "+_flare.FlareBatches[i].gameObject.name); 
			if(GUILayout.Button("Select",GUILayout.Width(60))){ 
					Selection.activeGameObject = _flare.FlareBatches[i].gameObject;
			}
			EditorGUILayout.EndHorizontal();
			}
		
		if(error){
			EditorGUILayout.HelpBox("Fix Errors before continuing.", MessageType.Error);
		}
		}
		FlareEditorHelper.DrawGuiDivider();
		
		EditorGUILayout.LabelField("Global Settings :",title);
		GUILayout.Space(10f);
		
		Rect  r  = EditorGUILayout.BeginVertical("box");
		{
			Rect r2 = r;
			r2.height = 20;
            
			if (GUI.Button(r2, GUIContent.none,dropDownButton))
				_flare.EditGlobals = _flare.EditGlobals ? false : true;
			
		 	GUILayout.Label("General");
            
            if(_flare.EditGlobals){
				
				GUILayout.Space(5f);
				
				//Scale
				float _flareGlobalScale = EditorGUILayout.Slider("Scale",_flare.GlobalScale,0f,2000f);
				
				if(_flareGlobalScale != _flare.GlobalScale){
					
					_flare.GlobalScale = _flareGlobalScale;
					
					foreach(ProFlare flare in Flares){
						flare.GlobalScale = _flare.GlobalScale;
					}
				}
				
				_flare.MultiplyScaleByTransformScale = EditorGUILayout.Toggle("Multiply Scale By Transform Scale",_flare.MultiplyScaleByTransformScale);

				float _flareGlobalBrightness = EditorGUILayout.Slider("Brightness",_flare.GlobalBrightness,0f,1f);
                
				if(_flareGlobalBrightness != _flare.GlobalBrightness){
					_flare.GlobalBrightness = _flareGlobalBrightness;
					_flare.GlobalTintColor.a = _flare.GlobalBrightness;
					
					foreach(ProFlare flare in Flares){
						flare.GlobalBrightness = _flareGlobalBrightness;
						flare.GlobalTintColor.a = _flare.GlobalBrightness;
					}
				}
				
				Color _flareGlobalTintColor = EditorGUILayout.ColorField("Tint",_flare.GlobalTintColor);
				
				if(_flareGlobalTintColor != _flare.GlobalTintColor){
					_flare.GlobalTintColor = _flareGlobalTintColor;
					_flare.GlobalBrightness = _flare.GlobalTintColor.a;
					
					foreach(ProFlare flare in Flares){
						flare.GlobalTintColor = _flareGlobalTintColor;
						flare.GlobalBrightness = _flare.GlobalTintColor.a;
					}
				}
				
				FlareEditorHelper.DrawGuiInBoxDivider();
                
				//Angle Falloff Options
				if(selectionCount <= 1){
				GUILayout.BeginHorizontal();
				{
					_flare.UseAngleLimit = EditorGUILayout.Toggle("Use Angle Falloff",_flare.UseAngleLimit);
					GUI.enabled = _flare.UseAngleLimit;
					_flare.maxAngle =  Mathf.Clamp(EditorGUILayout.FloatField("Max Angle",_flare.maxAngle),0,360);
				}
				GUILayout.EndHorizontal();
				
				_flare.UseAngleScale = EditorGUILayout.Toggle("  Affect Scale",_flare.UseAngleScale);
				
				_flare.UseAngleBrightness = EditorGUILayout.Toggle("  Affect Brightness",_flare.UseAngleBrightness);
				
				GUILayout.BeginHorizontal();
				{
					_flare.UseAngleCurve = EditorGUILayout.Toggle("  Use Curve",_flare.UseAngleCurve);
					GUI.enabled = _flare.UseAngleCurve;
					_flare.AngleCurve = EditorGUILayout.CurveField(_flare.AngleCurve);
					if(GUILayout.Button("Reset",GUILayout.MaxWidth(50))){ _flare.AngleCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1.0f));}
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();
				GUI.enabled = true;
                
				FlareEditorHelper.DrawGuiInBoxDivider();
				//Max Distance Options -
				GUILayout.BeginHorizontal();
				{
					_flare.useMaxDistance = EditorGUILayout.Toggle("Use Distance Falloff",_flare.useMaxDistance);
					GUI.enabled = _flare.useMaxDistance;
					_flare.GlobalMaxDistance = EditorGUILayout.FloatField("  Max Distance",_flare.GlobalMaxDistance);
				}
				GUILayout.EndHorizontal();
				_flare.useDistanceScale = EditorGUILayout.Toggle("  Affect Scale",_flare.useDistanceScale);
				_flare.useDistanceFade = EditorGUILayout.Toggle("  Affect Brightness",_flare.useDistanceFade);
                
				
                GUI.enabled = true;
				FlareEditorHelper.DrawGuiInBoxDivider();
				_flare.OffScreenFadeDist = EditorGUILayout.FloatField("Off Screen Fade Distance",_flare.OffScreenFadeDist);
									FlareEditorHelper.DrawGuiInBoxDivider();

				 _flare.neverCull = EditorGUILayout.Toggle("Never Cull Flare",_flare.neverCull);
				}
			}
		}
		
		EditorGUILayout.EndVertical();
		
		
		if(selectionCount <= 1)
		{
			r  = EditorGUILayout.BeginVertical("box");
			Rect r2 = r;
			r2.height = 20;
            
            
			if (GUI.Button(r2, GUIContent.none,dropDownButton))
				_flare.EditDynamicTriggering = _flare.EditDynamicTriggering ? false : true;
			
            
		 	GUILayout.Label("Dynamics Triggering");
			
			if(_flare.EditDynamicTriggering){GUILayout.Space(5f);
				
				_flare.useDynamicEdgeBoost = EditorGUILayout.Toggle("Use Dynamic Edge Boost",_flare.useDynamicEdgeBoost);
				GUI.enabled = _flare.useDynamicEdgeBoost;
				_flare.DynamicEdgeBoost = EditorGUILayout.FloatField("  Dynamic Edge Scale",_flare.DynamicEdgeBoost);
				_flare.DynamicEdgeBrightness = EditorGUILayout.FloatField("  Dynamic Edge Brightness",_flare.DynamicEdgeBrightness);
				_flare.DynamicEdgeRange = EditorGUILayout.FloatField("  Dynamic Edge Range",_flare.DynamicEdgeRange);
				_flare.DynamicEdgeBias = EditorGUILayout.FloatField("  Dynamic Edge Bias",_flare.DynamicEdgeBias);
				
				GUILayout.BeginHorizontal();
				{
                    _flare.DynamicEdgeCurve = EditorGUILayout.CurveField("  Dynamic Edge Curve", _flare.DynamicEdgeCurve);
                    if(GUILayout.Button("Reset",GUILayout.MaxWidth(50))) _flare.DynamicEdgeCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1), new Keyframe(1, 0f));
				}
				GUILayout.EndHorizontal();
				GUI.enabled = true;
				FlareEditorHelper.DrawGuiInBoxDivider();
				
				_flare.useDynamicCenterBoost = EditorGUILayout.Toggle("Use Dynamic Center Boost",_flare.useDynamicCenterBoost);
				
				GUI.enabled = _flare.useDynamicCenterBoost;
				_flare.DynamicCenterRange = EditorGUILayout.FloatField("  Dynamic Center Range",_flare.DynamicCenterRange);
				_flare.DynamicCenterBoost = EditorGUILayout.FloatField("  Dynamic Center Scale",_flare.DynamicCenterBoost);
				_flare.DynamicCenterBrightness = EditorGUILayout.FloatField("  Dynamic Center Brightness",_flare.DynamicCenterBrightness);
				
				GUI.enabled = true;
			}
			
			EditorGUILayout.EndVertical();
		}
		
		
		if(selectionCount <= 1)
		{
			r  = EditorGUILayout.BeginVertical("box");
			Rect r2 = r;
			r2.height = 20;
			
			
			if (GUI.Button(r2, GUIContent.none,dropDownButton))
				_flare.EditOcclusion = _flare.EditOcclusion ? false : true;
			
			
			GUILayout.Label("Occlusion");
			
			if(_flare.EditOcclusion){GUILayout.Space(5f);
				_flare.RaycastPhysics = EditorGUILayout.Toggle("Raycast Against Physics",_flare.RaycastPhysics);
				GUI.enabled = _flare.RaycastPhysics;
				_flare.mask = LayerMaskField(_flare.mask);
				
				FlareEditorHelper.DrawGuiInBoxDivider();
				EditorGUILayout.LabelField("Debug Info :");
				EditorGUILayout.Toggle("  Flare Occluded",_flare.Occluded);
				EditorGUILayout.ObjectField("  Occluding GameObject", _flare.OccludingObject, typeof(GameObject), true);
				GUI.enabled = true;
			}
			EditorGUILayout.EndVertical();
		}
		
		if(selectionCount <= 1)
		{
			r  = EditorGUILayout.BeginVertical("box");
			Rect r2 = r;
			r2.height = 20;
			
			
			if (GUI.Button(r2, GUIContent.none,dropDownButton))
				_flare.IoSetting = _flare.IoSetting ? false : true;
			
			
			GUILayout.Label("Import/Export");
			
			if(_flare.IoSetting){GUILayout.Space(5f);

				if(_flare._Atlas != null){
					TextAsset ta = EditorGUILayout.ObjectField("Flare JSON Import, Drag/Drop Here", null, typeof(TextAsset), false) as TextAsset;
					
					if (ta != null)
					{
						FlareJson.LoadFlareData(_flare, ta);
						Updated = true;
					}
					
					if(GUILayout.Button("Export Flare Json Data")){
						ProFlareExporter.ExportFlare(_flare);
					}
				}else{

					GUILayout.Label("Connect Flare Atlast Before Import Or Exporting Flare.");


				}
			}
			EditorGUILayout.EndVertical();
		}
		
		
		
		FlareEditorHelper.DrawGuiDivider();
		
		EditorGUILayout.LabelField("Element Settings :",title);
		GUILayout.Space(10f);
		
		if(selectionCount > 1){
			EditorGUILayout.HelpBox("Editing flare elements is not supported in while multiple flares selected.", MessageType.Warning,false);
		}else{
		 	if(_flare._Atlas != null)
	            for(int i = 0; i < _flare.Elements.Count;i++)
	                ElementEditor(_flare.Elements[i],(i+1));
			
			
			if(_flare._Atlas != null)
			if(GUILayout.Button("ADD NEW",thinButton)){
				ProFlareElement element = new ProFlareElement();
				
				element.flare = _flare;
				element.SpriteName = _flare._Atlas.elementsList[0].name;
				element.flareAtlas = _flare._Atlas;
				element.position = -1;
				element.Scale = 1;
				
				for(int i = 0; i < _flare.FlareBatches.Length; i++){
					_flare.FlareBatches[i].dirty = true;
				}
				//_flare._FlareBatch.dirty = true;
				_flare.Elements.Add(element);
			}
		}
		FlareEditorHelper.DrawGuiDivider();
        
		if (GUI.changed||Updated)
        {
			//Debug.Log("dirty:");
			Updated = false;
			guiChanged = true;
            EditorUtility.SetDirty (target);
			
			if(selectionCount > 1)
				foreach(GameObject go in Selection.gameObjects){
		
					ProFlare selectedFlare = go.GetComponent<ProFlare>();
					if(selectedFlare)
						EditorUtility.SetDirty(selectedFlare);
				}
        }
	}
	
	public bool Updated = false;
    
	
	void ElementEditor(ProFlareElement element,int count){
		
		ProFlareElement.Type elementType;
		
		EditorGUILayout.BeginHorizontal();
		{
            //
			if(GUILayout.Button("      ",thinButton,GUILayout.MaxWidth(35))){
                
			}
			if(GUILayout.Button(" ",thinButton,GUILayout.MaxWidth(32)))
				element.Visible = element.Visible ? false : true;
			
			
			Rect rect2 = GUILayoutUtility.GetLastRect();
			
			int extra = 0;
#if UNITY_4_3
			extra = 15;
#endif
			
			Rect outerRect = new Rect(35+extra,rect2.yMin-4,32,32);
		 	Rect final = new Rect(0,0,1,1);
			
			
			if( visibility_On == null) {
				visibility_On = Resources.Load("visibility-On") as Texture2D;
			}
			if( visibility_Off == null) {
				visibility_Off = Resources.Load("visibility-Off") as Texture2D;
			}
			
			if(element.Visible)
				GUI.DrawTextureWithTexCoords(outerRect,visibility_On, final, true);
			else
				GUI.DrawTextureWithTexCoords(outerRect,visibility_Off, final, true);
			
			_flare._Atlas.UpdateElementNameList();
			
			int id = EditorGUILayout.Popup(element.elementTextureID, _flare._Atlas.elementNameList,enumStyleButton,GUILayout.MaxWidth(200));
			
			
			if(element.elementTextureID != id){
				
				Debug.Log("Changing Sprite " + element.flareAtlas.elementsList[id].name);
				
				element.elementTextureID = id;
				element.SpriteName = element.flareAtlas.elementsList[id].name;
 
				for(int f = 0; f < element.flare.FlareBatches.Length; f++){ 
						element.flare.FlareBatches[f].dirty = true;
				}
			}
			
			
			elementType = (ProFlareElement.Type)EditorGUILayout.EnumPopup(element.type,enumStyleButton,GUILayout.MaxWidth(100));
            
			if(GUILayout.Button("EDIT",thinButton,GUILayout.MaxWidth(80))){
				if(element.Editing)
					element.Editing = false;
				else{
					
                    element.ElementSetting = false;
                    element.OffsetSetting = false;
                    element.ColorSetting = false;
                    element.ScaleSetting = false;
                    element.RotationSetting = false;
					element.OverrideSetting = false;
					
					element.Editing = true;
				}
			}
			
			if(GUILayout.Button("CLONE",thinButton)){
				element.Editing = false;
				_flare.Elements.Add(CloneElement(element));
				//_flare._FlareBatch.dirty = true;
				for(int i = 0; i < _flare.FlareBatches.Length; i++){
					_flare.FlareBatches[i].dirty = true;	
				}
			}
            
			
			if(GUILayout.Button("REMOVE",thinButtonRed)){
				for(int i = 0; i < _flare.FlareBatches.Length; i++){
					_flare.FlareBatches[i].dirty = true;	
				}
//				element.flare._FlareBatch.dirty = true;
				element.flare.Elements.Remove(element);
				return;
			}
		}
		EditorGUILayout.EndHorizontal();
		//Preview Texture Renderer
		{
			int extra = 0;
			#if UNITY_4_3
			extra = 15;
			#endif

			Rect rect2 = GUILayoutUtility.GetLastRect();
			
			Rect outerRect = new Rect(10+extra,rect2.yMin+1,22,22);
			
			if(_flare._Atlas)
				if(_flare._Atlas.texture)
					if((element.elementTextureID < _flare._Atlas.elementsList.Count))
						GUI.DrawTextureWithTexCoords(outerRect, _flare._Atlas.texture, _flare._Atlas.elementsList[element.elementTextureID].UV, false);
		}
        
		{
			if((element.colorTexture == null)||element.colorTextureDirty){
				if(element.useColorRange)
					element.colorTexture = CreateGradientTex(element,element.SubElementColor_Start,element.SubElementColor_End);
				else
					element.colorTexture = CreateGradientTex(element,element.ElementTint,element.ElementTint);
			}

			int extra = 0;
			#if UNITY_4_3
			extra = 15;
			#endif
			
			Rect rect2 = GUILayoutUtility.GetLastRect();
			Rect outerRect = new Rect(3+extra,rect2.yMin+1,6,22);
		 	Rect UV = new Rect(0,0,1,1);
			
			GUI.DrawTextureWithTexCoords(outerRect, element.colorTexture, UV, false);
			
		}
		
		if(element.type != elementType){
			element.type = elementType;
			
			
			switch(elementType){
				case(ProFlareElement.Type.Single):
                    break;
				case(ProFlareElement.Type.Multi):
                    
                    if(element.subElements.Count == 0)
                    {
						for(int i = 0; i < 5; i++){
	                        SubElement sub = new SubElement();
	                        sub.random = Random.Range(0f,1f);
	                        sub.random2 = Random.Range(0f,1f);
	                        sub.RandomColorSeedR = Random.Range(-1f,1f);
	                        sub.RandomColorSeedG = Random.Range(-1f,1f);
	                        sub.RandomColorSeedB = Random.Range(-1f,1f);
	                        element.subElements.Add(sub);
						}
						element.useRangeOffset = true;
						element.Editing = true;
                    }
                    break;
			}
			
			for(int f = 0; f < element.flare.FlareBatches.Length; f++){ 
					element.flare.FlareBatches[f].dirty = true;
			}
		}
		
 		if(element.Editing ){
	  		QuickEdit(element,count);
			
			if(element.type == ProFlareElement.Type.Multi)
 			   	ElementOptions(element,count);
			
 	 	  	OffsetOptions(element,count);
 	 	 	ColorOptions(element,count);
 	 	 	ScaleOptions(element,count);
 	 	 	RotationOptions(element,count);
 	 		OverrideOptions(element,count);
		}
	}
	
	void QuickEdit(ProFlareElement element,int count){
        
		EditorGUILayout.BeginVertical("box");
        
		EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Quick Edit -",GUILayout.MaxWidth(80));
            
            element.Scale = EditorGUILayout.Slider(element.Scale,0f,5f,GUILayout.MaxWidth(180));
			GUILayout.FlexibleSpace();
            if(!element.useColorRange){
                element.ElementTint = EditorGUILayout.ColorField(element.ElementTint,GUILayout.MaxWidth(60));
                
                if(GUI.changed)
                    element.colorTextureDirty = true;
                
              //  byte TintA = (byte)EditorGUILayout.Slider(element.Tint.a,0f,1f,GUILayout.MaxWidth(180));
				
              //  if(TintA != element.Tint.a){
              //      element.Tint.a = TintA;
              //  }
				
				float TintA = EditorGUILayout.Slider(element.ElementTint.a,0f,1f,GUILayout.MaxWidth(180));
				
                 if(TintA != element.ElementTint.a){
                     element.ElementTint.a = TintA;
                 }
                
            }else{
                element.SubElementColor_Start = EditorGUILayout.ColorField(element.SubElementColor_Start,GUILayout.MaxWidth(60));
                element.SubElementColor_End = EditorGUILayout.ColorField(element.SubElementColor_End,GUILayout.MaxWidth(60));

                if(GUI.changed)
                    element.colorTextureDirty = true;
                
                for(int i = 0; i < element.subElements.Count; i++){
                    element.subElements[i].color =	Color.Lerp(element.SubElementColor_Start,element.SubElementColor_End,element.subElements[i].random);
                }
            }
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.EndVertical();
	}
	
	void ElementOptions(ProFlareElement element,int count){
		
		Rect  r  = EditorGUILayout.BeginVertical("box");
		Rect r2 = r;
	
		r2.height = 20;

		if (GUI.Button(r2, GUIContent.none,dropDownButton))
            element.ElementSetting = element.ElementSetting ? false : true;
        
		GUILayout.Label("Multi Element Options");
        
		if(!element.ElementSetting){
			EditorGUILayout.EndVertical();
			return;
		}
		
		GUILayout.Space(5f);
		
		if(element.type == ProFlareElement.Type.Multi){
            
			EditorGUILayout.BeginHorizontal();
            {
                
                EditorGUILayout.LabelField("Sub Elements Count : "+element.subElements.Count);
                
                if(GUILayout.Button("+")){
                    SubElement sub = new SubElement();
                    
                    sub.random = Random.Range(0f,1f);
                    sub.random2 = Random.Range(0f,1f);
					 
                    sub.RandomScaleSeed = Random.Range(-1f,1f);
                    sub.RandomColorSeedR = Random.Range(-1f,1f);
                    sub.RandomColorSeedG = Random.Range(-1f,1f);
                    sub.RandomColorSeedB = Random.Range(-1f,1f);
                    sub.RandomColorSeedA = Random.Range(-1f,1f);
                    
                    element.subElements.Add(sub);
                     
                    for(int f = 0; f < element.flare.FlareBatches.Length; f++){ 
						element.flare.FlareBatches[f].dirty = true;
					}
                }
                if(GUILayout.Button("-")){
                    //SubElement sub = new SubElement();
                    if(element.subElements.Count > 0){
                        element.subElements.Remove(element.subElements[element.subElements.Count-1]);
                        
                        
                        for(int f = 0; f < element.flare.FlareBatches.Length; f++){ 
							element.flare.FlareBatches[f].dirty = true;
						}
                    }
                }
                
                if(GUILayout.Button("Update Random Seed")){
                    for(int i = 0; i < element.subElements.Count; i++){
                        Updated = true;
                        element.subElements[i].random = Random.Range(0f,1f);
                        element.subElements[i].random2 = Random.Range(0f,1f);
                        element.subElements[i].RandomScaleSeed = Random.Range(-1f,1f);
                        element.subElements[i].RandomColorSeedR = Random.Range(-1f,1f);
                        element.subElements[i].RandomColorSeedG = Random.Range(-1f,1f);
                        element.subElements[i].RandomColorSeedB = Random.Range(-1f,1f);
                        element.subElements[i].RandomColorSeedA = Random.Range(-1f,1f);
                    }
                }
			}
			EditorGUILayout.EndHorizontal();
		}
        EditorGUILayout.EndVertical();
	}
	
	
	void ColorOptions(ProFlareElement element,int count){
        
		Rect  r  = EditorGUILayout.BeginVertical("box");
		Rect r2 = r;
		r2.height = 20;
		
		
		if (GUI.Button(r2, GUIContent.none,dropDownButton))
            element.ColorSetting = element.ColorSetting ? false : true;
        
		
		GUILayout.Label("Color Options");
		{
			
			if(element.ColorSetting){
				GUILayout.Space(5f);
                if(element.type == ProFlareElement.Type.Single){
                    element.ElementTint = EditorGUILayout.ColorField("Tint",element.ElementTint);
                    
				}else{
                    
					EditorGUILayout.BeginHorizontal();
                    {
                        
                        element.useColorRange = EditorGUILayout.Toggle("Use Color Tint Range",element.useColorRange);
                        
                        if(!element.useColorRange){
                            element.ElementTint = EditorGUILayout.ColorField("Tint",element.ElementTint);
                        }else{
                            element.SubElementColor_Start = EditorGUILayout.ColorField(element.SubElementColor_Start);
                            element.SubElementColor_End = EditorGUILayout.ColorField(element.SubElementColor_End);
                        }
					}
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.LabelField("Color Random");
					element.RandomColorAmount.x = EditorGUILayout.Slider("  R",element.RandomColorAmount.x,0f,1f);
					element.RandomColorAmount.y = EditorGUILayout.Slider("  G",element.RandomColorAmount.y,0f,1f);
					element.RandomColorAmount.z = EditorGUILayout.Slider("  B",element.RandomColorAmount.z,0f,1f);
					element.RandomColorAmount.w = EditorGUILayout.Slider("  Brightness",element.RandomColorAmount.w,0f,1f);
                }
			}
			
			if(element.type == ProFlareElement.Type.Single){
                
			}else{
				if(!element.useColorRange){
                    
                    for(int i = 0; i < element.subElements.Count; i++){
                        Color col = element.ElementTint;
                        
                        col.r = Mathf.Clamp01( col.r+(element.RandomColorAmount.x*element.subElements[i].RandomColorSeedR));
                        col.g = Mathf.Clamp01( col.g+(element.RandomColorAmount.y*element.subElements[i].RandomColorSeedG));
                        col.b = Mathf.Clamp01( col.b+(element.RandomColorAmount.z*element.subElements[i].RandomColorSeedB));
                        col.a = Mathf.Clamp01( col.a+(element.RandomColorAmount.w*element.subElements[i].RandomColorSeedA));
                        
                        element.subElements[i].color = col;
                    }
                }else{
                    
                    for(int i = 0; i < element.subElements.Count; i++){
                        Color col =  Color.Lerp(element.SubElementColor_Start,element.SubElementColor_End,element.subElements[i].random);
                        
                        col.r = Mathf.Clamp01(col.r+(element.RandomColorAmount.x*element.subElements[i].RandomColorSeedR));
                        col.g = Mathf.Clamp01(col.g+(element.RandomColorAmount.y*element.subElements[i].RandomColorSeedG));
                        col.b = Mathf.Clamp01(col.b+(element.RandomColorAmount.z*element.subElements[i].RandomColorSeedB));
                        col.a = Mathf.Clamp01(col.a+(element.RandomColorAmount.w*element.subElements[i].RandomColorSeedA));
                        
                        element.subElements[i].color = col;
                    }
                    
                }
			}
		}
		EditorGUILayout.EndVertical();
	}
	
	void OffsetOptions(ProFlareElement element,int count){
        
		Rect  r  = EditorGUILayout.BeginVertical("box");
		Rect r2 = r;
		r2.height = 20;
		
		
		if (GUI.Button(r2, GUIContent.none,dropDownButton))
            element.OffsetSetting = element.OffsetSetting ? false : true;
		
		
		GUILayout.Label("Offset Options");
        
		{
			if(element.OffsetSetting){
                GUILayout.Space(5f);
                if(element.type == ProFlareElement.Type.Single){
                    element.position = EditorGUILayout.Slider("OffSet",element.position,-1.5f,2.5f);
                    
                }else{
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        
                        element.useRangeOffset = EditorGUILayout.Toggle("Use offset range",element.useRangeOffset);
                        
                        if(element.useRangeOffset){
                            
                            float minTemp = element.SubElementPositionRange_Min;
                            float maxTemp = element.SubElementPositionRange_Max;
                            
                            
                            EditorGUILayout.MinMaxSlider(ref  element.SubElementPositionRange_Min ,ref element.SubElementPositionRange_Max,-4.5f,4.5f);
                            
                            if((minTemp != element.SubElementPositionRange_Min)||(maxTemp != element.SubElementPositionRange_Max)||Updated){
                                
                            }
                            
                            for(int i = 0; i < element.subElements.Count; i++){
                                element.subElements[i].position = Mathf.Lerp(element.SubElementPositionRange_Min,element.SubElementPositionRange_Max,element.subElements[i].random);
                                element.subElements[i].scale = Mathf.Lerp(-100f,100f,element.subElements[i].random2);
                                Updated = true;
                            }
                            
                            EditorGUILayout.FloatField(element.SubElementPositionRange_Min,GUILayout.MaxWidth(30));
                            
                            EditorGUILayout.FloatField(element.SubElementPositionRange_Max,GUILayout.MaxWidth(30));
                            
                        }else{
                            element.position = EditorGUILayout.Slider(element.position,-1.5f,2.5f);
                        }
					}
                    EditorGUILayout.EndHorizontal();
                }
				
                
				EditorGUILayout.LabelField("Anamorphic");
				float x = EditorGUILayout.Slider("Vertical ",element.Anamorphic.x,0f,1f);
				float y = EditorGUILayout.Slider("Horizontal ",element.Anamorphic.y,0f,1f);
				element.Anamorphic = new Vector2(x,y);
				
                
                element.OffsetPostion = EditorGUILayout.Vector2Field("Position Offset", element.OffsetPostion);
                
			}
			
			if(element.type == ProFlareElement.Type.Single){
				
			}
			if(element.type == ProFlareElement.Type.Multi){
				for(int i = 0; i < element.subElements.Count; i++){
                    element.subElements[i].position = Mathf.Lerp(element.SubElementPositionRange_Min,element.SubElementPositionRange_Max,element.subElements[i].random);
                    element.subElements[i].scale = Mathf.Lerp(-100f,100f,element.subElements[i].random2);
                }
			}
			
		}EditorGUILayout.EndVertical();
	}
	
	
	void ScaleOptions(ProFlareElement element,int count){
        
		Rect  r  = EditorGUILayout.BeginVertical("box");
		Rect r2 = r;
		r2.height = 20;
		
		
		if (GUI.Button(r2, GUIContent.none,dropDownButton))
            element.ScaleSetting = element.ScaleSetting ? false : true;
		
		
		GUILayout.Label("Scale Options");
		
		{
			if(element.ScaleSetting){
				
				GUILayout.Space(5f);
				
				EditorGUILayout.BeginHorizontal();
				{
                    EditorGUILayout.LabelField("Scale");
                    element.Scale = EditorGUILayout.Slider(element.Scale,0f,5f);
				}
				EditorGUILayout.EndHorizontal();
				
				//EditorGUILayout.BeginHorizontal();
				{
                    EditorGUILayout.LabelField("Aspect",GUILayout.MinWidth(120));
                    element.size.x = Mathf.Max(0f, EditorGUILayout.FloatField("   X", element.size.x));
                    element.size.y = Mathf.Max(0f, EditorGUILayout.FloatField("   Y",element.size.y));
				}
				//EditorGUILayout.EndHorizontal();
				
				
				
				
				if(element.type == ProFlareElement.Type.Multi){
					
					element.ScaleRandom = EditorGUILayout.Slider("Random Scale",element.ScaleRandom,0,1f);
					
					EditorGUILayout.BeginHorizontal();
                    {
                        
                        element.useScaleCurve = EditorGUILayout.Toggle("Use Scale Range",element.useScaleCurve,GUILayout.MaxWidth(180));
                        
                        GUI.enabled = element.useScaleCurve;
                        
                        if(element.useScaleCurve)
                            Updated = true;
                        
                        if(element.type == ProFlareElement.Type.Multi){
                            element.ScaleCurve = EditorGUILayout.CurveField(element.ScaleCurve);
                        }
                        
                        if(GUILayout.Button("Reset",GUILayout.MaxWidth(50))){
                            element.ScaleCurve = new AnimationCurve(new Keyframe(0, 0.1f), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.1f));
                        }
                        
                        GUI.enabled =  true;
					}
					EditorGUILayout.EndHorizontal();
				}
            }
		}
		
		if(element.type == ProFlareElement.Type.Multi){
            for(int i = 0; i < element.subElements.Count; i++){
             	if(element.useScaleCurve)
					element.subElements[i].scale =  (1+element.subElements[i].RandomScaleSeed*(element.ScaleRandom))*element.ScaleCurve.Evaluate(element.subElements[i].random);
				else
					element.subElements[i].scale = (1+element.subElements[i].RandomScaleSeed*(element.ScaleRandom));
            }
        }
		
        EditorGUILayout.EndVertical();
	}
	
	void RotationOptions(ProFlareElement element,int count){
        
		Rect  r  = EditorGUILayout.BeginVertical("box");
		Rect r2 = r;
		r2.height = 20;
		
		
		if (GUI.Button(r2, GUIContent.none,dropDownButton))
            element.RotationSetting = element.RotationSetting ? false : true;
		
		
		GUILayout.Label("Rotation Options");
		{
            
			if(element.RotationSetting){
				GUILayout.Space(5f);
				
				element.angle = EditorGUILayout.FloatField("Angle",element.angle);
                
				if(element.type == ProFlareElement.Type.Multi){
                    element.useStarRotation = EditorGUILayout.Toggle("Use Star Rotation",element.useStarRotation);
					element.useRandomAngle = EditorGUILayout.Toggle("Use Random Rotation",element.useRandomAngle);
					
					
					if(element.useStarRotation){
						for(int i = 0; i < element.subElements.Count; i++){
							element.subElements[i].angle = (180f/element.subElements.Count)*i;
							Updated = true;
						}
					}else{
						for(int i = 0; i < element.subElements.Count; i++){
							element.subElements[i].angle = 0;
							Updated = true;
						}
					}
					
					if(element.useRandomAngle){
						
						float minTemp = element.SubElementAngleRange_Min;
					 	float maxTemp = element.SubElementAngleRange_Max;
						
						EditorGUILayout.BeginHorizontal();
                        {
                            
                            EditorGUILayout.MinMaxSlider(ref  element.SubElementAngleRange_Min ,ref element.SubElementAngleRange_Max,-180f,180f);
                            
                            EditorGUILayout.FloatField(element.SubElementAngleRange_Min);
                            
                            EditorGUILayout.FloatField(element.SubElementAngleRange_Max);
                            
                            if((minTemp != element.SubElementAngleRange_Min)||(maxTemp != element.SubElementAngleRange_Max)||Updated){
                                for(int i = 0; i < element.subElements.Count; i++){
                                    
                                    element.subElements[i].angle = element.subElements[i].angle+Mathf.Lerp(-element.SubElementAngleRange_Min,element.SubElementAngleRange_Max,element.subElements[i].random2);
                                    Updated = true;
                                }
                                
                            }
                            
						}
						EditorGUILayout.EndHorizontal();
                        
					}
				}
				element.rotateToFlare = EditorGUILayout.Toggle("Rotate To Flare",element.rotateToFlare);
				element.rotationSpeed = EditorGUILayout.FloatField("Movement Based Rotation",element.rotationSpeed);
				element.rotationOverTime = EditorGUILayout.FloatField("Rotation Overtime Speed",element.rotationOverTime);
			}
		}
        EditorGUILayout.EndVertical();
	}
	
	void OverrideOptions(ProFlareElement element,int count){
        
		
		Rect  r  = EditorGUILayout.BeginVertical("box");
		Rect r2 = r;
		r2.height = 20;
		
		
		if (GUI.Button(r2, GUIContent.none,dropDownButton))
            element.OverrideSetting = element.OverrideSetting ? false : true;
		
		
		GUILayout.Label("Override Options");
		{
			
			if(element.OverrideSetting){
				GUILayout.Space(5f);
				GUI.enabled = element.flare.useDynamicEdgeBoost;
				EditorGUILayout.BeginHorizontal();
				{
                    element.OverrideDynamicEdgeBoost = EditorGUILayout.Toggle("Dynamic Edge Scale",element.OverrideDynamicEdgeBoost);
					GUILayout.FlexibleSpace();
                    if(element.OverrideDynamicEdgeBoost)
                        element.DynamicEdgeBoostOverride = EditorGUILayout.FloatField("    ",element.DynamicEdgeBoostOverride);
				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();{
                    element.OverrideDynamicEdgeBrightness = EditorGUILayout.Toggle("Dynamic Edge Brightness",element.OverrideDynamicEdgeBrightness);
                    
                    if(element.OverrideDynamicEdgeBrightness)
                        element.DynamicEdgeBrightnessOverride = EditorGUILayout.FloatField("    ",element.DynamicEdgeBrightnessOverride);
				}
				EditorGUILayout.EndHorizontal();
				
				GUI.enabled = true;
				GUI.enabled = element.flare.useDynamicCenterBoost;
				
				EditorGUILayout.BeginHorizontal();
				{
                    element.OverrideDynamicCenterBoost = EditorGUILayout.Toggle("Dynamic Center Scale",element.OverrideDynamicCenterBoost);
                    
                    if(element.OverrideDynamicCenterBoost)
                        element.DynamicCenterBoostOverride = EditorGUILayout.FloatField("    ",element.DynamicCenterBoostOverride);
				}
				EditorGUILayout.EndHorizontal();
				
				
				EditorGUILayout.BeginHorizontal();
				{
                    element.OverrideDynamicCenterBrightness = EditorGUILayout.Toggle("Dynamic Center Brightness",element.OverrideDynamicCenterBrightness);
                    
                    if(element.OverrideDynamicCenterBrightness)
                        element.DynamicCenterBrightnessOverride = EditorGUILayout.FloatField("    ",element.DynamicCenterBrightnessOverride);
				}
				EditorGUILayout.EndHorizontal();
				
				GUI.enabled = true;
			}
			
		}
		EditorGUILayout.EndVertical();
	}
	
	Texture2D CreateGradientTex (ProFlareElement element, Color c0, Color c1)
	{
		if(element.colorTexture != null){
			DestroyImmediate(element.colorTexture);
			element.colorTexture = null;
		}
		
		element.colorTexture = new Texture2D(1, 16);
		element.colorTexture.name = "[ProFlare] Gradient Texture";
		element.colorTexture.hideFlags = HideFlags.DontSave;
        
        
		for (int i = 0; i < 16; ++i)
		{
			float f = i*(1f/16f);
			element.colorTexture.SetPixel(0, i, Color.Lerp(c0, c1, f));
		}
        
		element.colorTexture.Apply();
		element.colorTexture.filterMode = FilterMode.Bilinear;
		return element.colorTexture;
	}
	
	
	public int LayerMaskField (LayerMask selected) {
        
        ArrayList layers = new ArrayList ();
        
        ArrayList layerNumbers = new ArrayList ();
        
        string name = "";
        
        for (int i=0;i<32;i++) {
            
            string layerName = LayerMask.LayerToName (i);
            
            if (layerName != "") {
                
                if (selected == (selected | (1 << i))) {
                    
                    layers.Add ("✓  "+layerName);
                    
                    name += layerName+", ";
                } else {
                    layers.Add ("    "+layerName);
                }
                layerNumbers.Add (i);
            }
        }
        
        bool preChange = GUI.changed;
        
        GUI.changed = false;
        
        int[] LayerNumbers = layerNumbers.ToArray (typeof(int)) as int[];
        
        int newSelected = EditorGUILayout.Popup ("Layer Mask",-1,layers.ToArray(typeof(string)) as string[],EditorStyles.layerMaskField);
        
        if (GUI.changed) {
            
            if (selected == (selected | (1 << LayerNumbers[newSelected]))) {
                
                selected &= ~(1 << LayerNumbers[newSelected]);
                
                Debug.Log ("Set Layer "+LayerMask.LayerToName (LayerNumbers[newSelected]) + " To False "+selected.value);
				
            } else {
                
                Debug.Log ("Set Layer "+LayerMask.LayerToName (LayerNumbers[newSelected]) + " To True "+selected.value);
                
                selected = selected | (1 << LayerNumbers[newSelected]);
                
            }
            
        } else {
            
            GUI.changed = preChange;
            
        }
        return selected;
    }
	
	ProFlareElement CloneElement(ProFlareElement element){
		
		ProFlareElement _element = new ProFlareElement();
		
		_element.Editing = true;
        
        _element.Visible = element.Visible;
        
        _element.elementTextureID = element.elementTextureID;
        
        _element.SpriteName = element.SpriteName;
        
        _element.flare = element.flare;
        
        _element.flareAtlas = element.flareAtlas;
        
        _element.Brightness = element.Brightness;
        _element.Scale = element.Scale;
        _element.ScaleRandom = element.ScaleRandom;
        _element.ScaleFinal = element.ScaleFinal;
        
        _element.RandomColorAmount = element.RandomColorAmount;
        
        //Element OffSet Properties
        _element.position = element.position;
        
        _element.useRangeOffset = element.useRangeOffset;
        
        _element.SubElementPositionRange_Min = element.SubElementPositionRange_Min;
        _element.SubElementPositionRange_Max = element.SubElementPositionRange_Max;
        
        _element.SubElementAngleRange_Min = element.SubElementAngleRange_Min;
        _element.SubElementAngleRange_Max = element.SubElementAngleRange_Max;
        
        
        _element.OffsetPosition = element.OffsetPosition;
        
        _element.Anamorphic = element.Anamorphic;
        
        _element.OffsetPostion = element.OffsetPostion;
        
        _element.angle = element.angle;
        _element.FinalAngle = element.FinalAngle;
        _element.useRandomAngle = element.useRandomAngle;
        _element.useStarRotation = element.useStarRotation;
        _element.AngleRandom_Min = element.AngleRandom_Min;
        _element.AngleRandom_Max = element.AngleRandom_Max;
        _element.OrientToSource = element.OrientToSource;
        _element.rotateToFlare = element.rotateToFlare;
        _element.rotationSpeed = element.rotationSpeed;
        _element.rotationOverTime = element.rotationOverTime;
        
        _element.useColorRange = element.useColorRange;
        
        _element.ElementFinalColor = element.ElementFinalColor;
        
        _element.ElementTint = element.ElementTint;
        
        _element.SubElementColor_Start = element.SubElementColor_Start;
        _element.SubElementColor_End = element.SubElementColor_End;
        
        _element.useScaleCurve = element.useScaleCurve;
        _element.ScaleCurve = new AnimationCurve(element.ScaleCurve.keys);
        
	
        _element.OverrideDynamicEdgeBoost = element.OverrideDynamicEdgeBoost;
        _element.DynamicEdgeBoostOverride = element.DynamicEdgeBoostOverride;
        
        _element.OverrideDynamicCenterBoost = element.OverrideDynamicCenterBoost;
        _element.DynamicCenterBoostOverride = element.DynamicCenterBoostOverride;
        
        _element.OverrideDynamicEdgeBrightness = element.OverrideDynamicEdgeBrightness;
        _element.DynamicEdgeBrightnessOverride = element.DynamicEdgeBrightnessOverride;
        
        _element.OverrideDynamicCenterBrightness = element.OverrideDynamicCenterBrightness;
        _element.DynamicCenterBrightnessOverride = element.DynamicCenterBrightnessOverride;
        
        _element.size = element.size;
        
        for(int i = 0; i < element.subElements.Count; i++){
            
            SubElement Sub = new SubElement();
            
            Sub.color = element.subElements[i].color;
            Sub.position = element.subElements[i].position;
            Sub.offset = element.subElements[i].offset;
            Sub.angle = element.subElements[i].angle;
            Sub.scale = element.subElements[i].scale;
            Sub.random = element.subElements[i].random;
            Sub.random2= element.subElements[i].random2;
            
			
            Sub.RandomScaleSeed = element.subElements[i].RandomScaleSeed;
            Sub.RandomColorSeedR = element.subElements[i].RandomColorSeedR;
            Sub.RandomColorSeedG = element.subElements[i].RandomColorSeedG;
            Sub.RandomColorSeedB = element.subElements[i].RandomColorSeedB;
            Sub.RandomColorSeedA = element.subElements[i].RandomColorSeedA;
			
            _element.subElements.Add(Sub);
        }
        
        _element.type = element.type;
		
		return  _element;
		
	}
	
	void  OnSceneGUI () {
		_flare = target as ProFlare;
        if(!_flare.UseAngleLimit)
            return;
        
        Handles.color = new Color(1f,1f,1f,0.2f);
        
		Handles.DrawSolidArc(_flare.transform.position, 
                             _flare.transform.up, 
                             _flare.transform.forward, 
                             _flare.maxAngle/2, 
                             5);
        
		Handles.DrawSolidArc(_flare.transform.position, 
                             _flare.transform.up, 
                             _flare.transform.forward, 
                             -_flare.maxAngle/2, 
                             5);
        
        Handles.color = Color.white;
		
        Handles.ScaleValueHandle(_flare.maxAngle,
                                 _flare.transform.position + _flare.transform.forward*5,
                                 _flare.transform.rotation,
                                 1,
                                 Handles.ConeCap,
                                 2);
    }
}
