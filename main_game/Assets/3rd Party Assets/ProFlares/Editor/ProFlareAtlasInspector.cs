
/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


/// <summary>
/// ProFlareAtlasInspector.cs
/// Custom inspector for the ProFlareAtlas
/// </summary>

using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(ProFlareAtlas))]
public class ProFlareAtlasInspector : Editor {
	
	ProFlareAtlas _ProFlareAtlas;
	
	public string renameString;
	
	bool listeningForGuiChanges;
	
	bool guiChanged = false;
	
	private void CheckUndo()
    {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5  || UNITY_4_6  || UNITY_5_0 
		Event e = Event.current;
		
		if ( e.type == EventType.MouseDown && e.button == 0 || e.type == EventType.KeyUp && ( e.keyCode == KeyCode.Tab ) ) {
			//Debug.Log("record1");
			Undo.RecordObject(_ProFlareAtlas,"ProFlareAtlas Undo");

			listeningForGuiChanges = true;
			guiChanged = false;
		}
		
		if ( listeningForGuiChanges && guiChanged ) {
			//Debug.Log("record2");
 
			Undo.RecordObject(_ProFlareAtlas,"ProFlareAtlas Undo");
			listeningForGuiChanges = false;

		}
#else
        Event e = Event.current;
        
        if ( e.type == EventType.MouseDown && e.button == 0 || e.type == EventType.KeyUp && ( e.keyCode == KeyCode.Tab ) ) {
			Undo.SetSnapshotTarget( _ProFlareAtlas, "ProFlareAtlas Undo" );
            Undo.CreateSnapshot();
            Undo.ClearSnapshotTarget();
            listeningForGuiChanges = true;
            guiChanged = false;
        }
        
        if ( listeningForGuiChanges && guiChanged ) {
			Undo.SetSnapshotTarget( _ProFlareAtlas, "ProFlareAtlas Undo" );
            Undo.RegisterSnapshot();
            Undo.ClearSnapshotTarget();
            listeningForGuiChanges = false;
		}
#endif
    }
	
	public override void OnInspectorGUI () {
		_ProFlareAtlas = target as ProFlareAtlas;
		CheckUndo();
        //	 base.OnInspectorGUI();
		FlareEditorHelper.DrawGuiDivider();
		GUIStyle title = FlareEditorHelper.TitleStyle();
		GUIStyle thinButton = FlareEditorHelper.ThinButtonStyle();
		GUIStyle enumDropDown = FlareEditorHelper.EnumStyleButton();
		GUIStyle redButton = FlareEditorHelper.ThinButtonRedStyle();
        
		
		EditorGUILayout.LabelField("ProFlare Atlas Editor",title);
		GUILayout.Space(10f);
		
		_ProFlareAtlas.texture = EditorGUILayout.ObjectField("Flare Atlas Texture", _ProFlareAtlas.texture, typeof(Texture2D), false) as Texture2D;
		
		if(_ProFlareAtlas.texture == null){
			EditorGUILayout.HelpBox("Assign a texture to the atlas.", MessageType.Warning,true);
        	return;
		}
		TextAsset ta = EditorGUILayout.ObjectField("Atlas JSON Import", null, typeof(TextAsset), false) as TextAsset;
        
		if (ta != null)
		{
			FlareJson.LoadSpriteData(_ProFlareAtlas, ta);
			Updated = true;
		}
        
		FlareEditorHelper.DrawGuiDivider();
		EditorGUILayout.LabelField("Atlas Elements",title);
		GUILayout.Space(6f);
		
        EditorGUILayout.BeginHorizontal();
		
		
		if(_ProFlareAtlas.elementsList.Count < 1)
			
			EditorGUILayout.HelpBox("No Elements in flare atlas", MessageType.Warning,true);
		else{
			
			_ProFlareAtlas.elementNameList = new string[_ProFlareAtlas.elementsList.Count];
			
			for(int i = 0; i < _ProFlareAtlas.elementNameList.Length; i++)
				_ProFlareAtlas.elementNameList[i] = _ProFlareAtlas.elementsList[i].name;
            
            
            int _ProFlareAtlasElementNumber = EditorGUILayout.Popup(_ProFlareAtlas.elementNumber, _ProFlareAtlas.elementNameList,enumDropDown);
			
            if(_ProFlareAtlasElementNumber != _ProFlareAtlas.elementNumber){
                _ProFlareAtlas.elementNumber = _ProFlareAtlasElementNumber;
                renameString = _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].name;
            }
			
            if(GUILayout.Button("EDIT",thinButton)){Updated = true;
                if(_ProFlareAtlas.editElements)
                    _ProFlareAtlas.editElements = false;
                else
                    _ProFlareAtlas.editElements = true;
            }
        }
		
