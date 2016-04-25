/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Draw target over enemies
*/

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OutpostTarget : NetworkBehaviour 
{

    [SerializeField] Texture2D targetHard; // Target texture
    [SerializeField] Texture2D targetMedium; // Target texture
    [SerializeField] Texture2D targetEasy; // Target texture
    Texture2D target;
    int difficulty;
    float distance;
    bool renderTarget;
    Renderer myRenderer;
    GameObject player;
    bool showTarget = true;
    private Camera mainCam;
    private Color currentColour;
    private bool animationPlaying;
    float size;
    float targetScale = 0f;
    float xValue;

    void Start () 
    {
        myRenderer = GetComponent<Renderer>();
        currentColour = new Color(0.1f,1.0f,0f,0.8f);
        if(GameObject.Find("CameraManager(Clone)") != null)
        {
            mainCam = GameObject.Find("CameraManager(Clone)").GetComponent<Camera>();
            StartCoroutine(UpdateFrustum());
        }

        else
        {
            mainCam = GameObject.Find("CommanderCamera").GetComponent<Camera>();
        }
    }

   void Update()
   {
        if(player == null)
            player = Camera.main.gameObject;
        distance = Vector3.Distance(transform.position, player.transform.position); 

        if(animationPlaying)
        {
            if(xValue < 107.7f)
            {
                xValue += 80f * Time.deltaTime;
                targetScale = Mathf.Sin(Mathf.Deg2Rad * xValue) * 1.05f;
            }
            else
            {
                animationPlaying = false;
                targetScale = 1;
            }

        }
   }

   public void StartMission()
   {
        RpcStartMission();
   }

   [ClientRpc]
   public void RpcStartMission()
   {
        currentColour = Color.yellow;
        showTarget = true;

   }

   public void ShowTarget()
   {
        RpcShowTarget();
   }

   public void EndMission()
   {
        RpcEndMission();
   }

   [ClientRpc]
   public void RpcEndMission()
   {
        showTarget = false; 
   }

   [ClientRpc]
   public void RpcShowTarget()
   {
        animationPlaying = true;
        targetScale = 0;
        showTarget = true;
   }

   public void SetDifficultyTexture(int t_difficulty)
   {
        RpcSetDifficultyTexture(t_difficulty);
   }

   [ClientRpc]
   public void RpcSetDifficultyTexture(int t_difficulty)
   {
        difficulty = t_difficulty;
        if(difficulty == 0)
            target = targetHard;
        else if(difficulty == 1)
            target = targetMedium;
        else
            target = targetEasy;
   }

   IEnumerator UpdateFrustum()
   {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCam);
        if(GeometryUtility.TestPlanesAABB(planes, myRenderer.bounds))
            renderTarget = true;
        else renderTarget = false;
        yield return new WaitForSeconds(1f);
        StartCoroutine(UpdateFrustum());
   }

    // Draw target over enemy
    void OnGUI()
    {
        if(mainCam != null)
        {
            if(distance > 400)
            {    
                if(renderTarget && showTarget && target != null)
                {
                    Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position); // Convert from 3d to screen position
                    GUI.color = currentColour;
                    size = Mathf.Clamp(440f / (distance / 440f),0,440f) * targetScale; // Set size of target based on distance

                    GUI.DrawTexture(new Rect(screenPos.x - (size/2), Screen.height - screenPos.y - (size/2), size, size), target, ScaleMode.ScaleToFit, true, 0);
                }
            }
        }
 
    }
}