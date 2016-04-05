using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class OutpostTarget : NetworkBehaviour 
{
    private GameObject player, target;
    private float distance;
    private bool discovered;
    private Renderer myRenderer;
    [SerializeField] Material[] difficultyTextures;

	// Use this for initialization
	void Start () 
    {
	    player = GameObject.Find("CameraManager(Clone)");
        difficultyTextures[0].color = Color.red;
        difficultyTextures[1].color = Color.red;
        difficultyTextures[2].color = Color.red;
        discovered = false;
        StartCoroutine(UpdateDistance());
    }

    public void SetDifficultyTexture(int id)
    {
        RpcSetDifficultyTexture(id);
    }

    [ClientRpc]
    public void RpcSetDifficultyTexture(int id)
    {
        if(myRenderer == null)
            GetRenderer();
        myRenderer.material = difficultyTextures[id-1];
    }
	
	// Update is called once per frame
	void Update () 
    {
        if(myRenderer == null)
            GetRenderer();

            if(distance < 600 || !discovered)
                myRenderer.enabled = false;
            else
            {
                Vector3 v3 = player.transform.position - transform.position;
                target.transform.rotation = Quaternion.LookRotation(-v3);
                target.transform.eulerAngles = new Vector3(target.transform.eulerAngles.x, target.transform.eulerAngles.y, player.transform.eulerAngles.z);
                myRenderer.enabled = true;
            }

	}

    private void GetRenderer()
    {
        if(target == null)
            target = transform.Find("Target").gameObject;
        myRenderer = target.GetComponent<Renderer>();
    }

    public void StartMission()
    {
        difficultyTextures[0].color = Color.yellow;
        difficultyTextures[1].color = Color.yellow;
        difficultyTextures[2].color = Color.yellow;
    }

    public void EndMission()
    {
        difficultyTextures[0].color = Color.green;
        difficultyTextures[1].color = Color.green;
        difficultyTextures[2].color = Color.green;
    }

    private IEnumerator UpdateDistance()
    {
        distance = Vector3.Distance(player.transform.position, transform.position);
        yield return new WaitForSeconds(1f);
        StartCoroutine(UpdateDistance());
    }

    public void ShowTarget()
    {
        if(myRenderer == null)
            GetRenderer();
        myRenderer.enabled = true;
        discovered = true;
        RpcShowTarget();
    }

    [ClientRpc]
    public void RpcShowTarget()
    {
        if(myRenderer == null)
            GetRenderer();
        myRenderer.enabled = true;
        discovered = true;
    }

    public void HideTarget()
    {
        if(myRenderer == null)
            GetRenderer();
        myRenderer.enabled = false;
        RpcHideTarget();
        discovered = false;
    }

    [ClientRpc]
    public void RpcHideTarget()
    {
        if(myRenderer == null)
            GetRenderer();
        myRenderer.enabled = false;
        discovered = false;
    }
}
