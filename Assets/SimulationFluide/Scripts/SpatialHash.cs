using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialHash : MonoBehaviour
{
    /* Utilisation of Case
     * 
     * grid = new SpatialHash(bounds, dimension)
     * client = grid.NewClient(parameters);
     *
     * client.postion = newPosition;
     * grid.UpdateClient(client)
     *
     * nearby = grid.FindNearby(location, bounds);
     *
     * grid.RemoveClient(client);
     * 
     */

    //use for debbuging the grid 
    struct TestCell
    {
        private Vector3 pos;
    }
    
    [Header("Debugger")]
    public bool debug = false;

    public Transform testCell;

    public float cellSize = 1;
    public int nbCell = 2;

    private Dictionary<string, List<object>> _cells;
    
    private void OnDrawGizmos()
    {
        if (!debug) return;
        
        if(!Application.isPlaying)
            GridInitialization();

        if(testCell)
            Insert(testCell.position, new TestCell());
        
        for (int x = 0; x < nbCell; ++x)
        {
            for (int y = 0; y <= nbCell; ++y)
            {
                for (int z = 0; z <= nbCell; ++z)
                {
                    var cellWorldPos = new Vector3(
                        x * cellSize + cellSize / 2,
                        y * cellSize + cellSize / 2,
                        z * cellSize + cellSize / 2
                        );

                    if (!IsEmpty(x, y, z))
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(transform.position + cellWorldPos, 0.6f);
                    }

                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(transform.position + cellWorldPos, Vector3.one * cellSize);
                }
            }
        }
        
        

    }

    public void Awake()
    {
        GridInitialization();
    }

    public void GridInitialization()
    {
        _cells = new ();
    }
    
    public void Insert(Vector3 pos, object cell)
    {
        var key = GetKey(pos);
        
        if (!_cells.ContainsKey(key))
            _cells[key] = new List<object>();

        _cells[key].Add(cell);
    }

    public List<object> GetCell(Vector3 pos)
    {
        var key = GetKey(pos);

        if (_cells.ContainsKey(key))
            return _cells[key];
        
        return new();
    }
    
    //Find all neighbors with a given position 
    public void FindNearby()
    {
        
    }

    public bool IsEmpty(Vector3 pos)
    {
        var key = GetKey(pos);

        List<object> result = new();
        if (_cells.TryGetValue(key, out result))
        {
            return result.Count == 0;
        }
        
        return true;
    }
    
    public bool IsEmpty(int x, int y, int z)
    {
        var key = GetKey(x, y, z);

        List<object> result;
        if (_cells.TryGetValue(key, out result))
        {
            return result.Count == 0;
        }
        
        return true;
    }

    public void ClearGrid()
    {
        _cells.Clear();
    }
    
    private string GetKey(Vector3 pos)
    {
        //Get Cell index of the grid
        int cellX = (int)((pos.x - transform.position.x) / cellSize);
        int cellY = (int)((pos.y - transform.position.y) / cellSize);
        int cellZ = (int)((pos.z - transform.position.z) / cellSize);
        
        return hash(cellX, cellY, cellZ);
    }

    private string GetKey(int x, int y, int z)
    {
        return hash(x, y, z);
    }
    
    /* Hash Function
     * hash(x, y, z) = (x * p1 xor x * p2 xor z * p3) mod n
     * n is the hash table size
     * with a n equal to a prime number the function work most efficiently
     * p1, p2, p3 are large prime numbers
     * p1 = 73856093
     * p2 = 19349663
     * p3 = 83492791
     */
    string hash(int x, int y, int z)
    {
        //return (int)((x * 73856093 ^ y * 19349663 ^ z * 83492791) % (float)(Math.Pow(cellSize*2, 3)));
        return $"{x}/{y}/{z}";
    }
    
}
