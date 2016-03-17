using UnityEngine;
using System.Collections;

public class CommanderShoot : MonoBehaviour {

    [SerializeField] float cooldown;
    private bool canShoot = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(Input.GetMouseButtonDown(0) && canShoot)
        {
            canShoot = false;
            StartCoroutine("Cooldown");
        }
            
	}

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        canShoot = true;
    }

}
