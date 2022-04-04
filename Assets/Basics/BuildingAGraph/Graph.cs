using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FunctionLibrary;

public class Graph : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab;

    [SerializeField, Range(10, 100)]
    int resolution;

    [SerializeField]
    FunctionEnum functionEnum;

    [SerializeField, Min(0f)]
    float functionDuration;

    [SerializeField, Min(0f)]
    float transitionDuration;

    Transform[] points;
    float duration;
    bool transitioning;
    FunctionEnum transitionFunctionEnum;
    
    private void Awake()
    {
        points = new Transform[resolution * resolution];
        float step = 2.0f / resolution;
        Vector3 scale = Vector3.one * step;
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(pointPrefab);
            point.localScale = scale;
            point.SetParent(transform, false);
        }
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
        } else if (duration > functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transitionFunctionEnum = functionEnum;
            functionEnum = GetRandomFunctionEnumOtherThan(functionEnum);
        }

        if (transitioning)
        {
            UpdateFunctionTransition();
        } else
        {
            UpdateFunction();
        }
    }

    void UpdateFunction()
    {
        float time = Time.time;
        Function function = GetFunction(functionEnum);
        float step = 2.0f / resolution;
        int k = 0;
        float u, v;
        for (int i = 0; i < resolution; i++)
        {
            u = (i + 0.5f) * step - 1.0f;
            for (int j = 0; j < resolution; j++)
            {
                v = (j + 0.5f) * step - 1.0f;
                points[k].localPosition = function(u, v, time);
                k++;
            }
        }
    }

    void UpdateFunctionTransition()
    {
        Function from = GetFunction(transitionFunctionEnum);
        Function to = GetFunction(functionEnum);
        float progress = duration / transitionDuration;
        float time = Time.time;
        float step = 2.0f / resolution;
        int k = 0;
        float u, v;
        for (int i = 0; i < resolution; i++)
        {
            u = (i + 0.5f) * step - 1.0f;
            for (int j = 0; j < resolution; j++)
            {
                v = (j + 0.5f) * step - 1.0f;
                points[k].localPosition = Morph(u, v, time, from, to, progress);
                k++;
            }
        }
    }
}
