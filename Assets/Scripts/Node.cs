using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] private List<Node> neighbors;
    [SerializeField] private bool obstacle = false;
    public bool visited = false;

    public int curCost = 0;
    public int preCost = 0;

    public Node parent;

    public List<Node> Neighbors
    {
        get { return neighbors; }
        set { neighbors = value; }
    }

    public bool Obstacle
    {
        get { return obstacle; }
        set { obstacle = value; }
    }
}
