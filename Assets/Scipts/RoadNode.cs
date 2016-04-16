using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct nodeEdge
{
    public string A;
    public string B;

    public nodeEdge(string A, string B)
    {
        this.A = A;
        this.B = B;
    }
}

[System.Serializable]
public struct node
{
    public string ID;
    public Vector2 Position;
    public string[] Neighbors;
}

[System.Serializable]
public struct map
{
    public node[] Nodes;
}

public class Graph
{
    public Dictionary<string, string[]> edges = new Dictionary<string, string[]>();
    public Dictionary<nodeEdge, int> specialEdges = new Dictionary<nodeEdge, int>();
    public Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();

    public string[] Neighbors(string id)
    {
        return edges[id];
    }

    public int Cost(string a, string b)
    {
        var edgeA = new nodeEdge(a, b);
        var edgeB = new nodeEdge(b, a);

        if (specialEdges.ContainsKey(edgeA))
        {
            return specialEdges[edgeA];
        }
        else if (specialEdges.ContainsKey(edgeB))
        {
            return specialEdges[edgeB];
        }
        return 1;
    }
}


public class RoadNode : MonoBehaviour
{
    [SerializeField]
    private string start, goal;
    private List<GameObject> _sprites;
    [SerializeField]
    private GameObject sprite;

    Dictionary<string, string> cameFrom = new Dictionary<string, string>();
    Dictionary<string, int> costSoFar = new Dictionary<string, int>();
    PriorityQueue<int, string> frontier = new PriorityQueue<int, string>();
    List<string> pathList = new List<string>();
    Graph g = new Graph();

    public void Start()
    {

        g.edges = new Dictionary<string, string[]>
            {
            { "A", new [] { "C" , "F"} },
            { "B", new [] { "C" } },
            { "C", new [] { "A","B", "E", "F" } },
            { "D", new [] { "C" , "F"} },
            { "E", new [] { "C" } },
            { "F", new [] { "A", "C", "D", "G" } },
            { "G", new [] { "D", "F" } }
        };

        g.nodePositions = new Dictionary<string, Vector2>
        {
            {"A", new Vector2(10,10) },
            {"B", new Vector2(10,30) },
            {"C", new Vector2(20,20) },
            {"D", new Vector2(40,20) },
            {"E", new Vector2(50,40) },
            {"F", new Vector2(30,10) },
            {"G", new Vector2(60,10) },
        };

        g.specialEdges = new Dictionary<nodeEdge, int>
        {
            {new nodeEdge( "A","F" ), 50 },
            {new nodeEdge("D","F"), 50 },
            {new nodeEdge("F","A" ), 50 },
            {new nodeEdge("F","D"), 50 }
        };

        _sprites = new List<GameObject>();
       foreach (var node in g.nodePositions)
        {
            var tmp = Instantiate(sprite, node.Value, Quaternion.identity) as GameObject;
            _sprites.Add(tmp);
            tmp.transform.parent = gameObject.transform;
        }


    }

    public int Heuristic(Vector2 a, Vector2 b)
    {
        return Mathf.FloorToInt(Vector2.Distance(a, b));
    }

    string[] Search(Graph graph, string start, string goal)
    {
        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costSoFar[start] = 0;
        pathList.Clear();
        pathList.Add(start);

        while (!frontier.IsEmpty)
        {
            var current = frontier.Dequeue();
            pathList.Add(current);

            if (current == goal)
            {
                break;
            }

            Debug.Log("AT: " + current);
            foreach (var next in graph.Neighbors(current))
            {
                int newCost = costSoFar[current] + graph.Cost(current, next);
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority = newCost + Heuristic(graph.nodePositions[goal], graph.nodePositions[next]);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
        return pathList.ToArray();
    }

}
