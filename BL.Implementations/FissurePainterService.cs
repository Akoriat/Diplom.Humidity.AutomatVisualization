namespace BL.Implementations;

using System;
using Common.Entities;

public sealed class FissurePainterService
{
    private readonly SimulationSettings _cfg;
    private readonly Random _rnd;

    public FissurePainterService(SimulationSettings cfg)
    {
        _cfg = cfg;
        _rnd = new Random(cfg.RandomSeed);
    }

    public void GenerateCracks(SoilSlice slice)
    {
        int w = slice.Width;
        int h = slice.Height;
        int cracks = Math.Max(1, w / _cfg.CrackDensity);

        for (int i = 0; i < cracks; i++)
        {
            int startX = _rnd.Next(1, w - 1);
            Dig(slice, startX, 0);
        }
    }

    private void Dig(SoilSlice s, int x, int y)
    {
        int w = s.Width;
        int h = s.Height;
        while (y < h && x > 0 && x < w - 1)
        {
            s.Crack(x, y) = true;
            y++;
            if (_rnd.NextDouble() < _cfg.ZigzagChance)
                x += _rnd.Next(-1, 2);
            if (_rnd.NextDouble() < _cfg.ForkChance)
            {
                int branchX = Math.Clamp(x + _rnd.Next(-2, 3), 1, w - 2);
                Dig(s, branchX, y);
            }
        }
    }
}