using UnityEngine;
using System.Collections;

public class EnemyFinder : MonoBehaviour {

    private float radius = 0;
    private GameSettings settings;
    private int enemiesFound = 0;
    public GameObject[] enemyList;
    public bool searchCompleted = false;

	// Use this for initialization
	void Start () 
    {
        settings = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        enemyList = new GameObject[settings.projectileCount];
    }
	
	// Update is called once per frame
	void Update () {

        if(radius < settings.maxTargetRadius && enemiesFound < settings.projectileCount)
        {
            radius += Time.deltaTime * 1000f;
            transform.localScale = new Vector3(radius, radius, radius);
        }
        else
        {
            searchCompleted = true;
        }
	}

    void OnTriggerEnter (Collider col)
    {
        if(col.gameObject.tag.Equals ("EnemyShip") && enemiesFound < settings.projectileCount)
        {
			// Only target enemies that aren't hacked
			EnemyLogic logic = col.gameObject.GetComponentInChildren<EnemyLogic>();
			if (logic == null || !logic.IsHacked())
			{
				enemyList[enemiesFound] = col.gameObject;
				enemiesFound++;
			}
        }
    }
   
}
