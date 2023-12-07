namespace FishsGrandAdventure.Utils;

using System.Text;
using JetBrains.Annotations;
using UnityEngine;

[PublicAPI]
public static class MathUtils
{
    public const float Tolerance = 0.000001f;

    /// <summary>
    /// Square this value (better than Math.Pow(x, 2)).
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static int Sqr(this int a)
    {
        return a * a;
    }

    /// <summary>
    /// Square this value (better than Math.Pow(x, 2)).
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static long Sqr(this long a)
    {
        return a * a;
    }

    /// <summary>
    /// Square this value (better than Math.Pow(x, 2)).
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static float Sqr(this float a)
    {
        return a * a;
    }

    /// <summary>
    /// Square this value (better than Math.Pow(x, 2)).
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static double Sqr(this double a)
    {
        return a * a;
    }

    /// <summary>
    /// Proper modulo function that works with negatives.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    public static int Mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    public static byte ToByte(this float f)
    {
        // Same as Mathf.Clamp01 in Unity
        f = f < 0 ? 0 : f > 1 ? 1 : f;
        return (byte)(f * 255);
    }

    public static float ToFloat(this byte b)
    {
        return b / 255f;
    }

    public static string ToHexString(this byte[] ba)
    {
        var hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }

    /// <summary>
    /// Remaps a number range to a new number range.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="originalMin"></param>
    /// <param name="originalMax"></param>
    /// <param name="newMin"></param>
    /// <param name="newMax"></param>
    /// <returns></returns>
    public static float Remap(this float value, float originalMin, float originalMax, float newMin, float newMax)
    {
        float t = Mathf.InverseLerp(originalMin, originalMax, value);
        return Mathf.Lerp(newMin, newMax, t);
    }
}