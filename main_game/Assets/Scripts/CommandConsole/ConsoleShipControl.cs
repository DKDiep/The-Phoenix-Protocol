using UnityEngine;
using System.Collections;

public class ConsoleShipControl : MonoBehaviour {

	private const float rotationSpeed = 300;

	private Quaternion desiredRotation;
	private Quaternion currentRotation;
	private Quaternion rotation;
	private float xDeg, yDeg;

    private Material defaultMat;
    private Material highlightMat;

	void Start () {

 
	}
		
	void Update () {
		/*
         * Disabled the rotation as it was getting annoying. Not sure if we actually need this. 
		if (Input.GetMouseButton(0))
		{
			// Get mouse x position and rotate the ship about the y axis
			xDeg += Input.GetAxis("Mouse X") * rotationSpeed * 0.02f;
			desiredRotation = Quaternion.Euler(0, xDeg, 0);
			currentRotation = transform.rotation;
			rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime);
			transform.rotation = rotation;
		}
        */      
	}
  
    public void HighlightComponent(int component)
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in children) {
            if(child.name.Contains("Bridge") || child.name.Contains("Turret") || child.name.Contains("Engine")) 
                UnhighlightObject(child.gameObject);
        }
        foreach (Transform child in children) {
            if(component == 0 && (child.name.Contains("Bridge"))) 
                HighlightObject(child.gameObject); 
            if(component == 1 && (child.name.Contains("Turret"))) 
                HighlightObject(child.gameObject);
            if(component == 2 && (child.name.Contains("Engine"))) 
                HighlightObject(child.gameObject);
            if(component == 3 && (child.name.Contains("Bridge"))) 
                HighlightObject(child.gameObject);
        }
    }

    public void SetMaterials(Material defaultMat, Material highlightMat) 
    {
        this.defaultMat = defaultMat;
        this.highlightMat = highlightMat;
    }

    // Sets all materials belonging to a gameobject to the highlight material
    public void HighlightObject(GameObject obj)
    {
        if(obj != null)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            Material[] mats = renderer.materials;

            for(int j = 0; j < mats.Length; ++j)
                mats[j] = highlightMat;

            renderer.materials = mats;
        }
    }

    // Restores original game object materials
    public void UnhighlightObject(GameObject obj)
    {
        if(obj != null)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            Material[] mats = renderer.materials;

            for(int j = 0; j < mats.Length; ++j)
                mats[j] = defaultMat;

            renderer.materials = mats;
        }
    }
}
