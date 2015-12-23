using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sampleGUI : MonoBehaviour {

    public List<GameObject> theObject;
    ForceField3Y3 forceScript;
    public List<Material> theEfects;

    public List<float> delta;
    public List<float> rotateSpeed;
    public List<string> models;

    private int currentEffect = 0;
    private int currentObject = 0;
    private bool isOn = false;
    private ForceField3Y3.EffectMode effectMode = ForceField3Y3.EffectMode.Skin;
    private bool isHidden=false;
    private bool antHidden=false;
	// Use this for initialization
	void Start () {
        SetGameObject(currentObject);
	}
	
	// Update is called once per frame
	void Update () {
        if(isOn)
            theEfects[currentEffect].SetFloat("_Offset", delta[currentObject]);
        theObject[currentObject].transform.Rotate(Vector3.up * Time.deltaTime * rotateSpeed[currentObject]);

        if (effectMode == ForceField3Y3.EffectMode.Sphere)
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(0,1,-14), Time.deltaTime*20f);
        else
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(0, 1, -10), Time.deltaTime * 20f);
	}

    void OnGUI()
    {
        if (!isOn)
        {
            if (GUI.Button(new Rect(10, 10, 150, 30), "Set Effect On"))
            {
                forceScript.SetEffectOn();
                isOn = true;
            }
        }

        if (isOn)
        {
            if (GUI.Button(new Rect(10, 10, 150, 30), "Set Effect Off"))
            {
                forceScript.SetEffectOff();
                isOn = false;
            }
        }

        if (effectMode == ForceField3Y3.EffectMode.Sphere)
        {
            if (GUI.Button(new Rect(10, 50, 150, 30), "Mode Skin"))
            {
                forceScript.SetEffectMode(ForceField3Y3.EffectMode.Skin);
                effectMode = ForceField3Y3.EffectMode.Skin;
            }
        }

        if (effectMode == ForceField3Y3.EffectMode.Skin)
        {
            if (GUI.Button(new Rect(10, 50, 150, 30), "Mode Sphere"))
            {
                forceScript.SetEffectMode(ForceField3Y3.EffectMode.Sphere);
                effectMode = ForceField3Y3.EffectMode.Sphere;
            }
        }

        isHidden = GUI.Toggle(new Rect(10, 100, 150, 30), isHidden, "Hide Object");
        if (isHidden != antHidden)
        {
            antHidden = isHidden;
            forceScript.SetObjectHidden(isHidden);
        }

        if (GUI.Button(new Rect(200, 10, 150, 30), "<< Prev"))
        {
            currentEffect += theEfects.Count - 1;
            currentEffect %= theEfects.Count;
            forceScript.SetEffect(theEfects[currentEffect]);
        }

        if (GUI.Button(new Rect(370, 10, 150, 30), "Next >>"))
        {
            currentEffect ++;
            currentEffect %= theEfects.Count;
            forceScript.SetEffect(theEfects[currentEffect]);
        }
        GUI.Label(new Rect(300, 50, 200, 50), "Current Active Effect: " + (currentEffect + 1) + " of 30");

        if (GUI.Button(new Rect(550, 10, 150, 30), "<< Model"))
        {
            currentObject += theObject.Count - 1;
            currentObject %= theObject.Count;
            SetGameObject(currentObject);
        }

        if (GUI.Button(new Rect(720, 10, 150, 30), "Model >>"))
        {
            currentObject++;
            currentObject %= theObject.Count;
            SetGameObject(currentObject);
        }
        GUI.Label(new Rect(650, 50, 200, 50), "Current Model: " + models[currentObject]);
    }

    void ActivateGo(GameObject go)
    {
        forceScript = theObject[currentObject].GetComponent<ForceField3Y3>();
        forceScript.SetEffect(theEfects[currentEffect]);
        go.SetActive(true);
        forceScript.SetEffectMode(effectMode);
        forceScript.SetObjectHidden(isHidden);
        if(isOn)
            forceScript.SetEffectOn();
    }

    void DeactivateGo(GameObject go)
    {
        go.SetActive(false);
    }

    void SetGameObject(int numGo)
    {
        for (int i = 0; i < theObject.Count; i++)
        {
            if (i != numGo)
                DeactivateGo(theObject[i]);
        }
        ActivateGo(theObject[numGo]);
    }
}
