/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Minimap Settings
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class bl_MiniMap : MonoBehaviour
{
    // Target for the minimap.
    public GameObject m_Target;
    public int MiniMapLayer = 10;
    public Camera MMCamera = null;

    //Default height to view from, if you need have a static height, just edit this.
    private float height = 75;
    public bool useCompassRotation = false;
    public float CompassSize = 175f;
    public bool DynamicRotation = true;
    public bool SmoothRotation = true;
    public float LerpRotation = 8;

    private Vector3 MiniMapPosition = Vector2.zero;
    private Vector3 MiniMapRotation = Vector3.zero;
    private Vector2 MiniMapSize = Vector2.zero;

    public Texture MapTexture = null;
    public Color TintColor = new Color(1, 1, 1, 0.9f);
    public Color SpecularColor = new Color(1, 1, 1, 0.9f);
    public Color EmessiveColor = new Color(0, 0, 0, 0.9f);
    [Range(0.1f,4)] public float EmissionAmount = 1;
    [SerializeField]private Material ReferenceMat;

    public GameObject MapPlane = null;
    public RectTransform WorldSpace = null;
    public Canvas m_Canvas = null;
    public RectTransform MMUIRoot = null;
    public Dictionary<string, Transform> ItemsList = new Dictionary<string, Transform>();

    public static Camera MiniMapCamera = null;
    public static RectTransform MapUIRoot = null;

    private bool DefaultRotationMode = false;
    private Vector3 DeafultMapRot = Vector3.zero;
    private bool DefaultRotationCircle = false;

    void Awake()
    {
        GetMiniMapSize();
        MiniMapCamera = MMCamera;
        MapUIRoot = MMUIRoot;
        DefaultRotationMode = DynamicRotation;
        DeafultMapRot = m_Transform.eulerAngles;
        DefaultRotationCircle = useCompassRotation;

        ConfigureCamera3D();
    }

    public void ConfigureCamera3D()
    {
        Camera cam = (Camera.main != null) ? Camera.main : Camera.current;
        m_Canvas.worldCamera = cam;
        cam.nearClipPlane = 0.015f;
        m_Canvas.planeDistance = 0.1f;
    }

    void Update()
    {
        if (m_Target == null)
            return;
        if (MMCamera == null)
            return;

        PositionControll();
        RotationControll();
    }


    void PositionControll()
    {
        Vector3 p = m_Transform.position;

        // Update the transformation of the camera as per the target's position.
        p.x = Target.position.x;
        p.z = Target.position.z;
        p.y = Target.position.y + height;

        //Calculate player position
        if (Target != null)
        {
          Vector3 pp = MMCamera.WorldToViewportPoint(TargetPosition);
        }
        //Camera follow the target
        m_Transform.position = Vector3.Lerp(m_Transform.position, p, Time.deltaTime * 10);

    }

    /// <summary>
    /// 
    /// </summary>
    void RotationControll()
    {

            //get local reference.
            Vector3 e = m_Transform.eulerAngles;
            e.y = Target.eulerAngles.y;
                if (m_Transform.eulerAngles.y != e.y)
                {
                    //calculate the diference 
                    float d = e.y - m_Transform.eulerAngles.y;
                    //avoid lerp from 360 to 0 or reverse
                    if (d > 180 || d < -180)
                    {
                        m_Transform.eulerAngles = e;
                    }
                }
                //Lerp rotation of map
                m_Transform.eulerAngles = Vector3.Lerp(this.transform.eulerAngles, e, Time.deltaTime * LerpRotation);
    }

    public Material CreateMaterial()
    {
        Material mat = new Material(ReferenceMat);
        mat.mainTexture = MapTexture;
        mat.SetTexture("_EmissionMap", MapTexture);
        mat.SetFloat("_EmissionScaleUI", EmissionAmount);
        mat.SetColor("_EmissionColor", EmessiveColor);
        mat.SetColor("_SpecColor", SpecularColor);

        return mat;
    }

    void ResetMapRotation() { m_Transform.eulerAngles = new Vector3(90, 0, 0); }

    public void RotationMap(bool mode) { DynamicRotation = mode; DefaultRotationMode = DynamicRotation; }

    void GetMiniMapSize()
    {
        MiniMapSize = MMUIRoot.sizeDelta;
        MiniMapPosition = MMUIRoot.anchoredPosition;
        MiniMapRotation = MMUIRoot.eulerAngles;
    }

    public Transform Target
    {
        get
        {
            if (m_Target != null)
            {
                return m_Target.GetComponent<Transform>();
            }
            return this.GetComponent<Transform>();
        }
    }
    public Vector3 TargetPosition
    {
        get
        {
            Vector3 v = Vector3.zero;
            if (m_Target != null)
            {
                v = m_Target.transform.position;
            }
            return v;
        }
    }


    //Get Transform
    private Transform t;
    private Transform m_Transform
    {
        get
        {
            if (t == null)
            {
                t = this.GetComponent<Transform>();
            }
            return t;
        }
    }
}