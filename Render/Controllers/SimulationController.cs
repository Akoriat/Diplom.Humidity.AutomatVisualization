using BL.Implementations;
using Common.Configs;
using Common.Entities;
using Render.Rendering;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Render.Controllers;

public sealed class SimulationController
{
    private readonly SimulationSettings _cfg;
    private readonly SoilSlice _slice;
    private readonly FissurePainterService _painter;
    private readonly MoistureAutomatonService _automaton;
    private readonly DispatcherTimer _timer;
    private readonly ColorRamp _ramp = new ColorRamp();
    private readonly Action<WriteableBitmap> _onFrame;

    public SimulationController(Action<WriteableBitmap> onFrame,
                                int width = 256,
                                int height = 128,
                                int fps = 30)
    {
        double clientW = SystemParameters.WorkArea.Width - 40;
        double clientH = SystemParameters.WorkArea.Height - 140;

        int cpX = (int)Math.Floor(clientW / width);
        int cpY = (int)Math.Floor(clientH / height);
        RenderConfig.CellPx = Math.Max(2, Math.Min(cpX, cpY));

        _cfg = SimulationSettings.CreateDefault(width, height);
        _slice = new SoilSlice(_cfg);

        _painter = new FissurePainterService(_cfg);
        _painter.GenerateCracks(_slice);

        _automaton = new MoistureAutomatonService(
                 _slice,
                 new HumidityModel(_cfg.DripAmount));

        _onFrame = onFrame;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / fps)
        };
        _timer.Tick += (_, _) => Tick();

        PreFillSurface();
        _onFrame(SliceRenderer.Render(_slice, _ramp));
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    public void Step()
    {
        if (!_timer.IsEnabled) Tick();
    }

    private void Tick()
    {
        _automaton.Tick();
        _onFrame(SliceRenderer.Render(_slice, _ramp));
    }
    private void PreFillSurface()
    {
        foreach (int idx in _slice.CrackCells)
            _slice.MoistureArray[idx] = 255;

    }
    public void RecomputeScale(int clientW, int clientH)
    {
        int cpX = clientW / _slice.Width;
        int cpY = clientH / _slice.Height;
        int newCp = Math.Max(2, Math.Min(cpX, cpY));

        if (newCp != RenderConfig.CellPx)
        {
            RenderConfig.CellPx = newCp;
            _onFrame(SliceRenderer.Render(_slice, _ramp));
        }
    }
}
