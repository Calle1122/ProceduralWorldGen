using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RGBuilder : MonoBehaviour
{
    public int width, height;
    public int nodeSeparationDistance;
    public RGNode startNode;
    public List<RGNode> nodes = new List<RGNode>();
    public List<RGNode> poiNodes = new List<RGNode>();
    
    private RGNode[,] _grid;
    private List<Vector2Int> _toCollapse = new List<Vector2Int>();

    private Vector2Int[] _offsets;

    private void Awake()
    {
        _offsets = new Vector2Int[]
        {
            new Vector2Int(0, nodeSeparationDistance), // Up
            new Vector2Int(0, -nodeSeparationDistance), // Down
            new Vector2Int(nodeSeparationDistance, 0), // Right
            new Vector2Int(-nodeSeparationDistance, 0) // Left
        };
    }
    
    private void Start()
    {
        _grid = new RGNode[width, height];

        int x = width / 2;
        int y = height / 2;
        
        _grid[x, y] = startNode;
        GameObject newNode = Instantiate(_grid[x, y].prefab, new Vector3(x, 0, y), Quaternion.identity);
        
        CollapseWorld();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void CollapseWorld()
    {
        _toCollapse.Clear();
        
        _toCollapse.Add(new Vector2Int(width/2, height/2 + 1));

        while (_toCollapse.Count > 0)
        {
            int x = _toCollapse[0].x;
            int y = _toCollapse[0].y;

            List<RGNode> potentialNodes = new List<RGNode>(nodes);
            bool addedPoiNodes = false;

            Dictionary<int, Vector2Int> uncollapsedNeighborDictionary = new Dictionary<int, Vector2Int>();
            
            while (_grid[x, y] == null)
            {
                for (int i = 0; i < _offsets.Length; i++)
                {
                    Vector2Int neighbor = new Vector2Int(x + _offsets[i].x, y + _offsets[i].y);

                    if (IsInsideGrid(neighbor))
                    {
                        RGNode neighborNode = _grid[neighbor.x, neighbor.y];

                        // Neighbor has been collapsed
                        if (neighborNode != null)
                        {
                            switch (i)
                            {
                                case 0:
                                    WhittleNodes(potentialNodes, neighborNode.ReturnConnections(), RGConnection.Up, addedPoiNodes);
                                    break;
                                case 1:
                                    WhittleNodes(potentialNodes, neighborNode.ReturnConnections(), RGConnection.Down, addedPoiNodes);
                                    break;
                                case 2:
                                    WhittleNodes(potentialNodes, neighborNode.ReturnConnections(), RGConnection.Right, addedPoiNodes);
                                    break;
                                case 3:
                                    WhittleNodes(potentialNodes, neighborNode.ReturnConnections(), RGConnection.Left, addedPoiNodes);
                                    break;
                            }
                        }
                        // Neighbor has not yet collapsed
                        else
                        {
                            if (!_toCollapse.Contains(neighbor))
                            {
                                if (!uncollapsedNeighborDictionary.ContainsKey(i))
                                {
                                    uncollapsedNeighborDictionary.Add(i, neighbor);
                                }
                            }
                        }
                    }

                    //Neighbor is outside of grid
                    else
                    {
                        List<string> outsideOfGridConnections = new List<string>() { "None" };
                        
                        switch (i)
                        {
                            case 0:
                                WhittleNodes(potentialNodes, outsideOfGridConnections, RGConnection.Up, addedPoiNodes);
                                break;
                            case 1:
                                WhittleNodes(potentialNodes, outsideOfGridConnections, RGConnection.Down, addedPoiNodes);
                                break;
                            case 2:
                                WhittleNodes(potentialNodes, outsideOfGridConnections, RGConnection.Right, addedPoiNodes);
                                break;
                            case 3:
                                WhittleNodes(potentialNodes, outsideOfGridConnections, RGConnection.Left, addedPoiNodes);
                                break;
                        }
                    }
                }

                if (potentialNodes.Count < 1 && addedPoiNodes == false)
                {
                    // Add poiNodes to pool
                    potentialNodes.AddRange(poiNodes);
                    addedPoiNodes = true;
                }
                //No viable option
                else if (potentialNodes.Count < 1 && addedPoiNodes == true)
                {
                    _grid[x, y] = poiNodes[0];
                }
                else
                {
                    _grid[x, y] = potentialNodes[Random.Range(0, potentialNodes.Count)];
                }
            }

            GameObject newNode = Instantiate(_grid[x, y].prefab, new Vector3(x, 0, y), Quaternion.identity);
            _toCollapse.RemoveAt(0);

            List<string> nodeConnections = _grid[x, y].ReturnConnections();

            foreach (var nodeConnection in nodeConnections)
            {
                switch (nodeConnection)
                {
                    case "Up":
                        if (uncollapsedNeighborDictionary.TryGetValue(0, out Vector2Int valUp))
                        {
                            _toCollapse.Add(valUp);
                        }
                        break;
                    
                    case "Down":
                        if (uncollapsedNeighborDictionary.TryGetValue(1, out Vector2Int valDown))
                        {
                            _toCollapse.Add(valDown);
                        }
                        break;
                    
                    case "Right":
                        if (uncollapsedNeighborDictionary.TryGetValue(2, out Vector2Int valRight))
                        {
                            _toCollapse.Add(valRight);
                        }
                        break;
                    
                    case "Left":
                        if (uncollapsedNeighborDictionary.TryGetValue(3, out Vector2Int valLeft))
                        {
                            _toCollapse.Add(valLeft);
                        }
                        break;
                }
            }
        }
    }

    private void WhittleNodes(List<RGNode> potentialNodes, List<string> neighborConnections, RGConnection neighborPosition, bool addedPoiNodes)
    {
        switch (neighborPosition)
        {
            case RGConnection.Up:

                if (neighborConnections.Contains("None") && !addedPoiNodes)
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (potentialNodes[i].ReturnConnections().Contains("Up"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                
                // If neighbor is above the current tile and it connects down -> keep all possible tiles that connect up
                if (neighborConnections.Contains("Down"))
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (!potentialNodes[i].ReturnConnections().Contains("Up"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                // Else do the opposite
                else
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (potentialNodes[i].ReturnConnections().Contains("Up"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                
                break;
            
            case RGConnection.Down:
                
                if (neighborConnections.Contains("None") && !addedPoiNodes)
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (potentialNodes[i].ReturnConnections().Contains("Down"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                
                // If neighbor is below the current tile and it connects up -> keep all possible tiles that connect down
                if (neighborConnections.Contains("Up"))
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (!potentialNodes[i].ReturnConnections().Contains("Down"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                // Else do the opposite
                else
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (potentialNodes[i].ReturnConnections().Contains("Down"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                
                break;
            
            case RGConnection.Right:
                
                if (neighborConnections.Contains("None") && !addedPoiNodes)
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (potentialNodes[i].ReturnConnections().Contains("Right"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                
                // If neighbor is to the right of the current tile and it connects left -> keep all possible tiles that connect right
                if (neighborConnections.Contains("Left"))
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (!potentialNodes[i].ReturnConnections().Contains("Right"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                // Else do the opposite
                else
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (potentialNodes[i].ReturnConnections().Contains("Right"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                
                break;
            
            case RGConnection.Left:
                
                if (neighborConnections.Contains("None") && !addedPoiNodes)
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (potentialNodes[i].ReturnConnections().Contains("Left"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                
                // If neighbor is to the left of the current tile and it connects right -> keep all possible tiles that connect left
                if (neighborConnections.Contains("Right"))
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (!potentialNodes[i].ReturnConnections().Contains("Left"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                // Else do the opposite
                else
                {
                    for (int i = potentialNodes.Count - 1; i > -1; i--)
                    {
                        if (potentialNodes[i].ReturnConnections().Contains("Left"))
                        {
                            potentialNodes.RemoveAt(i);
                        }
                    }
                }
                
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(neighborPosition), neighborPosition, null);
        }
        
    }
    
    private bool IsInsideGrid(Vector2Int target)
    {
        if (target.x > -1 && target.x < width && target.y > -1 && target.y < height)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
