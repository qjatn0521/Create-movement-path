using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Graph
{
    private List<Node> nodes = new List<Node>();
    int length;

    public void AddNode(Node node)
    {
        if (nodes.Count % length != 0)
        {
            nodes[nodes.Count - 1].Neighbors.Add(node);
            node.Neighbors.Add(nodes[nodes.Count - 1]);
        }
        if (nodes.Count >= length)
        {
            nodes[nodes.Count - length].Neighbors.Add(node);
            node.Neighbors.Add(nodes[nodes.Count - length]);
        }
        
        nodes.Add(node);
    }

    public void AddObstacle(int y, int x)
    {
        int num = y * length + x;
        Node node = nodes[num];
        node.Neighbors.Clear();
        node.Obstacle = true;
        if (x % length != 0) nodes[num - 1].Neighbors.Remove(node);
        if (x % length != length-1) nodes[num + 1].Neighbors.Remove(node);
        if (y > 0) nodes[num - length].Neighbors.Remove(node);
        if (y < length-1) nodes[num + length].Neighbors.Remove(node);
    }

    public Node GetNode(int y, int x)
    {
        return nodes[y * length + x];
    }

    public Graph(int length)
    {
        this.length = length;
    }
}
