using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Join : MonoBehaviour
{
    public bool Fixed = true;

    public Vector3 Speed = Vector3.zero;
    
    public List<Platform> ConnectedPlatforms = new();

    public Vector3 PositionInitial = Vector3.zero;

    void Start()
    {
        PositionInitial = transform.position;
    }
    
    void Update()
    {
        if (Fixed) GetComponent<SpriteRenderer>().color = Color.red;
        else GetComponent<SpriteRenderer>().color = Color.yellow;
    }
}
