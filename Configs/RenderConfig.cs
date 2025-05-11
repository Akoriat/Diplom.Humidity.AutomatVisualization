using System.Windows.Media;

namespace Common.Configs;
public static class RenderConfig
{
    public static int CellPx { get; set; } = 60;
    public static readonly Color GridClr = Colors.DimGray;
    public static readonly Color CrackRod = Colors.Black;
    public static readonly Color BackColor = Color.FromRgb(70, 45, 25);
}
