using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public struct GlobalVariable
{
    public float time;
    public float density;
}

public struct Particle
{
    public Vector3 pos;
    public float mass;
}

public class LiquidSimulator : MonoBehaviour
{
    [Header("Global Settings")]
    
    public int numberOfParticles = 50;
    
    public ComputeShader liquidComputeShader;

    public BoxCollider boxSpawner;

    public GameObject ParticlePrefab;
    
    [Header("Fuild Settings")] public float density = 1.0f;
    
    private ComputeBuffer _computeBuffer;
    private ComputeBuffer _globalBuffer;
    
    // ###### PRIVATE FIELD ######
    
    private int _indexOfKernel;

    private int _resultTest;

    private GlobalVariable[] _globalVariable;
    
    private Particle[] _particles;

    private GameObject[] _particlesObject;
    
    void Start()
    {
        _globalVariable = new GlobalVariable[1];
        
        _particles = new Particle[numberOfParticles];
        _particlesObject = new GameObject[numberOfParticles];
        
        SpawnSpheres();

        _indexOfKernel = liquidComputeShader.FindKernel("CSMain");

        // stride 4 * pos(3)
        _computeBuffer = new ComputeBuffer(numberOfParticles, 16);

        _globalBuffer = new ComputeBuffer(1, 8);
        _globalVariable[0].density = density;
    }

    void SpawnSpheres()
    {
        for (int i = 0; i < numberOfParticles; ++i)
        {
            //Create Sphere Primitive
            var go = GameObject.Instantiate(ParticlePrefab, transform);
            
            //Spawn sphere on the BoxCollider define on the scene
            float x = Random.Range(boxSpawner.transform.position.x + boxSpawner.center.x - boxSpawner.size.x/2, boxSpawner.transform.position.x + boxSpawner.center.x + boxSpawner.size.x/2);
            float y = Random.Range(boxSpawner.transform.position.y + boxSpawner.center.y - boxSpawner.size.y/2, boxSpawner.transform.position.y + boxSpawner.center.y + boxSpawner.size.y/2);
            float z = Random.Range(boxSpawner.transform.position.z + boxSpawner.center.z - boxSpawner.size.z/2, boxSpawner.transform.position.z + boxSpawner.center.z + boxSpawner.size.z/2);

            Particle s = new Particle();
            s.pos.x = x;
            s.pos.y = y;
            s.pos.z = z;

            s.mass = Random.Range(1, 10);
            
            _particles[i] = s;
            
            go.transform.position = new Vector3(x, y, z);

            _particlesObject[i] = go;
        }
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        _globalVariable[0].time = Time.fixedDeltaTime;
        _globalVariable[0].time = density;

        _globalBuffer = new ComputeBuffer(1, 8);
        _globalBuffer.SetData(_globalVariable);
        
        _computeBuffer = new ComputeBuffer(numberOfParticles, 16);
        _computeBuffer.SetData(_particles);
        
        liquidComputeShader.SetBuffer(_indexOfKernel, "Result", _computeBuffer);
        liquidComputeShader.SetInt("numberOfParticles", numberOfParticles);
        liquidComputeShader.SetBuffer(_indexOfKernel, "_Particles", _computeBuffer);
        liquidComputeShader.SetBuffer(_indexOfKernel, "_Global", _globalBuffer);
        
        liquidComputeShader.SetFloat( "_Time", Time.fixedTime);
        
        liquidComputeShader.Dispatch(_indexOfKernel, 1024, 1, 1);

        _computeBuffer.GetData(_particles);
        _computeBuffer.Release();

        //Change the position of gameobject
        for (int i = 0; i < numberOfParticles; ++i)
        {
            Vector3 newPos = new Vector3();
            newPos.x = _particles[i].pos.x;
            newPos.y = _particles[i].pos.y;
            newPos.z = _particles[i].pos.z;
            
            _particlesObject[i].transform.position = newPos;
        }
    }
}
