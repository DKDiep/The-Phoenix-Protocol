using UnityEngine;

public static class bl_MiniMapUtils  {


    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewPoint"></param>
    /// <param name="maxAnchor"></param>
    /// <returns></returns>
    public static Vector3 CalculateMiniMapPosition(Vector3 viewPoint,RectTransform maxAnchor)
    {
        viewPoint = new Vector2((viewPoint.x * maxAnchor.sizeDelta.x) - (maxAnchor.sizeDelta.x * 0.5f),
            (viewPoint.y * maxAnchor.sizeDelta.y) - (maxAnchor.sizeDelta.y * 0.5f));

        return viewPoint;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bl_MiniMap GetMiniMap(int id = 0)
    {
        bl_MiniMap[] allmm = GameObject.FindObjectsOfType<bl_MiniMap>();
        return allmm[id];
    }
}