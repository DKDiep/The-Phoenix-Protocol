using UnityEngine;
using System.Collections;

public class ToggleMouse : MonoBehaviour {

    private bool cursorEnabled;

	// Use this for initialization
	void Start () {

    cursorEnabled = false;
    Cursor.visible = false;
	
	}
	
	// Update is called once per frame
	void Update () {

        if(Input.GetKeyDown(KeyCode.O))
        {
            cursorEnabled = !cursorEnabled;
            Cursor.visible = cursorEnabled;
        }
	
	}
}
