/*
    Get current number of active objects in the game
*/

using UnityEngine;
using System.Collections;

public class PerformanceTest : MonoBehaviour {

    private int enemies, asteroids, bullets = 0;

	void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.P))
            SampleScene();
	}

    private void SampleScene()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
        enemies = asteroids = bullets = 0;

        foreach(GameObject go in allObjects)
        {
            if(go.GetComponent<BulletLogic>() != null)
                bullets++;
            else if(go.GetComponent<AsteroidLogic>() != null)
                asteroids++;
            else if(go.GetComponent<EnemyLogic>() != null)
                enemies++;
        }

        Debug.Log("Number of enemies: " + enemies);
        Debug.Log("Number of asteroids: " + asteroids);
        Debug.Log("Number of bullets: " + bullets);
    }
}
