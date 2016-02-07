using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bl_IconItem : MonoBehaviour {

    [Separator("SETTINGS")]
    public float DestroyIn = 5f;
    [Separator("REFERENCES")]
    public Image TargetGrapihc;
    public Sprite DeathIcon = null;
    public Text InfoText = null;

    private CanvasGroup m_CanvasGroup;
    private Animator Anim;
    private float delay = 0.1f;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        //Get the canvas group or add one if nt have.
        if(GetComponent<CanvasGroup>() != null)
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
        }
        else { m_CanvasGroup = gameObject.AddComponent<CanvasGroup>(); }
        if(GetComponent<Animator>() != null)
        {
            Anim = GetComponent<Animator>();
        }
        if(Anim != null) { Anim.enabled = false; }
        m_CanvasGroup.ignoreParentGroups = true;
        m_CanvasGroup.alpha = 0;
    }

    /// <summary>
    /// When player or the target die,desactive,remove,etc..
    /// call this for remove the item UI from Map
    /// for change to other icon and desactive in certain time
    /// or destroy inmediate
    /// </summary>
    /// <param name="inmediate"></param>
    public void DestroyIcon(bool inmediate)
    {
        if (inmediate)
        {
            Destroy(gameObject);
        }
        else
        {
            //Change the sprite to icon death
            TargetGrapihc.sprite = DeathIcon;
            //destroy in 5 seconds
            Destroy(gameObject, DestroyIn);
        }
    }
    /// <summary>
    /// When player or the target die,desactive,remove,etc..
    /// call this for remove the item UI from Map
    /// for change to other icon and desactive in certain time
    /// or destroy inmediate
    /// </summary>
    /// <param name="inmediate"></param>
    /// <param name="death"></param>
    public void DestroyIcon(bool inmediate,Sprite death)
    {
        if (inmediate)
        {
            Destroy(gameObject);
        }
        else
        {
            //Change the sprite to icon death
            TargetGrapihc.sprite = death;
            //destroy in 5 seconds
            Destroy(gameObject, DestroyIn);
        }
    }
    /// <summary>
    /// Get info to desplay
    /// </summary>
    /// <param name="info"></param>
    public void GetInfoItem(string info)
    {
        if (InfoText == null)
            return;

        InfoText.text = info;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeIcon()
    {
        yield return new WaitForSeconds(delay);
        while(m_CanvasGroup.alpha < 1)
        {
            m_CanvasGroup.alpha += Time.deltaTime * 2;
            yield return null;
        }
        if (Anim != null) { Anim.enabled = true; }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool open = false;
    public void InfoItem()
    {
        open = !open;
        Animator a = GetComponent<Animator>();
        if (open)
        {
            a.SetBool("Open", true);
        }
        else
        {
            a.SetBool("Open", false);
        }
    }

    public void DelayStart(float v) { delay = v; StartCoroutine(FadeIcon()); }
}