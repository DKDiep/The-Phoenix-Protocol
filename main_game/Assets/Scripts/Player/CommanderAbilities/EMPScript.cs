using UnityEngine;
using System.Collections;

public class EMPScript : MonoBehaviour {

    private GameSettings settings;
    private float maxRadius;
    private float radius = 0;
    private float growthRate = 500f;

	// Use this for initialization
	void Start () {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        maxRadius = settings.empRadius;
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(radius < maxRadius)
        {
            radius += Time.deltaTime * growthRate;
            transform.localScale = new Vector3(radius, radius, radius);
        }
        else
        {
            Destroy(this.gameObject);
        }
	}
}
