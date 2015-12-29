/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Alter shield material when hit by bullet
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]


public class ShieldEffects : NetworkBehaviour
{

  [SerializeField] Material field;
  Color fullShield;
  Color emptyShield;
  Color fading;
  Color startFade;
  Renderer myMat;
  float shieldAlpha;
  float meshOffset;
  bool burstShield;

    void Start() 
    {
        // Combine seperate mesh elements together and add shield material
        /*
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;

        while (i < meshFilters.Length) 
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.active = false;
            i++;
        }

        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

        AssetDatabase.CreateAsset( transform.GetComponent<MeshFilter>().mesh, "Assets/Resources/testMesh.asset" );
        AssetDatabase.SaveAssets();

        transform.gameObject.active = true;
        */
        transform.parent = GameObject.Find("PlayerShip(Clone)").transform;
        transform.localPosition = new Vector3(0, 0.1300001f, -0.01466703f);
        this.gameObject.GetComponent<Renderer>().material = field;
        myMat = this.gameObject.GetComponent<Renderer>();
        myMat.material.SetColor("_InnerTint", new Color(0.0f,0.0f,0.0f,0.0f));
        myMat.material.SetColor("_OuterTint", new Color(0.0f,0.0f,0.0f,0.0f));
        myMat.material.SetFloat("_Offset", 0.05f);

        fullShield = new Color(0.20f, 0.57f, 1.0f);
        emptyShield = new Color(0.76f, 0.12f, 0.12f);
        shieldAlpha = 0;
        burstShield = false;
    }

    void Update()
    {
      if(shieldAlpha > 0 && !burstShield)
      {
        shieldAlpha -= 4f * Time.deltaTime;
        Color shieldCol = Color.Lerp(Color.black, startFade, shieldAlpha);
        myMat.material.SetColor("_InnerTint", shieldCol);
        myMat.material.SetColor("_OuterTint", shieldCol);
      }

      if(burstShield)
      {
        if(meshOffset < 300 || shieldAlpha > 0f)
        {
          meshOffset += 400f * Time.deltaTime;
          myMat.material.SetFloat("_Offset", meshOffset);
          shieldAlpha -= 2f * Time.deltaTime;
          Color shieldCol = Color.Lerp(Color.black, emptyShield, shieldAlpha);
          myMat.material.SetColor("_InnerTint", shieldCol);
          myMat.material.SetColor("_OuterTint", shieldCol);

        }
        else
        {
          burstShield = false;
          meshOffset = 0.05f;
          myMat.material.SetFloat("_Offset", meshOffset);
          shieldAlpha = 0f;
        }
      }
    }

    public void Impact(float value)
    {
        Color shieldCol = Color.Lerp(emptyShield, fullShield, value / 100f);
        startFade = shieldCol;
        myMat.material.SetColor("_InnerTint", shieldCol);
        myMat.material.SetColor("_OuterTint", shieldCol);
        shieldAlpha = 1f;
        RpcClientImpact(value);
    }

    public void ShieldDown()
    {
        meshOffset = 0.05f;
        burstShield = true;
        myMat.material.SetColor("_InnerTint", emptyShield);
        myMat.material.SetColor("_OuterTint", emptyShield);
        shieldAlpha = 1f;
        RpcClientShieldDown();
    }

    [ClientRpc]
    void RpcClientImpact(float value)
    {
        Color shieldCol = Color.Lerp(emptyShield, fullShield, value / 100f);
        startFade = shieldCol;
        myMat.material.SetColor("_InnerTint", shieldCol);
        myMat.material.SetColor("_OuterTint", shieldCol);
        shieldAlpha = 1f;
    }

    [ClientRpc]
    void RpcClientShieldDown()
    {
        meshOffset = 0.05f;
        burstShield = true;
        myMat.material.SetColor("_InnerTint", emptyShield);
        myMat.material.SetColor("_OuterTint", emptyShield);
        shieldAlpha = 1f;
    }

}