using UnityEngine;

public class bl_MMCompass : MonoBehaviour {

    [Tooltip("This target can be null, this will be take the same target of minimap")]
    public Transform Target;
    [Space(7)]
    public RectTransform CompassRoot;
    public RectTransform North;
    public RectTransform South;
    public RectTransform East;
    public RectTransform West;
    [HideInInspector] public int Grade;

    private int Opposite;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (Target == null)
        {
            if (GetComponent<bl_MiniMap>() != null)
            {
                Target = GetComponent<bl_MiniMap>().Target;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        //return always positive
        if (Target != null)
        {
            Opposite = (int)Mathf.Abs(Target.eulerAngles.y);
        }
        else
        {
            Opposite = (int)Mathf.Abs(m_Transform.eulerAngles.y);
        }
        //never greater than the maximum degree of rotation
        if (Opposite > 360)//if more
        {
            Opposite = Opposite % 360;//return to 0 
        }

        Grade = Opposite;
        //opposite angle
        if (Grade > 180)
        {
            Grade = Grade - 360;
        }
        North.anchoredPosition = new Vector2(((CompassRoot.sizeDelta.x * 0.5f) - (Grade * 2) - (CompassRoot.sizeDelta.x * 0.5f)), 0);
        South.anchoredPosition = new Vector2(((CompassRoot.sizeDelta.x * 0.5f) - Opposite * 2 + 360) - (CompassRoot.sizeDelta.x * 0.5f), 0);
        East.anchoredPosition = new Vector2(((CompassRoot.sizeDelta.x * 0.5f) - Grade * 2 + 180) - (CompassRoot.sizeDelta.x * 0.5f), 0);
        West.anchoredPosition = new Vector2(((CompassRoot.sizeDelta.x * 0.5f) - Opposite * 2 + 540) - (CompassRoot.sizeDelta.x * 0.5f), 0);

    }

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