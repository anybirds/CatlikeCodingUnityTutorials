using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
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
        public Vector3 direction;
        public Quaternion rotation;
        public Transform transform;
    }

    FractalPart[][] parts;

    FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    {
        var go = new GameObject($"Fractal Part L {levelIndex} C {childIndex}");
        go.transform.localScale = scale * Vector3.one;
        go.transform.SetParent(transform, false);  
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = material;
        return new FractalPart { 
            direction = directions[childIndex], 
            rotation = rotations[childIndex], 
            transform = go.transform };
    }

    private void Awake()
    {
        parts = new FractalPart[depth][];
        for (int i = 0, length = 1; i < depth; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
        }

        float scale = 1f;
        parts[0][0] = CreatePart(0, 0, scale);
        for (int i = 1; i < parts.Length; i++)
        {
            scale *= 0.5f;
            FractalPart[] levelParts = parts[i];
            for (int j = 0; j < levelParts.Length; j += 5)
            {
                for (int k = 0; k < 5; k++)
                {
                    levelParts[j + k] = CreatePart(i, k, scale);
                }
            }
        }
    }

    private void Update()
    {
        Quaternion deltaRotation = Quaternion.Euler(0f, 22.5f * Time.deltaTime, 0f);

        FractalPart rootPart = parts[0][0];
        rootPart.rotation *= deltaRotation;
        parts[0][0] = rootPart;
        for (int i = 1; i < parts.Length; i++)
        {
            FractalPart[] parentLevelPart = parts[i - 1];
            FractalPart[] levelParts = parts[i];
            for (int j = 0; j < levelParts.Length; j++)  
            {
                Transform parentPartTransform = parentLevelPart[j / 5].transform;
                FractalPart part = levelParts[j];
                Transform partTransform = part.transform;
                part.rotation *= deltaRotation;
                partTransform.localRotation = parentPartTransform.localRotation * part.rotation;
                partTransform.localPosition = parentPartTransform.localPosition + parentPartTransform.localRotation * (1.5f * partTransform.localScale.x * part.direction);
                levelParts[j] = part;
            }
        }
    }
}
