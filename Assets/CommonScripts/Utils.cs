using System;
using System.Globalization;
using UnityEngine;

public class Utils
{
    public static string ConvVector3ToString(Vector3 v)
    {
        return "<" + v.x + ", " + v.y + ", " + v.z + ">";
    }

    public static string ConvVector3ToString(Vector3 v, int roundTo)
    {
        return "<" + RoundTo(v.x, roundTo) + ", " + RoundTo(v.y, roundTo) + ", " + RoundTo(v.z, roundTo) + ">";
    }

    public static string ConvVector3ToString(Vector3 v, int roundTo, string format)
    {
        return string.Format(format, RoundTo(v.x, roundTo), RoundTo(v.y, roundTo), RoundTo(v.z, roundTo));
    }

    public static string ConvVector3ToString(Vector3 v, int roundTo, string format, float minValue)
    {
        float x = v.x > minValue ? v.x : 0;
        float y = v.y > minValue ? v.y : 0;
        float z = v.z > minValue ? v.z : 0;
        return string.Format(format, RoundTo(x, roundTo), RoundTo(y, roundTo), RoundTo(z, roundTo));
    }

    public static string ConvVector3ToStringAbs(Vector3 v, int roundTo, string format, float minValue)
    {
        float x = Mathf.Abs(v.x) > minValue ? v.x : 0;
        float y = Mathf.Abs(v.y) > minValue ? v.y : 0;
        float z = Mathf.Abs(v.z) > minValue ? v.z : 0;
        return string.Format(format, RoundTo(x, roundTo), RoundTo(y, roundTo), RoundTo(z, roundTo));
    }

    public static float RoundTo(float v, int numDigits)
    {
        return (float)System.Math.Round(v, numDigits);
    }

    public static bool ParseFloat(string s, out float v)
    {
        try
        {
            v = float.Parse(s, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            //
            Debug.LogWarning("Attempted an Invalid Parse");
            v = 0;
            return false;
        }
    }

    public static bool ParseInt(string s, out int v)
    {
        try
        {
            v = int.Parse(s, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            //
            Debug.LogWarning("Attempted an Invalid Parse");
            v = 0;
            return false;
        }
    }

    public static string GetRepeatingString(string s, int repeat)
    {
        string r = "";
        for (int i = 0; i < repeat; i++)
            r += s;
        return r;
    }

    public static int GetNumDigits(int v)
    {
        return v == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(v)) + 1);
    }

    public static int GetMaxDigits(Vector3 v3)
    {
        int max = 1;
        int x = GetNumDigits((int)Math.Truncate(v3.x));
        if (x > max)
            max = x;
        int y = GetNumDigits((int)Math.Truncate(v3.y));
        if (y > max)
            max = y;
        int z = GetNumDigits((int)Math.Truncate(v3.z));
        if (z > max)
            max = z;
        return max;
    }
}
