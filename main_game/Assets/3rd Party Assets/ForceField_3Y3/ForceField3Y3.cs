using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForceField3Y3 : MonoBehaviour
{
    //Effect modes. Can be:
    // skin effect. A baked mesh of the model with the effect applied
    // sphere. A sphere sorrounding the object with the effect applied
    public enum EffectMode
    {
        Skin,
        Sphere
    }

    //Effect activation. Can be:
    // AlwaysOn. The object has the effect activated by default. Nevertheless, the effect can be deactivated by user calls
    // User call. The effect is not active and can be swithed on/off by calling public funtions
    public enum EffectActivation
    {
        AlwaysOn,
        UserCall
    }

    public Material effect;                     //Effect to be applied to the object
    public EffectMode effectMode;               //Effect mode (skin or sphere)
    public EffectActivation effectActivation;   //Effect activation (AlwaysOn or UserCall
    public bool hideObject = false;             //Hides the object when the effect is on and shows it again when is off.

    private GameObject sphere;
    private Mesh theMesh;
    private bool EffectIsOn = false;
    private List<GameObject> allSubMeshes = new List<GameObject>();
    private float sphereRadius;
    private Vector3 sphereCenter;

    //Set the offect on.
    //This public method can be called by third party assets such as PlayMaker
    public void SetEffectOn()
    {
        EffectIsOn = true;
        if (effectMode == EffectMode.Sphere)
        {
            sphere.GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().enabled = false;
        }
        if (effectMode == EffectMode.Skin && theMesh != null)
        {
            GetComponent<Renderer>().enabled = true;
            sphere.GetComponent<Renderer>().enabled = false;
        }
       
        SetObjectVisiblility(!hideObject);
    }

    //Set the effect off.
    //This public method can be called by third party assets such as PlayMaker
    public void SetEffectOff()
    {
        EffectIsOn = false;
        if (effectMode == EffectMode.Sphere)
        {
            sphere.GetComponent<Renderer>().enabled = false;
        }
        if (effectMode == EffectMode.Skin && theMesh != null)
        {
            GetComponent<Renderer>().enabled = false;
        }

        SetObjectVisiblility(!hideObject);
    }

    //Set the effect to apply.
    //This public method can be called by third party assets such as PlayMaker
    public void SetEffect(Material setEffect)
    {
        effect = setEffect;
        sphere.GetComponent<Renderer>().material = effect;
        if (theMesh != null)
            GetComponent<Renderer>().material = effect;

        
    }

    //Set the effect mode to Skin or Sphere.
    //This public method can be called by third party assets such as PlayMaker
    public void SetEffectMode(EffectMode setEffectMode)
    {
        effectMode = setEffectMode;
        if (EffectIsOn)
            SetEffectOn();
    }

    //Set the object visibility when effect is On.
    //This public method can be called by third party assets such as PlayMaker
    public void SetObjectHidden(bool hidden)
    {
        hideObject = hidden;
        if (EffectIsOn)
            SetEffectOn();
        else
            SetEffectOff();
    }

	void Start () {
        //bake a new mesh
        theMesh=CreateEffectMesh();

        GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        GetComponent<Renderer>().receiveShadows = false;
        GetComponent<Renderer>().material = effect;

        sphereRadius = GetComponent<Renderer>().bounds.extents.magnitude;
        sphereCenter = GetComponent<Renderer>().bounds.center;

        //Adds a bounding sphere
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        float diam = 2f * sphereRadius;
        sphere.transform.localScale = new Vector3(diam, diam, diam);
        sphere.transform.position = sphereCenter;
        sphere.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        sphere.GetComponent<Renderer>().receiveShadows = false;
        sphere.GetComponent<Renderer>().material = effect;
        sphere.transform.parent = this.gameObject.transform;

        //Disable effect
        sphere.GetComponent<Renderer>().enabled = false;
        GetComponent<Renderer>().enabled = false;

        if (effectActivation == EffectActivation.AlwaysOn)
        {
            EffectIsOn = false;
            SetEffectOn();
        }
	}

    void SetObjectVisiblility(bool visible)
    {
        foreach (GameObject go in allSubMeshes)
            go.SetActive(visible);
    }

	// Update is called once per frame
	void Update () {
       
	}
    
    public Mesh CreateEffectMesh()
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>(false);

        int totalMeshCount = GetNumberOfMeshes(meshRenderers);

        if (totalMeshCount < 1)
            return null;

        Matrix4x4 localTransform = this.transform.worldToLocalMatrix;
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv1s = new List<Vector2>();
        List<Vector2> uv2s = new List<Vector2>();
        Dictionary<Material, List<int>> subMeshes = new Dictionary<Material, List<int>>();

        Material[] materialsef = new Material[1];
        materialsef[0] = effect;

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    MergeMeshes(filter.sharedMesh, materialsef, localTransform * filter.transform.localToWorldMatrix, vertices, normals, uv1s, uv2s, subMeshes);
                    if (filter.gameObject != this.gameObject)
                        allSubMeshes.Add(filter.gameObject);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        if (normals.Count > 0) 
            mesh.normals = normals.ToArray();
        if (uv1s.Count > 0) 
            mesh.uv = uv1s.ToArray();
        if (uv2s.Count > 0 && uv2s.Count == mesh.vertices.Length)
            mesh.uv2 = uv2s.ToArray();
 
        mesh.subMeshCount = subMeshes.Keys.Count;
        Material[] materials = new Material[subMeshes.Keys.Count];
        int mIdx = 0;
        foreach (Material m in subMeshes.Keys)
        {
            materials[mIdx] = m;
            mesh.SetTriangles(subMeshes[m].ToArray(), mIdx++);
        }

        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            MeshRenderer meshRend = GetComponent<MeshRenderer>();
            if (meshRend == null) meshRend = this.gameObject.AddComponent<MeshRenderer>();
            meshRend.sharedMaterials = materials;

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = this.gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
        }
        return mesh;
    }

    private int GetNumberOfMeshes(MeshRenderer[] renderers)
    {
        int numVertex = 0;
        int numMeshes = 0;
        if (renderers != null && renderers.Length > 0)
        {
            foreach (MeshRenderer meshRenderer in renderers)
            {
                MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    numVertex += filter.sharedMesh.vertexCount;
                    numMeshes++;
                }
            }
        }
        if (numVertex > 65535)
        {
            Debug.Log("Maximum number of vertex reached. Cannot add more meshes!");
            numMeshes = 0;
        }
        return numMeshes;
    }

    private void MergeMeshes(Mesh meshToMerge, Material[] ms, Matrix4x4 transformMatrix, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv1s, List<Vector2> uv2s, Dictionary<Material, List<int>> subMeshes)
    {
        if (meshToMerge == null) return;
        int vertexOffset = vertices.Count;
        Vector3[] vs = meshToMerge.vertices;

        for (int i = 0; i < vs.Length; i++)
        {
            vs[i] = transformMatrix.MultiplyPoint3x4(vs[i]);
        }
        vertices.AddRange(vs);

        Quaternion rotation = Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
        Vector3[] ns = meshToMerge.normals;
        if (ns != null && ns.Length > 0)
        {
            for (int i = 0; i < ns.Length; i++) ns[i] = rotation * ns[i];
            normals.AddRange(ns);
        }

        Vector2[] uvs = meshToMerge.uv;
        if (uvs != null && uvs.Length > 0) 
            uv1s.AddRange(uvs);
        uvs = meshToMerge.uv2;
        if (uvs != null && uvs.Length > 0) 
            uv2s.AddRange(uvs);

        for (int i = 0; i < ms.Length; i++)
        {
            if (i < meshToMerge.subMeshCount)
            {
                int[] ts = meshToMerge.GetTriangles(i);
                if (ts.Length > 0)
                {
                    if (ms[i] != null && !subMeshes.ContainsKey(ms[i]))
                    {
                        subMeshes.Add(ms[i], new List<int>());
                    }
                    List<int> subMesh = subMeshes[ms[i]];
                    for (int t = 0; t < ts.Length; t++)
                    {
                        ts[t] += vertexOffset;
                    }
                    subMesh.AddRange(ts);
                }
            }
        }
    }
}
