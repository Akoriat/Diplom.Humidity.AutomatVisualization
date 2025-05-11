using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Render.Controllers;

namespace UI
{
    public partial class MainWindow : Window
    {
        private readonly SimulationController _sim;
        private readonly Stopwatch _fpsWatch = new();
        private int _frames;

        public MainWindow()
        {
            InitializeComponent();
            _sim = new SimulationController(OnFrameReady);
        }
        private void OnFrameReady(WriteableBitmap bmp)
        {
            SurfaceImage.Source = bmp;
            if (!_fpsWatch.IsRunning) _fpsWatch.Start();
            if (++_frames == 30)
            {
                double fps = 30 / _fpsWatch.Elapsed.TotalSeconds;
                FpsLabel.Text = ((int)fps).ToString();
                _frames = 0;
                _fpsWatch.Restart();
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e) => _sim.Start();
        private void Stop_Click(object sender, RoutedEventArgs e) => _sim.Stop();
        private void Step_Click(object sender, RoutedEventArgs e) => _sim.Step();

        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);
            _sim.RecomputeScale((int)info.NewSize.Width, (int)info.NewSize.Height);
        }

    }
}