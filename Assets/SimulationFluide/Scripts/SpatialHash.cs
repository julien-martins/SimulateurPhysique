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
    public bool FlipNormals = false;

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

    public float[,,] GetGridValues()
    {
        var result = new float[nbCell, nbCell, nbCell];

        int x, y, z;
        for (x = 0; x < nbCell; ++x)
        {
            for (y = 0; y < nbCell; ++y)
            {
                for (z = 0; z < nbCell; ++z)
                {
                    result[x, y, z] = IsEmpty(x, y, z) ? 0 : 1;
                }
            }
        }

        return result;
    }

    public Vector3 GetNormal(float u, float v, float w)
    {
        var n = GetFirstDerivative(u, v, w);

        if (FlipNormals)
            return n.normalized * -1;
        else
            return n.normalized;
    }

    public Vector3 GetFirstDerivative(float u, float v, float w)
    {
        const float h = 0.005f;
        const float hh = h * 0.5f;
        const float ih = 1.0f / h;

        float dx_p1 = GetCell(u + hh, v, w);
        float dy_p1 = GetCell(u, v + hh, w);
        float dz_p1 = GetCell(u, v, w + hh);

        float dx_m1 = GetCell(u - hh, v, w);
        float dy_m1 = GetCell(u, v - hh, w);
        float dz_m1 = GetCell(u, v, w - hh);

        float dx = (dx_p1 - dx_m1) * ih;
        float dy = (dy_p1 - dy_m1) * ih;
        float dz = (dz_p1 - dz_m1) * ih;

        return new Vector3(dx, dy, dz);
    }

    public float GetCell(float u, float v, float w)
    {
        float x = u * (nbCell - 1);
        float y = v * (nbCell - 1);
        float z = w * (nbCell - 1);

        int xi = (int)Mathf.Floor(x);
        int yi = (int)Mathf.Floor(y);
        int zi = (int)Mathf.Floor(z);

        float v000 = IsEmpty(xi, yi, zi) ? 0 : 1;
        float v100 = IsEmpty(xi + 1, yi, zi) ? 0 : 1;
        float v010 = IsEmpty(xi, yi + 1, zi) ? 0 : 1;
        float v110 = IsEmpty(xi + 1, yi + 1, zi) ? 0 : 1;

        float v001 = IsEmpty(xi, yi, zi + 1) ? 0 : 1;
        float v101 = IsEmpty(xi + 1, yi, zi + 1) ? 0 : 1;
        float v011 = IsEmpty(xi, yi + 1, zi + 1) ? 0 : 1;
        float v111 = IsEmpty(xi + 1, yi + 1, zi + 1) ? 0 : 1;

        float tx = Mathf.Clamp01(x - xi);
        float ty = Mathf.Clamp01(y - yi);
        float tz = Mathf.Clamp01(z - zi);

        //use bilinear interpolation the find these values.
        float v0 = BLerp(v000, v100, v010, v110, tx, ty);
        float v1 = BLerp(v001, v101, v011, v111, tx, ty);

        //Now lerp those values for the final trilinear interpolation.
        return Lerp(v0, v1, tz);
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
    
    private static float Lerp(float v0, float v1, float t)
    {
        return v0 + (v1 - v0) * t;
    }
    
    private static float BLerp(float v00, float v10, float v01, float v11, float tx, float ty)
    {
        return Lerp(Lerp(v00, v10, tx), Lerp(v01, v11, tx), ty);
    }
    
}
