using System.Windows.Media;

namespace Render.Rendering;

internal sealed class ColorRamp
{
    private readonly Color[] _lut = new Color[256];

    public ColorRamp()
    {
        for (int i = 0; i < 256; i++)
        {
            if (i < 128)
            {
                double t = i / 127.0;
                _lut[i] = Lerp((101, 67, 33), (34, 139, 34), t);
            }
            else
            {
                double t = (i - 128) / 127.0;
                _lut[i] = Lerp((34, 139, 34), (15, 118, 255), t);
            }
        }
    }

    private static Color Lerp((byte r, byte g, byte b) a, (byte r, byte g, byte b) b, double t)
        => Color.FromRgb(
            (byte)(a.r + (b.r - a.r) * t),
            (byte)(a.g + (b.g - a.g) * t),
            (byte)(a.b + (b.b - a.b) * t));

    public Color this[int moisture] => _lut[moisture];
}
internal static class ColorsEx
{
    public static readonly Color Water = Color.FromRgb(15, 118, 255);

    public static readonly Color Soil = Color.FromRgb(70, 45, 25);
}

