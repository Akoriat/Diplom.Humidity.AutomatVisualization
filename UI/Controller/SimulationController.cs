using BL.Implementations;
using Common.Configs;
using System.Windows.Controls;
using System.Windows.Threading;

namespace UI.Controller
{
    public class SimulationController
    {
        private readonly SoilSimulator service;
        private readonly MoistureSimulationConfig config;
        private readonly SliceRenderer renderer;
        private readonly DispatcherTimer timer;
        private readonly Image imageControl;
        private readonly TextBlock fpsTextBlock;
        private readonly TextBlock paramsTextBlock;

        private int frameCount;
        private DateTime lastFpsTime;

        public bool IsRunning => timer.IsEnabled;

        public SimulationController(SoilSimulator service, MoistureSimulationConfig config,
                                    Image imageControl, TextBlock fpsText, TextBlock paramsText)
        {
            this.service = service;
            this.config = config;
            this.imageControl = imageControl;
            fpsTextBlock = fpsText;
            paramsTextBlock = paramsText;

            renderer = new SliceRenderer(service.Width, service.Height);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;

            frameCount = 0;
            lastFpsTime = DateTime.Now;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            service.Step();
            var bmp = renderer.Render(service.Grid);
            imageControl.Source = bmp;

            frameCount++;
            DateTime now = DateTime.Now;
            TimeSpan interval = now - lastFpsTime;
            if (interval.TotalSeconds >= 1.0)
            {
                double fps = frameCount / interval.TotalSeconds;
                fpsTextBlock.Text = $"FPS: {fps:F1}";
                frameCount = 0;
                lastFpsTime = now;
            }
            paramsTextBlock.Text = $"K={config.Conductivity:F2}, dt={config.TimeStep:F3}";
        }

        public void RenderCurrent()
        {
            var bmp = renderer.Render(service.Grid);
            imageControl.Source = bmp;
            fpsTextBlock.Text = "FPS: 0";
            paramsTextBlock.Text = $"K={config.Conductivity:F2}, dt={config.TimeStep:F3}";
        }

        public void Start()
        {
            if (!timer.IsEnabled)
            {
                frameCount = 0;
                lastFpsTime = DateTime.Now;
                timer.Start();
            }
        }

        public void Stop()
        {
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
        }
    }
}
