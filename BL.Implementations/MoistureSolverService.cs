using Common.Entities;

public sealed class MoistureSolverService
{
    private readonly SimulationSettings _cfg;
    private readonly byte[] _buffer;

    public MoistureSolverService(SimulationSettings cfg)
    {
        _cfg = cfg;
        _buffer = new byte[cfg.Width * cfg.Height];
    }

    public void Tick(SoilSlice slice, int steps = 1)
    {
        var moisture = slice.MoistureArray;
        var cracks = slice.CrackMask;
        int w = slice.Width;
        int h = slice.Height;

        for (int s = 0; s < steps; s++)
        {
            Parallel.For(1, h - 1, y =>
            {
                int row = y * w;
                for (int x = 1; x < w - 1; x++)
                {
                    int idx = row + x;
                    byte m = moisture[idx];

                    int down = idx + w;
                    if (cracks[down] && moisture[down] + _cfg.DripAmount < m)
                    {
                        _buffer[down] = (byte)(moisture[down] + _cfg.DripAmount);
                        m -= _cfg.DripAmount;
                    }

                    int left = idx - 1;
                    int right = idx + 1;

                    if (moisture[left] + _cfg.DiffuseAmount < m)
                    {
                        _buffer[left] = (byte)(moisture[left] + _cfg.DiffuseAmount);
                        m -= _cfg.DiffuseAmount;
                    }
                    if (moisture[right] + _cfg.DiffuseAmount < m)
                    {
                        _buffer[right] = (byte)(moisture[right] + _cfg.DiffuseAmount);
                        m -= _cfg.DiffuseAmount;
                    }

                    _buffer[idx] = m;
                }
            });

            Buffer.BlockCopy(_buffer, 0, moisture, 0, _buffer.Length);
            Array.Clear(_buffer);
        }
    }
}