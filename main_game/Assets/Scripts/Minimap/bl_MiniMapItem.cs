/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Attach to an object to display it on the MiniMap
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bl_MiniMapItem : MonoBehaviour 
{
    public GameObject GraphicPrefab = null;
    public Transform Target = null;
    public Vector3 OffSet = Vector3.zero;

    public Sprite Icon = null;
    public Color IconColor = new Color(1, 1, 1, 0.9f);
    public float Size = 20;

    bool isInteractable = false;
    bool OffScreen = true;
    public float BorderOffScreen = 0.01f;
    public float OffScreenSize = 10;
    [Range(0,3)]public float RenderDelay = 0.3f;

    private Image Graphic = null;
    private RectTransform RectRoot;
    private GameObject cacheItem = null;

    void Start()
    {
        if (bl_MiniMap.MapUIRoot != null)
        {
            Target = transform.parent;
            CreateIcon();
        }
    }

    void CreateIcon()
    {
        //Instantiate UI in canvas
        cacheItem = Instantiate(GraphicPrefab) as GameObject;
        RectRoot = bl_MiniMapUtils.GetMiniMap().MMUIRoot;

        //SetUp Icon UI
        Graphic = cacheItem.GetComponent<Image>();
        if (Icon != null) { Graphic.sprite = Icon; Graphic.color = IconColor; }
        cacheItem.transform.SetParent(RectRoot.transform, false);
        Graphic.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        cacheItem.GetComponent<CanvasGroup>().interactable = isInteractable;
        if (Target == null) { Target = this.GetComponent<Transform>(); }

        bl_IconItem ii = cacheItem.GetComponent<bl_IconItem>();
        ii.DelayStart(RenderDelay);

    }

    void Update()
    {
        if (Target == null)
            return;
        if (Graphic == null)
            return;
        //Get the Rect of Target UI
        RectTransform rt = Graphic.GetComponent<RectTransform>();
        //Setting the modify position
        Vector3 CorrectPosition = TargetPosition + OffSet;
        //Convert the position of target in ViewPortPoint
        Vector2 vp2 = bl_MiniMap.MiniMapCamera.WorldToViewportPoint(CorrectPosition);
        //Calculate the position of target and convert into position of screen
        Vector2 position = new Vector2((vp2.x * RectRoot.sizeDelta.x) - (RectRoot.sizeDelta.x * 0.5f),
            (vp2.y * RectRoot.sizeDelta.y) - (RectRoot.sizeDelta.y * 0.5f));
            //Calculate the max and min distance to move the UI
            //this clamp in the RectRoot sizeDela for border
            position.x = Mathf.Clamp(position.x, -((RectRoot.sizeDelta.x * 0.5f) - BorderOffScreen), ((RectRoot.sizeDelta.x * 0.5f) - BorderOffScreen));
            position.y = Mathf.Clamp(position.y, -((RectRoot.sizeDelta.y * 0.5f) - BorderOffScreen), ((RectRoot.sizeDelta.y * 0.5f) - BorderOffScreen));
        
        //calculate the position of UI again, determine if offscreen
        //if offscreen reduce the size
        float size = Size;
        //Use this (useCompassRotation when have a circle miniMap)
        if (m_miniMap.useCompassRotation)
        {
            //Compass Rotation
            Vector3 screenPos = Vector3.zero;
            //Calculate diference
            Vector3 forward = Target.position - m_miniMap.TargetPosition;
            //Position of target from camera
            Vector3 cameraRelativeDir = bl_MiniMap.MiniMapCamera.transform.InverseTransformDirection(forward);
            //normalize values for screen fix
            cameraRelativeDir.z = 0;
            cameraRelativeDir = cameraRelativeDir.normalized / 2;
           //Convert values to positive for calculate area OnScreen and OffScreen.
            float posPositiveX = Mathf.Abs(position.x);
            float relativePositiveX = Mathf.Abs((0.5f + (cameraRelativeDir.x * m_miniMap.CompassSize)));
            //when target if offScreen clamp position in circle area.
            if (posPositiveX >= relativePositiveX)
            {
                screenPos.x = 0.5f + (cameraRelativeDir.x * m_miniMap.CompassSize)/*/ Camera.main.aspect*/;
                screenPos.y = 0.5f + (cameraRelativeDir.y * m_miniMap.CompassSize);
                position = screenPos;
                size = OffScreenSize;
            }
            else
            {
                size = Size;
            }
        }
        else
        {
            if (position.x == (RectRoot.sizeDelta.x * 0.5f) - BorderOffScreen || position.y == (RectRoot.sizeDelta.y * 0.5f) - BorderOffScreen ||
                position.x == -(RectRoot.sizeDelta.x * 0.5f) - BorderOffScreen || -position.y == (RectRoot.sizeDelta.y * 0.5f) - BorderOffScreen)
            {
                size = OffScreenSize;
            }
            else
            {
                size = Size;
            }
        }
        //Apply position to the UI (for follow)
        rt.anchoredPosition = position;
        //Change size with smooth transition
        rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, new Vector2(size, size), Time.deltaTime * 8);
        Quaternion r = Quaternion.identity;
        //r.y = Target.rotation.y;

        rt.localRotation = r;
    }

    public void DestroyItem()
    {
            Graphic.GetComponent<bl_IconItem>().DestroyIcon();
    }

    public void HideItem()
    {
        if (cacheItem != null)
        {
            cacheItem.SetActive(false);
        }
        else
        {
            Debug.Log("There is no item to disable.");
        }
    }

    public void ShowItem()
    {
        if (cacheItem != null)
        {
            cacheItem.SetActive(true);
        }
        else
        {
            Debug.Log("There is no item to active.");
        }
    }

    public Vector3 TargetPosition
    {
        get
        {
            if (Target == null)
            {
                return Vector3.zero;
            }

            return new Vector3(Target.position.x, 0, Target.position.z);
        }
    }

    private bl_MiniMap _minimap = null;
    private bl_MiniMap m_miniMap
    {
        get
        {
            if (_minimap == null)
            {
                _minimap = this.cacheItem.transform.root.GetComponentInChildren<bl_MiniMap>();
            }
            return _minimap;
        }
    }
}