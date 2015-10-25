
/// ProFlares - v1.08- Copyright 2014-2015 All rights reserved - ProFlares.com


using UnityEngine;
using System.Collections.Generic;
using System;

public class ProFlareAtlas : MonoBehaviour {
	
	[System.Serializable]
	public class Element
	{
		public string name = "Flare Element";
		public Rect UV = new Rect(0f, 0f, 1f, 1f);
		public bool Imported;
	}
	
	public Texture2D texture;
	
	public int elementNumber = 0;
	
	public bool editElements = false;
	
	[SerializeField] public List<Element> elementsList = new List<Element>();
	
	public string[] elementNameList;
	
	public void UpdateElementNameList(){
		elementNameList = new string[elementsList.Count];
		
		for(int i = 0; i < elementNameList.Length; i++)
			elementNameList[i] = elementsList[i].name;
	}
    
}
