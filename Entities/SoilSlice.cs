using Common.Entities;

using System.Runtime.CompilerServices;

public sealed class SoilSlice
{
    public int Width { get; }
    public int Height { get; }

    private readonly byte[] _moisture;
    private readonly bool[] _cracks;

    public HashSet<int> CrackCells { get; } = new();

    public SoilSlice(SimulationSettings cfg)
    {
        Width = cfg.Width;
        Height = cfg.Height;
        _moisture = new byte[Width * Height];
        _cracks = new bool[Width * Height];
    }

    public byte[] MoistureArray => _moisture;
    public bool[] CrackMask => _cracks;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref byte Moisture(int x, int y) => ref _moisture[y * Width + x];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref bool Crack(int x, int y) => ref _cracks[y * Width + x];

    public void GenerateCracks(SoilSlice s)
    {
        var rnd = new Random();
        int w = s.Width;
        s.CrackCells.Clear();

        while (s.CrackCells.Count < 4)
        {
            int x = rnd.Next(0, w);
            s.CrackCells.Add(x);
        }
    }

}