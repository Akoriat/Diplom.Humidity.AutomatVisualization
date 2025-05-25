using Common.Entities;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BL.Implementations;

public class SliceRenderer : FrameworkElement
{
    private double[,] _moistureData;
    private bool[,] _crackData;
    private readonly int width;
    private readonly int height;
    private readonly WriteableBitmap bitmap;
    private readonly Color dry = Colors.White;   
    private readonly Color wet = Colors.Blue;    
    private readonly Color crack = Colors.Black; 

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
        this.width = width;
        this.height = height;
        bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
    }

    public WriteableBitmap Render(SoilSlice[,] grid)
    {
        int stride = width * 4;
        byte[] pixels = new byte[height * stride];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color;
                if (grid[y, x].IsCracked)
                {
                    color = crack;
                }
                else
                {
                    double m = grid[y, x].Moisture;
                    m = m < 0 ? 0 : m > 1 ? 1 : m;
                    byte r = (byte)(dry.R + (wet.R - dry.R) * m);
                    byte g = (byte)(dry.G + (wet.G - dry.G) * m);
                    byte b = (byte)(dry.B + (wet.B - dry.B) * m);
                    color = Color.FromRgb(r, g, b);
                }
                int idx = (y * width + x) * 4;
                pixels[idx + 0] = color.B;
                pixels[idx + 1] = color.G;
                pixels[idx + 2] = color.R;
                pixels[idx + 3] = 255;
            }
        }

        bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), pixels, stride, 0);
        return bitmap;
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

