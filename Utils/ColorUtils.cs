using JetBrains.Annotations;

namespace FishsGrandAdventure.Utils;

using UnityEngine;

[PublicAPI]
public static class ColorUtils
{
    public const double Tolerance = 0.000001f;

    // Shader Properties
    public static readonly Color32 Black = new Color32(0, 0, 0, 255);
    public static readonly Color32 Transparent = new Color32(0, 0, 0, 0);
    public static readonly Color32 Marker = new Color32(255, 0, 255, 255);
    public static readonly Color32 Teal = new Color32(0, 255, 255, 255);
    public static readonly Color32 TealA1 = new Color32(0, 255, 255, 1);
    public static readonly Color32 Purple = new Color(0.69f, 0f, 1f);
    public static readonly Color32 Orange = new Color(1f, 0.65f, 0f);
    public static readonly Color32 LightBlue = new Color(0, .87f, 1f);
    public static readonly Color32 Yellow = new Color(1f, 1f, 0f);
    public static readonly Color32 Bronze = new Color32(0xcd, 0x7f, 0x32, 0xff);
    public static readonly Color32 Silver = new Color32(0xc0, 0xc0, 0xc0, 0xff);
    public static readonly Color32 Gold = new Color32(0xff, 0xf7, 0x00, 0xff);
    public static readonly Color32 Platinum = new Color32(0xe5, 0xe4, 0xe2, 0xff);

    public static Color WithAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    public static Color WithAlpha(this Color color, byte alpha)
    {
        color.a = alpha.ToFloat();
        return color;
    }

    public static Color32 WithAlpha(this Color32 color, byte alpha)
    {
        color.a = alpha;
        return color;
    }

    public static Color32 WithAlpha(this Color32 color, float alpha)
    {
        color.a = alpha.ToByte();
        return color;
    }

    public static Color KeepAlpha(this Color color, Color newColor)
    {
        newColor.a = color.a;
        return newColor;
    }

    public static Color32 KeepAlpha(this Color32 color, Color32 newColor)
    {
        newColor.a = color.a;
        return newColor;
    }

    public static Color Clone(this Color color)
    {
        return new Color(color.r, color.g, color.b, color.a);
    }

    public static bool SameAs(ref Color32 a, ref Color32 b)
    {
        return a.a == b.a && a.b == b.b && a.g == b.g && a.r == b.r;
    }

    public static bool SameAs(ref Color a, ref Color b)
    {
        return Mathf.Abs(a.a - b.a) < Tolerance && Mathf.Abs(a.b - b.b) < Tolerance &&
               Mathf.Abs(a.g - b.g) < Tolerance && Mathf.Abs(a.r - b.r) < Tolerance;
    }

    public static Color InvertColor(this Color color)
    {
        return new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b);
    }

    public static Color RandomNonDarkColor()
    {
        return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    public static Color RandomColor()
    {
        return Random.ColorHSV(0f, 1f, 0f, 1f, .5f, 1f);
    }

    /// <summary>
    /// Converts a linear color space value to a gamma color space value.
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static float LinearToGamma(float val)
    {
        return Mathf.Pow(val, 0.4545454f);
    }

    /// <summary>
    /// Converts a gamma color space value to a linear color space value.
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static float GammaToLinear(float val)
    {
        return Mathf.Pow(val, 2.2f);
    }


    /// <summary>
    /// Returns a string with its proper string hex form.
    ///
    /// Format: RRGGBBAA
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static string ToHex(this Color c)
    {
        return $"{c.r.ToByte():X2}{c.g.ToByte():X2}{c.b.ToByte():X2}{c.a.ToByte():X2}";
    }

    /// <summary>
    /// Converts a color to its corresponding hex value.
    ///
    /// Format: RRGGBBAA
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static string ToHex(this Color32 c)
    {
        return $"{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";
    }
}