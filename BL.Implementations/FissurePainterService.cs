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

    public void GenerateCracks(SoilSlice s)
    {
        s.CrackCells.Clear();
        var rnd = new Random();
        while (s.CrackCells.Count < 4)
            s.CrackCells.Add(rnd.Next(0, s.Width));
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