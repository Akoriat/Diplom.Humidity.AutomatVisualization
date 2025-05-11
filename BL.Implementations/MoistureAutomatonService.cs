namespace BL.Implementations;

public sealed class MoistureAutomatonService
{
    private readonly SoilSlice _soil;
    private readonly HumidityModel _model;

    public MoistureAutomatonService(SoilSlice soil, HumidityModel model)
    {
        _soil = soil;
        _model = model;
    }

    public void Tick()
    {
        int w = _soil.Width;
        int h = _soil.Height;

        for (int y = h - 2; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                ref byte cur = ref _soil.Moisture(x, y);
                ref byte below = ref _soil.Moisture(x, y + 1);
                cur = _model.Eval(cur, ref below);
            }
        }
    }
}
