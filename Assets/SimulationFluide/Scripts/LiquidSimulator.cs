using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
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

    [Header("Render Settings")] 
    public Material WaterMaterial;
    public bool SmoothNormals = false;

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

    //Rendering Method
    private MarchingCube _marchingCube;

    private List<GameObject> meshes = new();

    private MeshFilter _meshFilter;
    
    void Start()
    {
        _marchingCube = new MarchingCube(0.0f);
        
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
        
        //For the rendering
        GameObject go = new GameObject("Mesh");
        go.transform.parent = transform;
        _meshFilter = go.AddComponent<MeshFilter>();
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.material = WaterMaterial;

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
            p.mass = Random.Range(1.0f, 3.0f);
            
            spatialHashRenderer.Insert(p.pos, p);
            
            _particles[i] = p;
            
            go.transform.position = new Vector3(x, y, z);

            _particlesObject[i] = go;
        }
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        spatialHashRenderer.ClearGrid();
        
        _globalVariable[0].time = Time.fixedDeltaTime;

        _globalBuffer = new ComputeBuffer(1, 16);
        _globalBuffer.SetData(_globalVariable);
        
        _computeBuffer = new ComputeBuffer(numberOfParticles, 44);
        _computeBuffer.SetData(_particles);
        
        liquidComputeShader.SetBuffer(_indexOfKernel, "Result", _computeBuffer);
        liquidComputeShader.SetInt("numberOfParticles", numberOfParticles);
        liquidComputeShader.SetBuffer(_indexOfKernel, "_Particles", _computeBuffer);
        liquidComputeShader.SetBuffer(_indexOfKernel, "_Global", _globalBuffer);
        
        liquidComputeShader.SetFloat( "_Time", Time.fixedDeltaTime);
        
        liquidComputeShader.Dispatch(_indexOfKernel, 1024, 1, 1);

        _computeBuffer.GetData(_particles);

        //Change the position of gameobject
        for (int i = 0; i < numberOfParticles; ++i)
        {
            Vector3 newPos = new Vector3();
            newPos.x = _particles[i].pos.x;
            newPos.y = _particles[i].pos.y;
            newPos.z = _particles[i].pos.z;
            
            spatialHashRenderer.Insert(newPos, _particles[i]);
            
            _particlesObject[i].transform.position = newPos;
        }

        GenerateLiquidMesh();
    }

    void GenerateLiquidMesh()
    {
        meshes.Clear();
        
        var verts = new List<Vector3>();
        var normals = new List<Vector3>();
        var indices = new List<int>();

        _marchingCube.Generate(spatialHashRenderer.GetGridValues(), verts, indices);
        
        //create the normals
        if (SmoothNormals)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                //Presumes the vertex is in local space where
                //the min value is 0 and max is width/height/depth.
                Vector3 p = verts[i];

                float u = p.x / (spatialHashRenderer.nbCell - 1.0f);
                float v = p.y / (spatialHashRenderer.nbCell - 1.0f);
                float w = p.z / (spatialHashRenderer.nbCell - 1.0f);

                Vector3 n = spatialHashRenderer.GetNormal(u, v, w);

                normals.Add(n);
            }
            
        }

        var position = Vector3.zero;

        CreateMesh(verts, normals, indices, position);
    }

    void CreateMesh(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);
        
        if(normals.Count > 0) mesh.SetNormals(normals); else mesh.RecalculateNormals();

        mesh.RecalculateBounds();

        _meshFilter.mesh = mesh;
        _meshFilter.gameObject.transform.localPosition = position;

        //meshes.Add(_meshFilter.gameObject);
    }
    
}
