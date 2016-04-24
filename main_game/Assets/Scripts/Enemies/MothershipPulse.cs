using UnityEngine;
using System.Collections;

public class MothershipPulse : MonoBehaviour {

    private Material material;
    private float finalValue = 0f;
    private float input = 0;

	// Use this for initialization
	void Start () {

        material = GameObject.Find("blades").GetComponent<Renderer>().material;
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(input < 3.14f)
            input += 1.5f * Time.deltaTime;
        else
            input = 0;
        finalValue = Mathf.Sin(input);
        material.SetFloat("_EmissionGain", (finalValue / 4f) + 0.05f);
	}
}
