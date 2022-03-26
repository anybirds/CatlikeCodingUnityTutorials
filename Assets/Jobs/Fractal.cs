using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

public class Fractal : MonoBehaviour
{
    static readonly int matricesId = Shader.PropertyToID("_Matrices");
    static MaterialPropertyBlock propertyBlock;

    [SerializeField, Range(1, 8)]
    int depth;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    float3[] directions = { up(), right(), left(), forward(), back() };
    quaternion[] rotations = { quaternion.identity, quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI), quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI) };

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor
    {
        public float spinAngleDelta;
        public float scale;

        [ReadOnly]
        public NativeArray<FractalPart> parents;
        public NativeArray<FractalPart> parts;
        [WriteOnly]
        public NativeArray<float3x4> matrices;

        public void Execute(int i)
        {
            FractalPart parent = parents[i / 5];
            FractalPart part = parts[i];
            part.spinAngle += spinAngleDelta;
            part.worldRotation = mul(parent.worldRotation, mul(part.rotation, quaternion.RotateY(part.spinAngle)));
            part.worldPosition = parent.worldPosition + mul(parent.worldRotation, 1.5f * scale * part.direction);
            parts[i] = part;
            float3x3 r = float3x3(part.worldRotation) * scale;
            matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        }
    }

    struct FractalPart
    {
        public float3 direction, worldPosition;
        public quaternion rotation, worldRotation;
        public float spinAngle;
    }

    NativeArray<FractalPart>[] parts;
    NativeArray<float3x4>[] matrices;
    ComputeBuffer[] matricesBuffers;

    FractalPart CreatePart(int levelIndex, int childIndex) =>
        new FractalPart
        {
            direction = directions[childIndex],
            rotation = rotations[childIndex]
        };

    private void OnEnable()
    {
        propertyBlock ??= new MaterialPropertyBlock();
        parts = new NativeArray<FractalPart>[depth];
        matrices = new NativeArray<float3x4>[depth];
        matricesBuffers = new ComputeBuffer[depth];
        int stride = 12 * 4;
        for (int i = 0, length = 1; i < depth; i++, length *= 5)
        {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        parts[0][0] = CreatePart(0, 0);
        for (int i = 1; i < parts.Length; i++)
        {
            var levelParts = parts[i];
            for (int j = 0; j < levelParts.Length; j += 5)
            {
                for (int k = 0; k < 5; k++)
                {
                    levelParts[j + k] = CreatePart(i, k);
                }
            }
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }
        parts = null;
        matrices = null;
        matricesBuffers = null;
    }

    private void OnValidate()
    {
        if (enabled && parts != null)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        float scale = 1f;
        float spinAngleDelta = 0.125f * PI * Time.deltaTime;
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = mul(transform.rotation, mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle)));
        rootPart.worldPosition = transform.position;
        float objectScale = transform.localScale.x;
        parts[0][0] = rootPart;
        float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

        JobHandle jobHandle = default;
        for (int i = 1; i < parts.Length; i++)
        {
            scale *= 0.5f;
            jobHandle = new UpdateFractalLevelJob
            {
                spinAngleDelta = spinAngleDelta,
                scale = scale,
                parents = parts[i - 1],
                parts = parts[i],
                matrices = matrices[i]
            }.ScheduleParallel(parts[i].Length, 5, jobHandle);
            // Schedule(parts[i].Length, jobHandle);
        }
        jobHandle.Complete();

        Bounds bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        for (int i = 0; i < depth; i++)
        {
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);
            material.SetBuffer(matricesId, buffer);
            propertyBlock.SetBuffer(matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, properties: propertyBlock);
        }
    }
}
