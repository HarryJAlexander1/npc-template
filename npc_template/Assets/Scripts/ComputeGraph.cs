using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComputeGraph : MonoBehaviour
{
    public List<GameObject> GroundSegments;
    // Start is called before the first frame update
    void Start()
    {
        var output = GetGroundTopVertices(GroundSegments);

        foreach (var item in output) 
        {
            Debug.Log(item);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3[] GetGroundTopVertices(List<GameObject> GroundMeshSegments) 
    {
        Vector3[] groundMeshVertices = new Vector3[GroundMeshSegments.Count * 24]; // cube mesh has 24 vertices

        for (int i = 0; i < GroundMeshSegments.Count; i++) 
        {
            Mesh mesh = GroundMeshSegments[i].GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;

            for (int j = 0; j < vertices.Length; j++) 
            {
                Vector3 worldPos = GroundMeshSegments[i].transform.TransformPoint(vertices[j]);
                groundMeshVertices[j + (i * 24)] = worldPos;
            }
        }
        return groundMeshVertices.Where(x => x.y == (transform.localScale.y * 0.5f)).ToArray();
    }

    private class Graph 
    {
        List<Vertex> Vertices { get; set; }
        public Graph(List<Vertex> vertices) 
        {
            Vertices = vertices;
        }
    }

    private class Vertex 
    {
        Vector3 Position { get; set; }
        List<Vertex> Neighbours { get; set; }
        public Vertex(Vector3 position) 
        {
            Position = position;
        }
    }
}
