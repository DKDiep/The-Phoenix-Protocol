using UnityEngine;
using System.Collections;

public class CrosshairMovement : MonoBehaviour {

    private int controlling = 0;
    private const int N_CROSSHAIRS = 4;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
    void Update()
    {
        // Check to see if any of the crosshair keys have been pressed
        for (int i = 0; i < N_CROSSHAIRS; i++)
            if (Input.GetKeyDown(i.ToString()))
            {
                controlling = i;
                Debug.Log("Controlling " + i);
            }

    }

	void FixedUpdate ()
    {
        // Get the currently controlled crosshair
        Transform selectedCrosshair = this.transform.Find("CrosshairImage" + controlling);

        // Update its position to the current mouse position
        Vector3 position = selectedCrosshair.position;
        position.x = Input.mousePosition.x;
        position.y = Input.mousePosition.y;
        selectedCrosshair.position = position;
    }


}
