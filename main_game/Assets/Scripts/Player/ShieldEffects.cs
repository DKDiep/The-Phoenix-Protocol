using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]


public class ShieldEffects : MonoBehaviour 
{

  [SerializeField] Material field;
  Color fullShield;
  Color emptyShield;
  Renderer myMat;
  float shieldAlpha;
  Color fading;
  Color startFade;
  float meshOffset;
  bool burstShield;

    void Start() 
    {
        // Combine seperate mesh elements together and add shield material
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
        transform.gameObject.active = true;

        this.gameObject.GetComponent<Renderer>().material = field;
        myMat = this.gameObject.GetComponent<Renderer>();
        myMat.material.SetColor("_InnerTint", new Color(0.0f,0.0f,0.0f,0.0f));
        myMat.material.SetColor("_OuterTint", new Color(0.0f,0.0f,0.0f,0.0f));
        myMat.material.SetFloat("_Offset", 0.05f);
        transform.localPosition = new Vector3(0, 5.96f, -6.54f);

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
          meshOffset += 300f * Time.deltaTime;
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
    }

    public void ShieldDown()
    {
      meshOffset = 0.05f;
      burstShield = true;
      myMat.material.SetColor("_InnerTint", emptyShield);
      myMat.material.SetColor("_OuterTint", emptyShield);
      shieldAlpha = 1f;
    }
}