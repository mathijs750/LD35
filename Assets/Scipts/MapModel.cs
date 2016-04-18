using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public enum edgeMode
{
    Normal, Exit, OneWay, Blocked
}

[System.Serializable]
public struct nodePair
{
    public string NodeA;
    public string NodeB;

    public nodePair(string A, string B)
    {
        NodeA = A;
        NodeB = B;
    }

    public override string ToString()
    {
        return NodeA + "-" + NodeB;
    }

    #region Comparison overrides
    public override bool Equals(System.Object obj)
    {
        return obj is nodePair && this == (nodePair)obj;
    }
    public override int GetHashCode()
    {
        return NodeA.GetHashCode() ^ NodeB.GetHashCode();
    }
    public static bool operator ==(nodePair x, nodePair y)
    {
        return x.NodeA == y.NodeA && x.NodeB == y.NodeB;
    }
    public static bool operator !=(nodePair x, nodePair y)
    {
        return !(x == y);
    }
    #endregion
}


[System.Serializable]
public struct nodeEdge
{
    public nodePair pairA;
    public nodePair pairB;
    public int Cost;
    public edgeMode Mode;
    public int Traffic;
}

[System.Serializable]
public struct node
{
    public string ID;
    public Vector2 Position;
    public string[] Neighbors;
}

[System.Serializable]
public struct Map
{
    public node[] Nodes;
    public nodeEdge[] Edges;
}

public class Graph
{
    public Dictionary<string, string[]> nodes = new Dictionary<string, string[]>();
    public Dictionary<nodeEdge, int> specialEdges = new Dictionary<nodeEdge, int>();
    public Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();

    public string[] Neighbors(string id)
    {
        return nodes[id];
    }

    public int Cost(string a, string b)
    {
    /*   var edgeA = new nodeEdge(a, b);
        var edgeB = new nodeEdge(b, a);

        if (specialEdges.ContainsKey(edgeA))
        {
            return specialEdges[edgeA];
        }
        else if (specialEdges.ContainsKey(edgeB))
        {
            return specialEdges[edgeB];
        }*/
        return 1;
    }

}

public struct MapAgent
{
    public string[] MovementQueue;
    public int Ammount;

    public MapAgent(string[] queue, int ammount)
    {
        MovementQueue = queue;
        Ammount = ammount;
    }
}

public class MapModel : MonoBehaviour
{
    [SerializeField]
    private string start, goal;
    private List<GameObject> _sprites;
    [SerializeField]
    private GameObject sprite;
    [SerializeField]
    TextAsset jsonData;

    Dictionary<string, string> cameFrom = new Dictionary<string, string>();
    Dictionary<string, int> costSoFar = new Dictionary<string, int>();
    PriorityQueue<int, string> frontier = new PriorityQueue<int, string>();
    List<string> pathList = new List<string>();
    Graph g = new Graph();

    public void Start()
    {
        g.nodes = new Dictionary<string, string[]>();
        g.nodePositions = new Dictionary<string, Vector2>();

        var mapData = JsonUtility.FromJson<Map>(jsonData.text);
        foreach (node node in mapData.Nodes)
        {
            g.nodes.Add(node.ID, node.Neighbors);
            g.nodePositions.Add(node.ID, node.Position);
        }

        List<nodeEdge> edges = new List<nodeEdge>();
        
        foreach (var node in g.nodes)
        {
            foreach (var neigbour in node.Value)
            {
                Debug.Log(node.Key + "-" + neigbour);
                nodeEdge edge = new nodeEdge();
                edge.pairA = new nodePair(node.Key, neigbour);
                edge.pairB = new nodePair(neigbour, node.Key);
                edge.Cost = 2;
                edge.Mode = edgeMode.Normal;

                if (edges.Count == 0) { edges.Add(edge); }

                foreach(var existingEdge in edges)
                {
                    if (existingEdge.pairA != edge.pairA  && existingEdge.pairB != edge.pairA)
                    {
                        edges.Add(edge);
                    }
                }
            }    
        }
        // g.

        Debug.LogWarning(edges.Count);
        Debug.LogWarning(JsonUtility.ToJson(edges.ToArray()));

        /*

        g.specialEdges = new Dictionary<nodeEdge, int>
        {
            {new nodeEdge( "A","F" ), 50 },
            {new nodeEdge("D","F"), 50 },
            {new nodeEdge("F","A" ), 50 },
            {new nodeEdge("F","D"), 50 }
        };*/


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
