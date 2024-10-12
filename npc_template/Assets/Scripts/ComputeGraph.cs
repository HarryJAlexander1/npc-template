using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ComputeGraph : MonoBehaviour
{
    public List<GameObject> GroundSegments;
    public GameObject Ground;
    public Graph LevelGraph;

    // Start is called before the first frame update
    void Start()
    {
        Vector3[] temp = GetGroundTopVertices(GroundSegments);
        LevelGraph = GenerateGraph(temp, 8);
        Debug.Log("Graph Vertices" + LevelGraph.Vertices.Length);
       /* foreach (var vertex in LevelGraph.Vertices) 
        {
            Debug.Log($"Vertex {vertex} neighbours count = " + vertex.Neighbours.Count);
        }*/
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 1654; i < 1655; i++)
        {
            for (int j = 0; j < LevelGraph.Vertices[i].Neighbours.Count; j++)
            {
                Gizmos.DrawSphere(LevelGraph.Vertices[i].Neighbours[j].Position, 0.25f);
            }
        }
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

    private Graph GenerateGraph(Vector3[] vertices, int divisions)
    {
        List<Vertex> interpolatedVertices = new List<Vertex>();
        float vertexInterval = 0;

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

            if (vertexInterval == 0)
                vertexInterval = xInterval;

            for (int divX = 0; divX <= divisions; divX++) 
            {
                for (int divZ = 0; divZ <= divisions; divZ++) 
                {
                    Vertex v = new Vertex(new(minX + (xInterval * divX), transform.localScale.y * 0.5f, minZ + (zInterval * divZ))); // interpolate new vertices
                    // Debug.Log($"Generated vertex pos= {v}");
                    if (!ContainsSimilarVertex(interpolatedVertices, v, 0.00001f))
                    {
                        interpolatedVertices.Add(v);
                    }
                }
            }
        }

        return new Graph(interpolatedVertices.ToArray(), vertexInterval);
    }

    private bool ContainsSimilarVertex(List<Vertex> vertices, Vertex newVertex, float tolerance)
    {
        foreach (var vertex in vertices)
        {
            if (Vector3.Distance(vertex.Position, newVertex.Position) < tolerance)
            {
                return true;  // Similar vertex found
            }
        }
        return false;
    }

    public class Graph 
    {
        public Vertex[] Vertices { get; set; }
        private float VertexInterval { get; set; }
        public Graph(Vertex[] vertices, float vertexInterval) 
        {
            VertexInterval = vertexInterval;
            Vertices = vertices;
            CreateEdges();
        }

        private void CreateEdges() 
        {     
            for (int i = 0; i < Vertices.Length; i++)
            {
                for (int j = 0; j < Vertices.Length; j++)
                {
                    if (i == j) { continue; }

                    if (Vertices[i].IsNeighbour(Vertices[j], VertexInterval))
                    {
                        Vertices[i].AddEdge(Vertices[j]);
                    }
                }
            }
        }
    }

    public class Vertex 
    {
        public Vector3 Position { get; set; }
        public List<Vertex> Neighbours { get; set; }
        public Vertex(Vector3 position) 
        {
            Neighbours = new List<Vertex>();
            Position = position;
        }
        public void AddEdge(Vertex vertexB)
        {
            Neighbours.Add(vertexB);
            //vertexB.Neighbours.Add(this);
        }
        public bool IsNeighbour(Vertex vertexB, float interval)
        {
            var offsets = new[]
            {
                new Vector3(Position.x - interval, Position.y, Position.z - interval),
                new Vector3(Position.x + interval, Position.y, Position.z + interval),
                new Vector3(Position.x + interval, Position.y, Position.z - interval),
                new Vector3(Position.x - interval, Position.y, Position.z + interval),
                new Vector3(Position.x, Position.y, Position.z + interval),
                new Vector3(Position.x, Position.y, Position.z - interval),
                new Vector3(Position.x + interval, Position.y, Position.z),
                new Vector3(Position.x - interval, Position.y, Position.z)
            };

            return offsets.Contains(vertexB.Position);
        }
    }
}
