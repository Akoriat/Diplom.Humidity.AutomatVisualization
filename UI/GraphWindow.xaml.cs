using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UI
{
    public partial class GraphWindow : Window
    {
        public GraphWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            var xAxis = ((Axis[])Resources["XAxesArray"])[0];
            xAxis.Labeler = value => $"{value} t";
            var yAxis = ((Axis[])Resources["YAxesArray"])[0];
            yAxis.Labeler = value => $"{value:P0}";
        }
    }
}
