using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate Vector3 Function(float u, float v, float t);

    private static Function[] functions = { Wave, MultiWave, Ripple, Sphere, Twist, Torus };

    public enum FunctionEnum
    {
        Wave,
        MultiWave,
        Ripple,
        Sphere,
        Band,
        Torus,
    }

    public static Function GetFunction(FunctionEnum functionEnum)
    {
        return functions[(int)functionEnum];
    }

    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;
        return p;
    }

    public static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + 0.5f * t));
        p.y += Sin(2.0f * PI * (v + t)) * 0.5f;
        p.y += Sin(PI * (u + v + 0.25f * t));
        p.y *= 1 / 2.5f;
        p.z = v;
        return p;
    }

    public static Vector3 Ripple(float u, float v, float t)
    {
        float d = Sqrt(u * u + v * v);
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (4.0f * d - t)) / (1.0f + 10.0f * d);
        p.z = v;
        return p;
    }
    
    public static Vector3 Sphere(float u, float v, float t)
    {
        float r = 0.5f + 0.5f * Sin(PI * t);
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Cos(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Sin(PI * u);
        return p;
    }

    public static Vector3 Twist(float u, float v, float t)
    {
        float r = 0.9f + 0.1f * Sin(PI * (6.0f * u + 4.0f * v + t));
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = s * Cos(PI * u);
        p.y = r * Sin(0.5f * PI * v);
        p.z = s * Sin(PI * u);
        return p;
    }

    public static Vector3 Torus(float u, float v, float t)
    {
        float r1 = 0.7f + 0.1f * Sin(PI * (6.0f * u + 0.5f * t));
        float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Cos(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Sin(PI * u);
        return p;
    }
}
