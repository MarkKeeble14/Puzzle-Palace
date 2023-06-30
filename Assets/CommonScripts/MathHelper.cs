using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathHelper
{
    public static float Normalize(float x, float min, float max, float a, float b)
    {
        return (b - a) * ((x - min) / (max - min)) + a;
    }

    public static bool ApproximatelyEqual(Vector3 a, Vector3 b)
    {
        return (Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y));
    }

    public static bool ApproximatelyEqual(Color a, Color b)
    {
        return (
                Mathf.Approximately(a.r, b.r)
            && Mathf.Approximately(a.g, b.g)
            && Mathf.Approximately(a.b, b.b)
            && Mathf.Approximately(a.a, b.a));
    }

    // clockwise
    public static Vector3 RotateClockwise(Vector3 aDir, float angle)
    {
        return new Vector3(aDir.z, 0, -aDir.x);
    }

    // counter clockwise
    public static Vector3 RotateCounterClockwise(Vector3 aDir, float angle)
    {
        return new Vector3(-aDir.z, 0, aDir.x);
    }

    public static int RoundToNearestGivenInt(int v1, int v2)
    {
        if (v1 % v2 == 0)
        {
            return Mathf.FloorToInt(v1 / v2) * v2;
        }
        else
        {
            return ((Mathf.FloorToInt(v1 / v2)) * v2) + v2;
        }
    }
}
