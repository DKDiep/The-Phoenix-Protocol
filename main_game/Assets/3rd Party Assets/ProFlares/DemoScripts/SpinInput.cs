/// ProFlares - v1.08 - Copyright 2014-2015 All rights reserved - ProFlares.com


/// <summary>
/// SpinInput.cs
/// Rotates a transform based on click dragging. Works on ether X or Y Axis. Y Axis can be limited.
/// </summary>

using UnityEngine;
using System.Collections;

namespace ProFlares {
public class SpinInput : MonoBehaviour {
	
	private Transform thisTransform;
	
	float xVerlo;
	
	float targetVerloX;
	float lastX;
	float currentX;
	
	float offsetX;
	
	float finalVeloX;
	
	float tempVeloX;
		
	float YVerlo;
	
	float targetVerloY;
	float lastY;
	float currentY;
	
	float offsetY;
	
	float finalVeloY;
	
	float tempVeloY;
	
	public int cropDist = 180;
	
	public float ResponseTime = 0.2f;
	
	public bool touchMode = true;
	void Start () {
		thisTransform = transform;
	}
		
	public bool X;
	
	public bool Y;
	
 
	void LateUpdate () {
		
		
		if(X){
                    
            if(Input.GetMouseButtonDown(0)){
                //				print("MouseDown");
                lastX = 0;
                currentX = 0;
                offsetX = Input.mousePosition.x;
            }
          
            
            if(Input.GetMouseButton(0)){
                
                lastX = currentX;
                currentX = Input.mousePosition.x-offsetX;
                
                targetVerloX = (currentX-lastX)*2;
				
				if((currentX-lastX > 1)||(currentX-lastX < -1)){
					 
				}
                targetVerloX = targetVerloX*3.5f;
                
            }else{
                
                targetVerloX = targetVerloX*0.95f;
            }
			finalVeloX = Mathf.Lerp(finalVeloX,targetVerloX,20*Time.deltaTime);
			
			thisTransform.Rotate(0,finalVeloX*Time.deltaTime,0);
        }
        
        
        
        if(Y){ 
                if(Input.GetMouseButtonDown(0)){
                    //				print("MouseDown");
                    lastY = 0;
                    currentY = 0;
                    offsetY = Input.mousePosition.y;
                }
                
                if(Input.GetMouseButton(0)){
                    
                    lastY = currentY;
                    currentY = Input.mousePosition.y-offsetY;
                    
                    targetVerloY = (currentY-lastY)*-2;
                    targetVerloY = targetVerloY*1.5f;
                    
                }else{
                    
                    targetVerloY = targetVerloY*0.95f;
                }

				
			finalVeloY = Mathf.Lerp(finalVeloY,targetVerloY,20*Time.deltaTime);
			
			
			thisTransform.Rotate(finalVeloY*Time.deltaTime,0,0);
			

			Quaternion newrotation = thisTransform.rotation;
			
			newrotation.x = Mathf.Clamp(newrotation.x,-0.1f,0.3f);
			
			thisTransform.rotation = newrotation;
 
        }
        
	}
	}
}