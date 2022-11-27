using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleSpawner : MonoBehaviour
{

    public GameObject particlePrefab;

    public Text particleNumber;

    private int _particleNumber;
    
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if(Input.GetMouseButton(0))
        {
            Instantiate(particlePrefab, mousePos, Quaternion.identity, this.transform);
            _particleNumber++;
            particleNumber.text = _particleNumber.ToString();
        }
    }
}
