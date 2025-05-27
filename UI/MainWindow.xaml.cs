using BL.Implementations;
using Common.Configs;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace UI
{
    public partial class MainWindow : Window
    {
        private SoilSimulator simulator;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();

            int width = (int)WidthSlider.Value;
            int height = (int)HeightSlider.Value;
            double conductivity = ConductivitySlider.Value;
            double rainVolume = RainSlider.Value;
            double initialMoisture = InitialMoistureSlider.Value / 100.0;
            double crackChance = CrackChanceSlider.Value / 100.0;

            simulator = new SoilSimulator(width, height);

            simulator.Conductivity = conductivity;
            simulator.RainVolumeLiters = rainVolume;
            simulator.TimeStep = TimeStepSlider.Value;

            for (int r = 0; r < simulator.Height; r++)
            {
                for (int c = 0; c < simulator.Width; c++)
                {
                    simulator.Grid[r, c].Moisture = initialMoisture;
                }
            }

            var rand = new Random();
            for (int r = 0; r < simulator.Height; r++)
            {
                for (int c = 0; c < simulator.Width; c++)
                {
                    if (rand.NextDouble() < crackChance)
                    {
                        simulator.Grid[r, c].IsCracked = true;
                    }
                }
            }

            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (simulator == null)
                return;

            simulator.Step();

            var renderer = new SliceRenderer(simulator.Width, simulator.Height);
            var bmp = renderer.Render(simulator.Grid);
            SimulationImage.Source = bmp;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();

            
            StartButton_Click(sender, e);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (simulator == null)
                return;

            var dlg = new SaveFileDialog
            {
                Filter = "Состояние симуляции|*.dat|Все файлы|*.*",
                DefaultExt = "dat"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    simulator.SaveState(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Состояние симуляции|*.dat|Все файлы|*.*",
                DefaultExt = "dat"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    timer.Stop();

                    simulator = SoilSimulator.LoadState(dlg.FileName);

                    WidthSlider.Value = simulator.Width;
                    HeightSlider.Value = simulator.Height;
                    ConductivitySlider.Value = simulator.Conductivity;
                    TimeStepSlider.Value = simulator.RainVolumeLiters;

                    var renderer = new SliceRenderer(simulator.Width, simulator.Height);
                    var bmp = renderer.Render(simulator.Grid);
                    SimulationImage.Source = bmp;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void RainSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (simulator != null)
                simulator.RainVolumeLiters = e.NewValue;
        }

    }
}
