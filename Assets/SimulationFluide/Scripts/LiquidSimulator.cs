using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Sphere
{
    public Vector3 pos;
}

public class LiquidSimulator : MonoBehaviour
{
    public int numberOfElements = 1;
    
    public ComputeShader liquidComputeShader;

    private ComputeBuffer _computeBuffer;
    
    private int _indexOfKernel;

    private int _resultTest;

    private Sphere[] _spheres;
    
    void Start()
    {
        _spheres = new Sphere[numberOfElements];
        for (int i = 0; i < numberOfElements; ++i)
            _spheres[i] = new Sphere();
        
        _indexOfKernel = liquidComputeShader.FindKernel("CSMain");

        // stride 4 * pos(3)
        _computeBuffer = new ComputeBuffer(numberOfElements, 12);
        _computeBuffer.SetData(_spheres);

        liquidComputeShader.SetBuffer(_indexOfKernel, "Result", _computeBuffer);
        liquidComputeShader.Dispatch(_indexOfKernel, 16, 16, 1);
        
        _computeBuffer.GetData(_spheres);
        _computeBuffer.Release();
        
        Debug.Log(_spheres[0].pos);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
