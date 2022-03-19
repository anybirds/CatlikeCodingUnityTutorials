using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FunctionLibrary;

public class GPUGraph : MonoBehaviour
{
    const int maxResolution = 1000;

    [SerializeField, Range(10, maxResolution)]
    int resolution;

    [SerializeField]
    FunctionEnum functionEnum;

    [SerializeField, Min(0f)]
    float functionDuration;

    [SerializeField, Min(0f)]
    float transitionDuration;

    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;

    float duration;
    bool transitioning;
    FunctionEnum transitionFunctionEnum;
    ComputeBuffer positionsBuffer;

    static int
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time"),
        transitionProgressId = Shader.PropertyToID("_TransitionProgress");

    private void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        computeShader.SetFloat(transitionProgressId, Mathf.SmoothStep(0f, 1f, duration /transitionDuration));
        int kernelIndex = (int)functionEnum + (int)(transitioning ? transitionFunctionEnum : functionEnum) * FunctionCount;
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);
        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f * step));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

    private void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    private void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration > transitionDuration)
            {
                duration -= functionDuration;
                transitioning = false;
            }
        }
        else if (duration > functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transitionFunctionEnum = functionEnum;
            functionEnum = GetRandomFunctionEnumOtherThan(functionEnum);
        }

        UpdateFunctionOnGPU();
    }

}
