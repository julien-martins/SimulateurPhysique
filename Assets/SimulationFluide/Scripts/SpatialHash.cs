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
    
    /* Hash Function
     * hash(x, y, z) = (x * p1 xor x * p2 xor z * p3) mod n
     * n is the hash table size
     * with a n equal to a prime number the function work most efficiently
     * p1, p2, p3 are large prime numbers
     * p1 = 73856093
     * p2 = 19349663
     * p3 = 83492791
     */

    public bool debug = false;

    public float cellSize;
    public int nbCell = 2;
    
    private void OnDrawGizmos()
    {
        if (!debug) return;
        
        for (float x = -nbCell; x < nbCell; ++x)
        {
            for (float y = -nbCell; y < nbCell; ++y)
            {
                for (float z = -nbCell; z < nbCell; ++z)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position + new Vector3(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2, z * cellSize + cellSize / 2), Vector3.one * cellSize);
                }
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
