/*
    2015-2016 Team Pyrolite
    Project "Sky Base"
    Authors: Marc Steene
    Description: MiniMap Item information
*/

using UnityEngine;

public class bl_MMItemInfo
{
    public Vector3 Position;
    public Transform Target;
    public float Size = 12;
    public Color Color = new Color(1, 1, 1, 0.95f);
    public bool Interactable = false;
    public Sprite Sprite;
    
    public bl_MMItemInfo(Vector3 position)
    {
        Position = position;
    }

    public bl_MMItemInfo(Transform target)
    {
        Target = target;
    }
}