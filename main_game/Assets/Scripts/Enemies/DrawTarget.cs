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

    [SerializeField] Texture2D target; // Target texture
    [SyncVar] float distance;
    [SyncVar] bool draw;
    [SyncVar] bool myRender;
    EnemyLogic myLogic;
    Renderer myRenderer;

	void Start () 
    {
        myRenderer = GetComponent<Renderer>();

        if(isServer) myLogic = GetComponentInChildren<EnemyLogic>();
	}
	
	// Transmit values to clients
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
        if(distance < 800 && distance > 5 && draw)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position); // Convert from 3d to screen position
            /*Vector3 testPos = Camera.main.WorldToViewportPoint(transform.position); 

            if(testPos.x < 0 || testPos.x > 1 || testPos.y < 0 || testPos.y > 1) myRender = false;
            else myRender = true;*/

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            if(GeometryUtility.TestPlanesAABB(planes, myRenderer.bounds))
                myRender = true;
            else myRender = false;

            if(myRender)
            {
                float size = Mathf.Clamp(128f / (distance / 100f),0,128); // Set size of target based on distance
                GUI.DrawTexture(new Rect(screenPos.x - (size/2), Screen.height - screenPos.y - (size/2), size, size), target, ScaleMode.ScaleToFit, true, 0);
            }
        } 
    }
}
