using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour {
    public Material material;

    public Vector3[] border;

    public void setBorder(Vector3[] border) {
        this.border = border;
    }

    void Start() {
        Vector3[] vertices = new Vector3[border.Length + 1];
        vertices[0] = new Vector3(0,0);
        for(int vertIndex = 0; vertIndex < border.Length; vertIndex++) {
            vertices[vertIndex+1] = border[vertIndex];
        }

        int[] triangles = new int[border.Length * 3];
        for(int tri = 0;tri < border.Length; tri++) {
            triangles[(tri*3)+0] = 0;
            triangles[(tri*3)+1] = tri+1;
            triangles[(tri*3)+2] = tri+2;
        }
        triangles[triangles.Length-1] = 1; // Fix the wrap-around

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        transform.localScale = new Vector3(1,1,1);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
    }

    // Update is called once per frame
    void Update() {
        Vector3[] vertices = new Vector3[border.Length + 1];
        vertices[0] = new Vector3(0,0);
        //Debug.Log(transform.position);
        for(int vertIndex = 0; vertIndex < border.Length; vertIndex++) {
            vertices[vertIndex+1] = border[vertIndex];//transform.InverseTransformPoint(border[vertIndex]);
        }

        int[] triangles = new int[border.Length * 3];
        for(int tri = 0;tri < border.Length; tri++) {
            triangles[(tri*3)+0] = 0;
            triangles[(tri*3)+1] = tri+1;
            triangles[(tri*3)+2] = tri+2;
        }
        triangles[triangles.Length-1] = 1; // Fix the wrap-around

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        transform.localScale = new Vector3(1,1,1);
        GetComponent<MeshFilter>().mesh = mesh;
    }


}
