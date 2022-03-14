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

    Transform[] points;

    
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
}
