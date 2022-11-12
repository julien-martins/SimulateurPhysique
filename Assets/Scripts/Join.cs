using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Join : MonoBehaviour
{
    public bool Fixed = true;

    public List<Platform> ConnectedPlatforms = new();
    
    void Update()
    {
        if (Fixed) GetComponent<SpriteRenderer>().color = Color.red;
        else GetComponent<SpriteRenderer>().color = Color.yellow;
    }
}