        if(GUILayout.Button("ADD NEW",thinButton)){
			
            _ProFlareAtlas.editElements = true;
			
            ProFlareAtlas.Element element = new ProFlareAtlas.Element();
			
            element.name = "New Element " + _ProFlareAtlas.elementsList.Count;
			
            renameString = element.name;
            element.Imported = false;
			
            _ProFlareAtlas.elementsList.Add(element);
            
            _ProFlareAtlas.elementNumber = _ProFlareAtlas.elementsList.Count-1;
            
			
            Updated = true;
        }
        
		EditorGUILayout.EndHorizontal();
        
		if(_ProFlareAtlas.elementsList.Count < 1)
			return;
		
		EditorGUILayout.BeginVertical("box");
        GUILayout.Space(20f);
		
        Rect lastRect = GUILayoutUtility.GetLastRect();
        
        Rect outerRect2 = new Rect(lastRect.center.x,0+lastRect.yMin,200,200);
        
        if(_ProFlareAtlas.elementsList.Count > 0){
            GUI.DrawTextureWithTexCoords(outerRect2, _ProFlareAtlas.texture, _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].UV, false);
            GUILayout.Space(200f);
        }
        GUI.enabled = _ProFlareAtlas.editElements;
		
		 
		
        if(_ProFlareAtlas.editElements){
			int extra = 0;
#if UNITY_4_3
			extra = 10;
#endif
            Rect outerRect3 = new Rect(107+extra,lastRect.yMin,0.5f,200);
            
            Rect rect = new Rect(0,0,1,1);
            
            GUI.DrawTextureWithTexCoords(outerRect3, EditorGUIUtility.whiteTexture, rect, false);
            
            Rect outerRect4 = new Rect(7+extra,100+lastRect.yMin,200,0.5f);
            
            GUI.DrawTextureWithTexCoords(outerRect4, EditorGUIUtility.whiteTexture, rect, true);
        }
        
        GUILayout.BeginHorizontal();
		
        if(!_ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].Imported){
            renameString = EditorGUILayout.TextField("Name",renameString);
            if(GUILayout.Button("RENAME")){
                Updated = true;
                _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].name = renameString;
            }
        }else
            EditorGUILayout.LabelField("Name - "+renameString);
		
        GUILayout.EndHorizontal();
		
        EditorGUILayout.Toggle("Imported :", _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].Imported);
		
        float width = EditorGUILayout.Slider("Width", _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].UV.width,0f,1f);
        float height = EditorGUILayout.Slider("Height", _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].UV.height,0f,1f);
        
        float CenterX = EditorGUILayout.Slider("Center X", _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].UV.center.x,0f,1f);
        float CenterY = EditorGUILayout.Slider("Center Y", _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].UV.center.y,0f,1f);
        
        float xMin = _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].UV.xMin;
        float yMin = _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].UV.yMin;
        
        Rect newRect = new Rect(xMin,yMin,width,height);
        
        newRect.center = new Vector2(CenterX,CenterY);
        
        GUILayout.Space(40f);
        
        _ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber].UV = newRect;
		
        if(GUILayout.Button("DELETE ELEMENT",redButton)){
            Updated = true;
            _ProFlareAtlas.elementsList.Remove(_ProFlareAtlas.elementsList[_ProFlareAtlas.elementNumber]);
            _ProFlareAtlas.elementNumber = 0;
        }
		
		EditorGUILayout.EndVertical();
		
		GUI.enabled = true;
        
        
		if (GUI.changed || Updated)
        {
			Updated = false;
			guiChanged = true;
            EditorUtility.SetDirty (target);
        }
		
		FlareEditorHelper.DrawGuiDivider();
	}
	bool Updated = false;
}
	
