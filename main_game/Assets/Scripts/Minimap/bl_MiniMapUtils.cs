/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: MiniMap helper functions
*/

using UnityEngine;

public static class bl_MiniMapUtils  
{

    public static Vector3 CalculateMiniMapPosition(Vector3 viewPoint,RectTransform maxAnchor)
    {
        viewPoint = new Vector2((viewPoint.x * maxAnchor.sizeDelta.x) - (maxAnchor.sizeDelta.x * 0.5f),
            (viewPoint.y * maxAnchor.sizeDelta.y) - (maxAnchor.sizeDelta.y * 0.5f));

        return viewPoint;
    }

    public static bl_MiniMap GetMiniMap(int id = 0)
    {
        bl_MiniMap[] allmm = GameObject.FindObjectsOfType<bl_MiniMap>();
        return allmm[id];
    }
}