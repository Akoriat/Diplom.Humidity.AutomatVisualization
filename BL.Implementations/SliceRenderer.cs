using Common.Entities;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BL.Implementations;

public class SliceRenderer : FrameworkElement
{
    private double[,] _moistureData;
    private bool[,] _crackData;
    private readonly int _width;
    private readonly int _height;
    private readonly WriteableBitmap _writeableBitmap;
    private readonly Color DryColor = Colors.White;   
    private readonly Color WetColor = Colors.Blue;    
    private readonly Color CrackColor = Colors.Black; 

    public double[,] MoistureData
    {
        get => _moistureData;
        set
        {
            _moistureData = value;
            InvalidateVisual();
        }
    }

    public bool[,] CrackData
    {
        get => _crackData;
        set
        {
            _crackData = value;
            InvalidateVisual();
        }
    }

    public SliceRenderer() { }

    public SliceRenderer(int width, int height)
    {
        _width = width;
        _height = height;
        _writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
    }

    public WriteableBitmap Render(SoilSlice[,] grid)
    {
        int stride = _width * 4;
        byte[] pixels = new byte[_height * stride];

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Color color;
                if (grid[y, x].IsCracked)
                {
                    color = CrackColor;
                }
                else
                {
                    double m = grid[y, x].Moisture;
                    m = m < 0 ? 0 : m > 1 ? 1 : m;
                    byte r = (byte)(DryColor.R + (WetColor.R - DryColor.R) * m);
                    byte g = (byte)(DryColor.G + (WetColor.G - DryColor.G) * m);
                    byte b = (byte)(DryColor.B + (WetColor.B - DryColor.B) * m);
                    color = Color.FromRgb(r, g, b);
                }
                int idx = (y * _width + x) * 4;
                pixels[idx + 0] = color.B;
                pixels[idx + 1] = color.G;
                pixels[idx + 2] = color.R;
                pixels[idx + 3] = 255;
            }
        }

        _writeableBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, _width, _height), pixels, stride, 0);
        return _writeableBitmap;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (_moistureData == null) return;

        int rows = _moistureData.GetLength(0);
        int cols = _moistureData.GetLength(1);
        if (rows == 0 || cols == 0) return;

        double cellWidth = this.ActualWidth / cols;
        double cellHeight = this.ActualHeight / rows;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Rect rect = new Rect(c * cellWidth, r * cellHeight, cellWidth, cellHeight);
                Color color;

                if (_crackData != null && _crackData[r, c])
                {
                    color = Colors.Black;
                }
                else
                {
                    double value = _moistureData[r, c];
                    if (value < 0.0) value = 0.0;
                    if (value > 1.0) value = 1.0;
                    byte intensity = (byte)(255 * (1.0 - value));
                    color = Color.FromRgb(intensity, intensity, 255);
                }
                dc.DrawRectangle(new SolidColorBrush(color), null, rect);
            }
        }

        SolidColorBrush overlayBrush = new SolidColorBrush(Color.FromArgb(50, 200, 200, 200));
        for (int c = 0; c < cols; c++)
        {
            bool isCracked = (_crackData != null && _crackData[0, c]);
            if (!isCracked)
            {
                Rect topRect = new Rect(c * cellWidth, 0, cellWidth, cellHeight);
                dc.DrawRectangle(overlayBrush, null, topRect);
            }
        }
    }
}

