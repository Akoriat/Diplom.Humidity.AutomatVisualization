using BL.Implementations;
using Common.Configs;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UI
{
    public partial class MainWindow : Window
    {
        private SoilSimulator simulator;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
        }
        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (simulator == null)
                return;

            for (int r = 0; r < simulator.Height; r++)
            {
                for (int c = 0; c < simulator.Width; c++)
                {
                    var slice = simulator.Grid[r, c];
                    slice.ThetaS = ThetaSSlider.Value;
                    slice.ThetaR = ThetaRSlider.Value;
                    slice.Alpha = AlphaSlider.Value;
                    slice.N = NSlider.Value;
                    slice.Ks = KsSlider.Value;
                }
            }

            simulator.Dz = DzSlider.Value;
            simulator.Dx = DxSlider.Value;

            MessageBox.Show("Параметры СВП применены ко всем ячейкам.", "Настройки",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }
        //private void StartButton_Click(object sender, RoutedEventArgs e)
        //{
        //    timer.Stop();

        //    int width = (int)WidthSlider.Value;
        //    int height = (int)HeightSlider.Value;
        //    double conductivity = ConductivitySlider.Value;
        //    double rainVolume = RainSlider.Value;
        //    double initialMoisture = InitialMoistureSlider.Value / 100.0;
        //    double crackChance = CrackChanceSlider.Value / 100.0;

        //    simulator = new SoilSimulator(width, height);

        //    simulator.Conductivity = conductivity;
        //    simulator.RainVolumeLiters = rainVolume;
        //    simulator.TimeStep = TimeStepSlider.Value;

        //    for (int r = 0; r < simulator.Height; r++)
        //    {
        //        for (int c = 0; c < simulator.Width; c++)
        //        {
        //            simulator.Grid[r, c].Moisture = initialMoisture;
        //        }
        //    }

        //    var rand = new Random();
        //    for (int r = 0; r < simulator.Height; r++)
        //    {
        //        for (int c = 0; c < simulator.Width; c++)
        //        {
        //            if (rand.NextDouble() < crackChance)
        //            {
        //                simulator.Grid[r, c].IsCracked = true;
        //            }
        //        }
        //    }

        //    timer.Start();
        //}

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (simulator != null)
            {
                if (!timer.IsEnabled)
                    timer.Start();
                return;
            }

            InitializeSimulator();
            timer.Start();
        }

        private void InitializeSimulator()
        {
            int width = (int)WidthSlider.Value;
            int height = (int)HeightSlider.Value;
            double conductivity = ConductivitySlider.Value;
            double rainVolume = RainSlider.Value;
            double initialMoisture = InitialMoistureSlider.Value / 100.0;
            double crackChance = CrackChanceSlider.Value / 100.0;
            double dz = DzSlider.Value;
            double dx = DxSlider.Value;

            simulator = new SoilSimulator(width, height, dz, dx);

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
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (simulator == null)
                return;

            simulator.Step();

            double mean = 0;
            for (int r = 0; r < simulator.Height; r++)
                for (int c = 0; c < simulator.Width; c++)
                    mean += simulator.Grid[r, c].Moisture;
            mean /= simulator.Height * simulator.Width;

            var vm = (MainViewModel)DataContext;
            vm.OnSimulationStepCompleted(mean);

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
            InitializeSimulator();
            timer.Start();
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
        private void GraphButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            var win = new GraphWindow(vm);
            win.Owner = this;
            win.Show();
        }
    }
}
