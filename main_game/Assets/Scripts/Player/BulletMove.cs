/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Client side bullet movement
*/

using UnityEngine;
using System.Collections;

public class BulletMove : MonoBehaviour 
{

  [SerializeField] float speed;
  //[SyncVar] Color bulletColor;
  //[SyncVar] bool ready = false;

  void Update () 
  {
    /*if(ready)
    {
        Renderer[] rend = GetComponentsInChildren<Renderer>();

        for(int i = 0; i < rend.Length; i++)
        {
            rend[i].material.SetColor("_TintColor", bulletColor);
        }
        ready = false;
    }*/

    transform.position += transform.forward * Time.deltaTime * speed;
  }

 /* public void SetColor(Color bullet)
  {
    bulletColor = bullet;
    ready = true;
  }*/
}
