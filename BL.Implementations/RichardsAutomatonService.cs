using System.Runtime.CompilerServices;

namespace BL.Implementations;

public sealed class RichardsAutomatonService
{
    private readonly SoilSlice _soil;
    private readonly int _w, _h;

    private readonly double[] _hm = new double[256];
    private readonly double[] _K = new double[256];

    private readonly double[] _head;
    private readonly byte[] _next;

    private readonly double _dt;
    private readonly double _dx;
    private readonly double _byteToHeight;

    public RichardsAutomatonService(
        SoilSlice soil,
        double dt = 0.25,
        double dx = 1.0,
        double θr = 0.045,
        double θs = 0.43,
        double α = 0.145,
        double n = 2.68,
        double l = 0.5,
        double Ks = 1.0e-4
        )
    {
        _soil = soil;
        _w = soil.Width;
        _h = soil.Height;
        _dt = dt;
        _dx = dx;

        _head = new double[_w * _h];
        _next = new byte[_w * _h];

        double m = 1.0 - 1.0 / n;
        for (int b = 0; b < 256; b++)
        {
            double θ = θr + (θs - θr) * b / 255.0;
            double Se = (θ - θr) / (θs - θr);
            double hm = -1.0 / α * Math.Pow(Math.Pow(Se, -1.0 / m) - 1.0, 1.0 / n);
            double Krel = Math.Pow(Se, l) *
                         Math.Pow(1.0 - Math.Pow(1.0 - Math.Pow(Se, 1.0 / m), m), 2.0);
            _hm[b] = hm;
            _K[b] = Ks * Krel;
        }
        _byteToHeight = (θs - θr) * _dx / 255.0;
    }

    public void Tick(int subSteps = 1)
    {
        var src = _soil.MoistureArray;

        for (int s = 0; s < subSteps; s++)
        {
            for (int idx = 0; idx < src.Length; idx++)
            {
                byte m = src[idx];
                _head[idx] = _hm[m] + (_h - 1 - idx / _w);
                _next[idx] = m;
            }

            for (int y = 0; y < _h; y++)
            {
                int row = y * _w;
                for (int x = 0; x < _w; x++)
                {
                    int idx = row + x;
                    double hi = _head[idx];
                    double Ki = _K[src[idx]];

                    if (y + 1 < _h)
                        Exchange(idx, idx + _w, hi, Ki);

                    if (x + 1 < _w)
                        Exchange(idx, idx + 1, hi, Ki);
                }
            }

            Buffer.BlockCopy(_next, 0, src, 0, src.Length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Exchange(int i, int j, double hi, double Ki)
    {
        double hj = _head[j];
        double dh = hj - hi;
        if (dh >= 0) return;

        double Kij = 0.5 * (Ki + _K[_soil.MoistureArray[j]]);
        double q = -Kij * dh / _dx;
        double waterHeight = q * _dt;
        int mv = (int)(waterHeight / _byteToHeight);
        if (mv == 0) return;

        mv = Math.Min(mv, _next[i]);
        mv = Math.Min(mv, 255 - _next[j]);
        _next[i] -= (byte)mv;
        _next[j] += (byte)mv;

    }
}
