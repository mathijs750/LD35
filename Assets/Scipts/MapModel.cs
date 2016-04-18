using UnityEngine;
using System.Collections.Generic;



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

    public nodePair Reversed()
    {
        return new nodePair(NodeB, NodeA);
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
    public Dictionary<string, string[]> nodes;
    public Dictionary<string, Vector2> nodePositions;
    public Dictionary<nodePair, nodeEdge> edges;

    public string[] Neighbors(string id)
    {
        return nodes[id];
    }

    public int Cost(string a, string b)
    {
        nodePair checkPair = new nodePair(a, b);
        if (edges.ContainsKey(checkPair))
        {
            return edges[checkPair].Cost;
        }
        else if (edges.ContainsKey(checkPair.Reversed()))
        {
            return edges[checkPair.Reversed()].Cost;
        }
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

    // search variables
    Dictionary<string, string> cameFrom = new Dictionary<string, string>();
    Dictionary<string, int> costSoFar = new Dictionary<string, int>();
    PriorityQueue<int, string> frontier = new PriorityQueue<int, string>();
    Graph graph;
    List<string> pathList = new List<string>();


    public void Start()
    {
        graph = new Graph();
        graph.nodes = new Dictionary<string, string[]>();
        graph.nodePositions = new Dictionary<string, Vector2>();
        graph.edges = new Dictionary<nodePair, nodeEdge>();

        Map mapData = JsonUtility.FromJson<Map>(jsonData.text);

        foreach (node node in mapData.Nodes)
        {
            graph.nodes.Add(node.ID, node.Neighbors);
            graph.nodePositions.Add(node.ID, node.Position);
        }
        foreach (nodeEdge edge in mapData.Edges)
        {
            graph.edges.Add(edge.pairA, edge);
        }

        Debug.Log(Search(graph, start, goal).ToString());
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

        if (!graph.nodes.ContainsKey(start) || !graph.nodes.ContainsKey(goal))
        {
            return pathList.ToArray();
        }

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
