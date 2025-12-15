using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject point;
    public GameObject player;
    public List<GameObject> obstacles;
    public Graph graph;
    public int scene;
    public float speed = 1.0f;

    List<Node> nodeStack = new List<Node>();

    List<Transform> transformsOfNodes = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        transformsOfNodes.Clear();
        Node parentNode = null;
        if (scene == 0)
        {
            graph = new Graph(9);
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    addNodeM(i, j);
            addObstacleM(1, 4);
            addObstacleM(1, 6);
            addObstacleM(2, 2);
            addObstacleM(3, 3);
            addObstacleM(5, 5);
            addObstacleM(5, 7);
            addObstacleM(6, 1);
            addObstacleM(6, 4);
            addObstacleM(8, 2);

            parentNode = FindRoute(6, 5, 1, 1);
        } else
        {
            graph = new Graph(18);
            for (int i = 0; i < 18; i++)
                for (int j = 0; j < 18; j++)
                    addNodeM(i, j);
            for (int i = 0; i < 10; i++)
                addObstacleM(1, 7 + i);
            for (int i = 0; i < 11; i++)
                addObstacleM(14, 6 + i);
            for (int i = 0; i < 9; i++)
                addObstacleM(3+i, 5);
            for (int i = 0; i < 12; i++)
                addObstacleM(2+i, 16);

            parentNode = FindRoute(1, 5, 16, 15);
        }
        
        while (parentNode!= null)
        {
            transformsOfNodes.Insert(0,parentNode.transform);
            parentNode = parentNode.parent;
        }

        StartCoroutine(Move());
    }

    private void addNodeM(int y, int x)
    {
        GameObject node = Instantiate(point, new Vector3(x, 0,-y), transform.rotation);
        graph.AddNode(node.GetComponent<Node>());
    }
    private void addObstacleM(int y, int x)
    {
        int ran = new System.Random().Next(0, 6);

        Instantiate(obstacles[ran], new Vector3(x, 0,-y), transform.rotation);
        graph.AddObstacle(y, x);
    }

    private Node FindRoute(int startY,  int startX, int endY, int endX)
    {
        nodeStack.Clear();
        Node startNode = graph.GetNode(startY, startX);
        startNode.curCost = 0;
        startNode.preCost = Mathf.Abs(endY - startY) + Mathf.Abs(endX - startX);
        startNode.visited = true;
        nodeStack.Add(startNode);

        int bestCost = 99;
        Node bestParentNode = null;

        while (nodeStack.Count != 0) {
            Node prebestNode = PredictNode();
            if (prebestNode.preCost == 0 && bestCost>prebestNode.curCost) {
                bestCost = prebestNode.curCost;
                bestParentNode = prebestNode;
            }
                
            if (prebestNode.preCost + prebestNode.curCost > bestCost) continue;

            foreach (Node node in prebestNode.Neighbors) {
                if(!node.visited) {
                    node.parent = prebestNode;
                    node.curCost = prebestNode.curCost + 1;
                    int y = (int)(-node.transform.position.z);
                    int x = (int)(node.transform.position.x);

                    node.preCost = (int)(Mathf.Abs(y - endY) + Mathf.Abs(x - endX));
                    nodeStack.Add(node);
                    node.visited = true;
                }
            }
        }
        
        return bestParentNode;
    }

    private Node PredictNode()
    {
        Node prebestNode = null;
        int prebestCost = 100;
        foreach(Node node in nodeStack) {
            if (node.curCost + node.preCost < prebestCost) {
                prebestNode = node;
                prebestCost = node.curCost+node.preCost;
            }  
        }

        nodeStack.Remove(prebestNode);

        return prebestNode;
    }

    private Vector3 GetPosition(float t,List<Transform> transforms)
    {
        if (t > 1) t = 1;
        float y = 0;
        float x = 0;

        int n = transforms.Count-1;
        for (int i=0;i<=n;i++)
        {
            y += GetCombination(n,i) * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i) * transforms[i].position.z;
            x += GetCombination(n, i) * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i) * transforms[i].position.x;
        }

        //print("y : " + y + ", x : " + x+", t :"+t);
        return new Vector3(x,0,y);
    }
    private long GetCombination(int n, int r)
    {
        System.Numerics.BigInteger result = 1;

        for(int i = n;i>n-r;i--)
        {
            result *= i;
        }
        for(int i=2;i<=r;i++)
        {
            result /= i;
        }
        return (long)result;
    }

    private IEnumerator Move()
    {
        player.GetComponent<Animator>().SetFloat("Speed_f", 0.5f);
        while (transformsOfNodes.Count>0)
        {
            List<Transform> dividedTransforms = DivideTransforms();
            int n = 100*dividedTransforms.Count;
            for(int i=0;i<n;i++)
            {
                player.transform.position = GetPosition((float)i / n,dividedTransforms);
                Vector3 nextPosition = GetPosition((float)(i + 1) / n,dividedTransforms);
                Vector3 direction = nextPosition - player.transform.position;
                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                player.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                yield return new WaitForSeconds(0.01f/speed);
            }
        }

        player.GetComponent<Animator>().SetFloat("Speed_f", 0.0f);
    }

    private List<Transform> DivideTransforms()
    {
        bool curve = false;
        if(transformsOfNodes.Count <4)
        {
            List<Transform> tmp = new List<Transform>();
            foreach (Transform t in transformsOfNodes)
                tmp.Add(t);
            transformsOfNodes.Clear();
            return tmp;
        } else
        {
            List<Transform> tmp = new List<Transform>();
            Vector3 dir1 = transformsOfNodes[1].position - transformsOfNodes[0].position;
            Vector3 dir2 = transformsOfNodes[2].position - transformsOfNodes[1].position;
            tmp.Add(transformsOfNodes[0]);
            tmp.Add(transformsOfNodes[1]);
            tmp.Add(transformsOfNodes[2]);
            transformsOfNodes.RemoveAt(0);
            if (dir1.x == dir2.x && dir1.z == dir2.z)
            {
                dir2 = transformsOfNodes[2].position - transformsOfNodes[1].position;
                while(dir1.x == dir2.x && dir1.z == dir2.z)
                {
                    tmp.Add(transformsOfNodes[2]);
                    transformsOfNodes.RemoveAt(0);
                    dir2 = transformsOfNodes[2].position - transformsOfNodes[1].position;
                }
                tmp.RemoveAt(tmp.Count-1);
                return tmp;
            }
            else
            {
                dir1 = dir2;
                dir2 = transformsOfNodes[2].position - transformsOfNodes[1].position;
                while (dir1.x != dir2.x || dir1.z != dir2.z)
                {
                    tmp.Add(transformsOfNodes[2]);
                    transformsOfNodes.RemoveAt(0);
                    dir1 = dir2;
                    dir2 = transformsOfNodes[2].position - transformsOfNodes[1].position;
                }
                transformsOfNodes.RemoveAt(0);
                return tmp;
            }
        }
    }

}
