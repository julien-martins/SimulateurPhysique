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
    public int numberOfParticles;
    public float stiffness;
}

public struct Particle
{
    public Vector3 acc;
    public Vector3 vel;
    public Vector3 pos;
    public float mass;
    public float density;
}

public class LiquidSimulator : MonoBehaviour
{
    [Header("Global Settings")]
    
    public int numberOfParticles = 50;
    
    public ComputeShader liquidComputeShader;

    public BoxCollider boxSpawner;

    public GameObject ParticlePrefab;
    
    [Header("Fuild Settings")] 
    public float density = 1.0f;

    public float stiffness = 1.0f;

    [Header("Optimisation")] 
    public SpatialHash spatialHashRenderer;
    public SpatialHash spatialHashNeighbors;
    
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
        _computeBuffer = new ComputeBuffer(numberOfParticles, 44);

        //Create Global buffer and add all global parameter to the buffer
        _globalBuffer = new ComputeBuffer(1, 16);
        _globalVariable[0].density = density;
        _globalVariable[0].numberOfParticles = numberOfParticles;
        _globalVariable[0].stiffness = stiffness;
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

            //Create Particle with this parameter
            Particle p = new Particle();
            p.pos = new Vector3(x, y, z);
            p.acc = new();
            p.vel = new();
            p.density = Random.Range(0.2f, 0.7f);
            p.mass = Random.Range(0.5f, 1.5f);
            
            
            spatialHashRenderer.Insert(p.pos, p);
            
            _particles[i] = p;
            
            go.transform.position = new Vector3(x, y, z);

            _particlesObject[i] = go;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        _globalVariable[0].time = Time.fixedDeltaTime;

        _globalBuffer = new ComputeBuffer(1, 16);
        _globalBuffer.SetData(_globalVariable);
        
        _computeBuffer = new ComputeBuffer(numberOfParticles, 44);
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
