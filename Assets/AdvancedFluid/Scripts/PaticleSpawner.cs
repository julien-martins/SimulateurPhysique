using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PaticleSpawner : MonoBehaviour
{
    [SerializeField] private Mesh particleMesh;
    [SerializeField] private Material particleMat;

    [SerializeField] private ComputeShader compute;
    
    [SerializeField] private int nbParticle = 100;

    private Matrix4x4[] _matrices;
    private Bounds _bounds;

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    private struct ParticleProperties
    {
        public Matrix4x4 mat;

        public static int Size()
        {
            return
                sizeof(float) * 4 * 4;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {

        _bounds = new Bounds(transform.position, Vector3.one * (300 + 1));
        
        InitializeBuffers();
    }

    // Update is called once per frame
    void Update()
    {
        int kernel = compute.FindKernel("CSMain");
        
        //Draw a bunch of meshes each frame
        compute.Dispatch(kernel, Mathf.CeilToInt(nbParticle / 64f), 1, 1);
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMat, _bounds, argsBuffer);
    }

    private void InitializeBuffers()
    {
        int kernel = compute.FindKernel("CSMain");
        
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes
        args[0] = particleMesh.GetIndexCount(0);
        args[1] = (uint)nbParticle;
        args[2] = particleMesh.GetIndexStart(0);
        args[3] = particleMesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        ParticleProperties[] propertiesArray = new ParticleProperties[nbParticle];
        for (int i = 0; i < nbParticle; i++)
        {
            ParticleProperties prop = new ParticleProperties();
            Vector3 pos = new Vector3(Random.Range(-300, 300), Random.Range(0, 300), Random.Range(-300, 300));
            Quaternion rot = quaternion.identity;
            Vector3 scale = Vector3.one;

            prop.mat = Matrix4x4.TRS(pos, rot, scale);

            propertiesArray[i] = prop;
        }

        meshPropertiesBuffer = new ComputeBuffer(nbParticle, ParticleProperties.Size());
        meshPropertiesBuffer.SetData(propertiesArray);
        compute.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
        particleMat.SetBuffer("_Properties", meshPropertiesBuffer);
    }
}
