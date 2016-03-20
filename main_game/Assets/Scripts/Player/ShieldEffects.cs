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
	#pragma warning disable 0649 // Disable warnings about unset private SerializeFields
	[SerializeField] private Material field; // Shield material to use
	#pragma warning restore 0649

	private Color fullShield;  // Colour of shield when full
	private Color emptyShield; // Color of shield when empty
    private Color overdrive;
	private Color fading;      // Intermediate colour
	private Color startFade;   // Temporary colour used when fading
	private Renderer myMat;
	private float shieldAlpha; // Alpha value of shield colour
	private float meshOffset;  // Positional offset from player ship mesh
	private bool burstShield;  // Detect when shield is depleted
    public bool overdriveEnabled = false;

    void Start() 
    {
        // Initialise shield parent and position
        transform.parent = GameObject.Find("PlayerShip(Clone)").transform;
        transform.localPosition = new Vector3(0, 0, 0);
        this.gameObject.GetComponent<Renderer>().material = field;

        // Set colours
        myMat = this.gameObject.GetComponent<Renderer>();
        myMat.material.SetColor("_InnerTint", new Color(0.0f,0.0f,0.0f,0.0f));
        myMat.material.SetColor("_OuterTint", new Color(0.0f,0.0f,0.0f,0.0f));
        myMat.material.SetFloat("_Offset", 0.05f);

        fullShield = new Color(0.20f, 0.57f, 1.0f);
        emptyShield = new Color(0.76f, 0.12f, 0.12f);
        overdrive = new Color(0f,1f,0f);
        shieldAlpha = 0;
        burstShield = false;
    }

    void Update()
    {
        if(!overdriveEnabled)
        {
                // When hit, fade in shield
          if(shieldAlpha > 0 && !burstShield)
          {
            shieldAlpha -= 4f * Time.deltaTime;
            Color shieldCol = Color.Lerp(Color.black, startFade, shieldAlpha);
            myMat.material.SetColor("_InnerTint", shieldCol);
            myMat.material.SetColor("_OuterTint", shieldCol);
          }

          // When shield is initially depleted, increase meshOffset and size for burst effect
          if(burstShield)
          {
            if(meshOffset < 600 || shieldAlpha > 0f)
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
        else
        {
            if(shieldAlpha < 1f)
            {
                shieldAlpha += 8f * Time.deltaTime;
                Color shieldCol = Color.Lerp(Color.black, overdrive, shieldAlpha);
                startFade = overdrive;
                myMat.material.SetColor("_InnerTint", shieldCol);
                myMat.material.SetColor("_OuterTint", shieldCol);
            }


        }

    }

    // When player is hit, initialise shield fade values
    public void Impact(float value)
    {
        if(!overdriveEnabled)
        {
            Color shieldCol = Color.Lerp(emptyShield, fullShield, value / 100f);
            startFade = shieldCol;
            myMat.material.SetColor("_InnerTint", shieldCol);
            myMat.material.SetColor("_OuterTint", shieldCol);
            shieldAlpha = 1f;
            RpcClientImpact(value);
        }

    }

    // When shield is down, initialise burst effect values
    public void ShieldDown()
    {
        if(!overdriveEnabled)
        {
            meshOffset = 0.05f;
            burstShield = true;
            myMat.material.SetColor("_InnerTint", emptyShield);
            myMat.material.SetColor("_OuterTint", emptyShield);
            shieldAlpha = 1f;
            RpcClientShieldDown();
        }

    }

    // Client RPC calls are duplicates of the server functions, makes sure effect is also visible on clients
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