using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace UI;
public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<ISeries> MoistureSeries { get; } = new()
    {
        new LineSeries<double>
        {
            Name   = "Средняя влажность",
            Values = new ObservableCollection<double>(),
            Fill = null 
        }
    };

    public void OnSimulationStepCompleted(double meanMoisture)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var lineSeries = (LineSeries<double>)MoistureSeries[0];
            ((ObservableCollection<double>)lineSeries.Values).Add(meanMoisture);
            if (((ObservableCollection<double>)lineSeries.Values).Count > 600)
                ((ObservableCollection<double>)lineSeries.Values).RemoveAt(0);
        });
    }
}
