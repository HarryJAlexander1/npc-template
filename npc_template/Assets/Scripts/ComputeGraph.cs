using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
public class ComputeGraph : MonoBehaviour
{
    public List<GameObject> GroundSegments;
    public GameObject Ground;
    private Vector3[] GraphVertices;
    // Start is called before the first frame update
    void Start()
    {
        Vector3[] temp = GetGroundTopVertices(GroundSegments);
        GraphVertices = InterpolateVertices(temp, 8);
    }

    void OnDrawGizmos()
    { 
        foreach (var vertex in GraphVertices) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(vertex, 0.25f);
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

    private Vector3[] InterpolateVertices(Vector3[] vertices, int divisions)
    {
        List<Vector3> interpolatedVertices = new List<Vector3>();

        for (int i = 0; i < vertices.Length - 4; i += 4) // each segment will always have 4 'surface' vertices
        {
            float minX = Mathf.Infinity;
            float maxX = -Mathf.Infinity;
            float minZ = Mathf.Infinity;
            float maxZ = -Mathf.Infinity;

            for (int j = i; j < i + 4; j++) // find min and max x and z coordinates per 4 vertices
            {
                if (vertices[j].x < minX)
                    minX = vertices[j].x;
                if (vertices[j].x > maxX)
                    maxX = vertices[j].x;
                if (vertices[j].z < minZ)
                    minZ = vertices[j].z;
                if (vertices[j].z > maxZ)
                    maxZ = vertices[j].z;
            }

            float xInterval = (maxX - minX) / divisions;
            float zInterval = (maxZ - minZ) / divisions;

            for (int divX = 0; divX <= divisions; divX++) 
            {
                for (int divZ = 0; divZ <= divisions; divZ++) 
                {
                    Vector3 v = new Vector3(minX + (xInterval * divX), transform.localScale.y * 0.5f, minZ + (zInterval * divZ));
                    // Debug.Log($"Generated vertex pos= {v}");
                    if (!ContainsSimilarVertex(interpolatedVertices, v, 0.00001f))
                    {
                        interpolatedVertices.Add(v);
                    }
                }            
            }
        }
        return interpolatedVertices.ToArray();
    }

    private bool ContainsSimilarVertex(List<Vector3> vertices, Vector3 newVertex, float tolerance)
    {
        foreach (var vertex in vertices)
        {
            if (Vector3.Distance(vertex, newVertex) < tolerance)
            {
                return true;  // Similar vertex found
            }
        }
        return false;
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
