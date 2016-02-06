using UnityEngine;

public class bl_CreateItemExample : MonoBehaviour {

    private bl_MMExampleManager exampler;
    private bl_MiniMap MiniMap;

    void Start()
    {
        exampler = GetComponent<bl_MMExampleManager>();
        MiniMap = exampler.GetActiveMiniMap;
    }
	// Update is called once per frame
	void Update () {
        if (MiniMap == null)
            return;

        if (Input.GetButtonDown("Fire1"))
        {
            CreateItem();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            //you only need call like this when receive damage the player / target
            MiniMap.DoHitEffect();
        }
	}

    /// <summary>
    /// Example of how create a item in run time without references
    /// just need a position where you need instantiate
    /// for sure are options for personalize the item
    /// see full structure for options in bl_MMItemInfo.cs
    /// </summary>
    void CreateItem()
    {
        //This just a example of a position where instantiate the item
        //in this example we will instantiate where input mouse is.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray,out hit))
        {
            Debug.DrawLine(ray.origin, hit.point,Color.red);
            //in this part we create a new item info
            //the most basic method is just send the (Vector3)position
            bl_MMItemInfo info = new bl_MMItemInfo(hit.point);
            //but you can customize this before send to create for eg:
            //info.Color = Color.white;
            //info.Size = 15; 
            //info.Sprite = CustomSpriteReference;
            //se bl_MMItemInfo.cs for full options avaible.

            //Now send the info to the bl_MiniMap.cs reference for create it.
            MiniMap.CreateNewItem(info);
            //Done!
        }
    }
}