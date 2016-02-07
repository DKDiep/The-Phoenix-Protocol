using UnityEngine;
using System.Collections;

public class bl_WorldSpace : MonoBehaviour {

    [Header("Use UI editor Tool for scale the wordSpace")]
    public Color GizmoColor = new Color(1, 1, 1, 0.75f);

    /// <summary>
    /// Debuging world space of map
    /// </summary>
    void OnDrawGizmosSelected()
    {
        RectTransform r = this.GetComponent<RectTransform>();
        Vector3 v = r.sizeDelta;
        Vector3 pivot = r.localPosition;

        Gizmos.color = GizmoColor;
        Gizmos.DrawCube(pivot, new Vector3(v.x, 2, v.y));
        Gizmos.DrawWireCube(pivot, new Vector3(v.x, 2, v.y));
    }
}