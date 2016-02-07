using UnityEngine;
using UnityEngine.UI;

public class bl_MaskHelper : MonoBehaviour {

    public Sprite MiniMapMask = null;
    public Sprite WorldMapMask = null;
    [Space(5)]
    public Image Background = null;
    public Sprite MiniMapBackGround = null;
    public Sprite WorldMapBackGround = null;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        m_image.sprite = MiniMapMask;
    }


    private Image _image = null;
    private Image m_image
    {
        get
        {
            if (_image == null)
            {
                _image = this.GetComponent<Image>();
            }
            return _image;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="full"></param>
    public void OnChange(bool full = false)
    {
        if (full)
        {
            m_image.sprite = WorldMapMask;
            if (Background != null) { Background.sprite = WorldMapBackGround; }
        }
        else
        {
            m_image.sprite = MiniMapMask;
            if (Background != null) { Background.sprite = MiniMapBackGround; }
        }
    }
}