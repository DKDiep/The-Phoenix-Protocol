﻿/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Draw target over enemies
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DrawTarget : NetworkBehaviour 
{

    [SerializeField] Texture2D target;
    [SyncVar] float distance;
    [SyncVar] bool draw;
    [SyncVar] bool myRender;
    EnemyLogic myLogic;
    Renderer myRenderer;

	// Use this for initialization
	void Start () 
    {
        myRenderer = GetComponent<Renderer>();

        if(isServer) myLogic = GetComponentInChildren<EnemyLogic>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(isServer)
        {
            myRender = myRenderer.isVisible;
            draw = myLogic.draw;
            distance = myLogic.distance;
        } 
	}

    // Draw target over enemy
    void OnGUI()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        screenPos = GUIUtility.ScreenToGUIPoint(screenPos);
        float size = Mathf.Clamp(128f / (distance / 100f),0,128);

        if(distance < 750 && draw && myRender) GUI.DrawTexture(new Rect(screenPos.x - (size/2), Screen.height - screenPos.y - (size/2), size, size), target, ScaleMode.ScaleToFit, true, 0);
    }
}