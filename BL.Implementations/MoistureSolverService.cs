using Common.Entities;

public sealed class MoistureSolverService
{
    private readonly SimulationSettings _cfg;
    private readonly byte[] _verticalBuffer;
    private readonly byte[] _horizontalBuffer;

    public MoistureSolverService(SimulationSettings cfg)
    {
        _cfg = cfg;
        int size = cfg.Width * cfg.Height;
        _verticalBuffer = new byte[size];
        _horizontalBuffer = new byte[size];
    }

    public void Tick(SoilSlice slice, int steps = 1)
    {
        var moisture = slice.MoistureArray;
        int w = slice.Width;
        int h = slice.Height;

        for (int s = 0; s < steps; s++)
        {
            Buffer.BlockCopy(moisture, 0, _verticalBuffer, 0, moisture.Length);
            for (int y = 0; y < h - 1; y++)
            {
                int row = y * w;
                int rowDown = (y + 1) * w;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x;
                    int down = rowDown + x;
                    byte current = moisture[idx];
                    byte below = _verticalBuffer[down];
                    if (current > below + _cfg.DripAmount)
                    {
                        byte moved = _cfg.DripAmount;
                        if (moved > 255 - below) moved = (byte)(255 - below);
                        _verticalBuffer[down] += moved;
                        _verticalBuffer[idx] -= moved;
                    }
                }
            }
            Buffer.BlockCopy(_verticalBuffer, 0, moisture, 0, moisture.Length);

            Buffer.BlockCopy(moisture, 0, _horizontalBuffer, 0, moisture.Length);
            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 1; x < w - 1; x++)
                {
                    int idx = row + x;
                    byte current = moisture[idx];

                    int left = idx - 1;
                    byte mLeft = _horizontalBuffer[left];
                    if (current > mLeft + _cfg.DiffuseAmount)
                    {
                        byte moved = _cfg.DiffuseAmount;
                        if (moved > 255 - mLeft) moved = (byte)(255 - mLeft);
                        _horizontalBuffer[left] += moved;
                        _horizontalBuffer[idx] -= moved;
                    }

                    int right = idx + 1;
                    byte mRight = _horizontalBuffer[right];
                    if (current > mRight + _cfg.DiffuseAmount)
                    {
                        byte moved = _cfg.DiffuseAmount;
                        if (moved > 255 - mRight) moved = (byte)(255 - mRight);
                        _horizontalBuffer[right] += moved;
                        _horizontalBuffer[idx] -= moved;
                    }
                }
            }
            Buffer.BlockCopy(_horizontalBuffer, 0, moisture, 0, moisture.Length);
        }
    }
}
