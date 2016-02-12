/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: Display compass and rotate
*/

using UnityEngine;

public class bl_MMCompass : MonoBehaviour 
{
    public Transform target;
    [SerializeField] RectTransform compassRoot;
    [SerializeField] RectTransform north;
    [SerializeField] RectTransform south;
    [SerializeField] RectTransform east;
    [SerializeField] RectTransform west;

    private int grade, opposite;

    void Start()
    {
        if (target == null)
        {
            if (GetComponent<bl_MiniMap>() != null)
            {
                target = GetComponent<bl_MiniMap>().Target;
            }
        }
    }

    void Update()
    {
        if (target != null)
        {
            opposite = (int)Mathf.Abs(target.eulerAngles.y);
        }
        else
        {
            opposite = (int)Mathf.Abs(m_Transform.eulerAngles.y);
        }

        // Wrap to 360
        if (opposite > 360)
        {
            opposite %= 360; 
        }

        grade = opposite;

        if (grade > 180)
        {
            grade = grade - 360;
        }

        // Set compass letter positions
        north.anchoredPosition = new Vector2(((compassRoot.sizeDelta.x * 0.5f) - (grade * 2) - (compassRoot.sizeDelta.x * 0.5f)), 0);
        south.anchoredPosition = new Vector2(((compassRoot.sizeDelta.x * 0.5f) - opposite * 2 + 360) - (compassRoot.sizeDelta.x * 0.5f), 0);
        east.anchoredPosition = new Vector2(((compassRoot.sizeDelta.x * 0.5f) - grade * 2 + 180) - (compassRoot.sizeDelta.x * 0.5f), 0);
        west.anchoredPosition = new Vector2(((compassRoot.sizeDelta.x * 0.5f) - opposite * 2 + 540) - (compassRoot.sizeDelta.x * 0.5f), 0);
    }

    // If target isn't set, use own position as reference
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