using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

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

    Vector3[] directions = { Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
    Quaternion[] rotations = { Quaternion.identity, Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f), Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f) };

    struct FractalPart
    {
        public Vector3 direction, worldPosition;
        public Quaternion rotation, worldRotation;
        public float spinAngle;
    }

    FractalPart[][] parts;
    Matrix4x4[][] matrices;
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
        parts = new FractalPart[depth][];
        matrices = new Matrix4x4[depth][];
        matricesBuffers = new ComputeBuffer[depth];
        int stride = 16 * 4;
        for (int i = 0, length = 1; i < depth; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
            matrices[i] = new Matrix4x4[length];
            matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        parts[0][0] = CreatePart(0, 0);
        for (int i = 1; i < parts.Length; i++)
        {
            FractalPart[] levelParts = parts[i];
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
        for (int i = 0; i < depth; i++)
        {
            matricesBuffers[i].Release();
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
        Quaternion deltaRotation = Quaternion.Euler(0f, 22.5f * Time.deltaTime, 0f);
        float scale = 1f;
        float spinAngleDelta = 22.5f * Time.deltaTime;
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = transform.rotation * rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f);
        rootPart.worldPosition = transform.position;
        float objectScale = transform.localScale.x;
        parts[0][0] = rootPart;
        matrices[0][0] = Matrix4x4.TRS(rootPart.worldPosition, rootPart.worldRotation, objectScale * Vector3.one);
        for (int i = 1; i < parts.Length; i++)
        {
            scale *= 0.5f;
            FractalPart[] parentLevelPart = parts[i - 1];
            FractalPart[] levelParts = parts[i];
            Matrix4x4[] levelMatrices = matrices[i];
            for (int j = 0; j < levelParts.Length; j++)  
            {
                FractalPart parent = parentLevelPart[j / 5];
                FractalPart part = levelParts[j];
                part.spinAngle += spinAngleDelta;
                part.worldRotation = parent.worldRotation * part.rotation * Quaternion.Euler(0f, part.spinAngle, 0f);
                part.worldPosition = parent.worldPosition + parent.worldRotation * (scale * 1.5f * part.direction);
                levelParts[j] = part;
                levelMatrices[j] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, scale * Vector3.one);
            }
        }

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
