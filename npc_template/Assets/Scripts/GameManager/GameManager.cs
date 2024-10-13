using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathFinding;

public class GameManager : MonoBehaviour
{
    public Graph Graph = null;
    public GameObject Npc;
    public int NpcAmount = 200;
    public void SpawnNPC() // spawn npc at random vertex in graph
    {
        for (int npcNumber = 0; npcNumber < NpcAmount; npcNumber++) 
        {
            int randomStart = Random.Range(0, Graph.Vertices.Length);
            var npc = Instantiate(Npc, Graph.Vertices[randomStart].Position, Quaternion.identity);
            npc.GetComponent<NpcBehaviour>().SourceVertex = Graph.Vertices[randomStart];

            for (int npcDestinations = 0; npcDestinations < 50; npcDestinations++)
            {
                int randomEnd = Random.Range(0, Graph.Vertices.Length);
                npc.GetComponent<NpcBehaviour>().Destinations.Enqueue(Graph.Vertices[randomEnd]);
            }
        }
    }
}
