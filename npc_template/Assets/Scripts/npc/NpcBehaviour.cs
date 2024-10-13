using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathFinding;
using System.IO;
using Unity.VisualScripting;
using Unity.Collections.LowLevel.Unsafe;

public class NpcBehaviour : MonoBehaviour
{
    public float Speed = 1.0f;
    private int CurrentStep = 0;
    public Vertex SourceVertex = null;
    private List<Vertex> Path = new List<Vertex>();
    private Animator Animator;
    public Queue<Vertex> Destinations = new Queue<Vertex>();
    private void Start()
    {
        Animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        AnimationControl();
        Movement();
    }

    private void AnimationControl() 
    {
        if (CurrentStep == Path.Count)
            Animator.SetBool("IsWalking", false);
        else 
        {
            Animator.SetBool("IsWalking", true);
        }
    }

    private void TraverseGraph()
    {
        if (Path.Count > 0) // if a path is loaded
        {
            if (IsApproximatelyAtNode(Path[CurrentStep], 0.1f) && CurrentStep < Path.Count - 1)
                CurrentStep++;
   
            MoveToNode(Path[CurrentStep]);

            if (CurrentStep == Path.Count - 1) // npc has fully traversed path, time to reset
            {
                SourceVertex = Path[CurrentStep];
                Path.Clear();
                CurrentStep = Path.Count;
            } 
        }      
    }

    private void Movement()
    {
        if (Destinations.Count != 0 && Path.Count == 0)
        {
            FindShortestPath(SourceVertex, Destinations.Dequeue());
        }

        TraverseGraph();
    }

    public void FindShortestPath(Vertex source, Vertex destination)
    {
        // Initialize the queue for BFS
        Queue<Vertex> queue = new Queue<Vertex>();
        // Dictionary to store the parent of each Graph.Vertex for reconstructing the path
        Dictionary<Vertex, Vertex> parent = new Dictionary<Vertex, Vertex>();

        // Enqueue the source Graph.Vertex
        queue.Enqueue(source);
        parent[source] = null;

        while (queue.Count > 0)
        {
            Vertex current = queue.Dequeue();

            // Check if the current Graph.Vertex is the destination
            if (current == destination)
            {
                // Reconstruct the path from source to destination
                Path = ReconstructPath(parent, source, destination);
                return;
            }

            // Enqueue neighbors if not already visited
            foreach (Vertex neighbor in current.Neighbours)
            {
                if (!parent.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    parent[neighbor] = current;
                }
            }
        }

        // If no path is found
        Path = null;
    }

    List<Vertex> ReconstructPath(Dictionary<Vertex, Vertex> parent, Vertex source, Vertex destination)
    {
        List<Vertex> path = new List<Vertex>();
        Vertex current = destination;

        // Reconstruct the path from destination to source
        while (current != null)
        {
            path.Insert(0, current); // Insert at the beginning to reverse the order
            current = parent[current];
        }

        var count = 0;
        foreach (Vertex v in path)
        {
            count++;
        }
        return path;
    }
    private bool IsApproximatelyAtNode(Vertex node, float threshold)
    {
        if (Vector3.Distance(new(node.Position.x, 0, node.Position.z), new(transform.position.x, 0, transform.position.z)) < threshold)
        {
            return true;
        }
        return false;
    }

    private void MoveToNode(Vertex node)
    {
        Vector3 direction = node.Position - transform.position;
        transform.LookAt(node.Position);
        // Normalize the direction to get a unit vector
        direction.Normalize();

        // Move towards the target
        transform.position += direction * Speed * Time.deltaTime;
    }
}
