using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]


public class CombineMesh : MonoBehaviour {

  [SerializeField ] Material field;
    void Start() {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.active = false;
            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.active = true;

        this.gameObject.AddComponent<MeshCollider>();
        Forcefield force = this.gameObject.AddComponent<Forcefield>();
        force.CollisionEnter = true;
        force.Field = this.gameObject;
        this.gameObject.GetComponent<Renderer>().material = field;
    }
}