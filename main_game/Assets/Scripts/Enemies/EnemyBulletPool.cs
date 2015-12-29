using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyBulletPool : MonoBehaviour 
{

    public int pooledAmount;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject bulletLogic;
    public List<GameObject> bullets;

    void Start()
    {
        bullets = new List<GameObject>();
        for(int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject) Instantiate(bullet);
            GameObject logic = (GameObject) Instantiate(bulletLogic);
            logic.transform.parent = obj.transform;
            logic.transform.localPosition = Vector3.zero;
            obj.SetActive(false);
            ServerManager.NetworkSpawn(obj);
            bullets.Add(obj);
        }
    }
	
    public GameObject RequestBullet()
    {
        int i = 0;
        for(i = 0; i < bullets.Count; i++)
        {
                if(!bullets[i].activeInHierarchy)
                {
                    break;
                }
        }

        return bullets[i];
    }
}
