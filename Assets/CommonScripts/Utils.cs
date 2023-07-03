using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Utils
{
    public static int StandardSentinalValue => -1;

    public static string CapitalizeFirstLetter(string s)
    {
        return s[0].ToString().ToUpper() + s.Substring(1, s.Length - 1).ToLower();
    }

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

    public static string GetPluralization(int num)
    {
        return num > 1 ? "es" : "";
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

    public static string ParseDuration(int time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = time - (minutes * 60);
        return minutes + "m:" + seconds + "s";
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

    public static IEnumerator ChangeScale(Transform t, Vector3 target, float changeRate)
    {
        while (t.localScale != target)
        {
            t.localScale = Vector3.MoveTowards(t.localScale, target, Time.deltaTime * changeRate);
            yield return null;
        }
    }

    public static IEnumerator ChangeCanvasGroupAlpha(CanvasGroup cv, float target, float changeRate)
    {
        while (cv.alpha != target)
        {
            cv.alpha = Mathf.MoveTowards(cv.alpha, target, Time.deltaTime * changeRate);
            yield return null;
        }
    }

    public static IEnumerator ChangeColor(Image image, Color target, float changeRate)
    {
        while (image.color != target)
        {
            image.color = Vector4.MoveTowards(image.color, target, changeRate * Time.deltaTime);
            yield return null;
        }
    }

    public static int ConvertCharToInt(char v)
    {
        int num;
        if (int.TryParse(v.ToString(), out num))
        {
            return num;
        }
        return -1;
    }

    public static float TestFunc(string label, Action func, bool printResult)
    {
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
        st.Start();
        func?.Invoke();
        st.Stop();
        if (printResult)
            Debug.Log(label + " - Took: " + st.ElapsedMilliseconds + "ms");
        return st.ElapsedMilliseconds;
    }

    public static void TestFunc(int repititions, string label, Action func, bool printResult, bool printAvg)
    {
        float total = 0;
        for (int i = 0; i < repititions; i++)
        {
            total += TestFunc(label, func, printResult);
        }
        if (printAvg)
        {
            Debug.Log(label + " - Average: " + (total / repititions) + "ms");
        }
    }
}
