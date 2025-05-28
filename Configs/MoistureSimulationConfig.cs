using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Common.Configs;

public class MoistureSimulationConfig : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    private int _gridWidth = 100;
    public int GridWidth
    {
        get => _gridWidth;
        set { if (_gridWidth == value) return; _gridWidth = value; OnPropertyChanged(); }
    }
    private int _gridHeight = 100;
    public int GridHeight {
        get => _gridHeight;
        set { if (_gridHeight == value) return; _gridHeight = value; OnPropertyChanged(); }
    }

    private double _conductivity = 0.1;
    public double Conductivity {
        get => _conductivity;
        set { if (_conductivity == value) return; _conductivity = value; OnPropertyChanged(); }
    }

    private double _timeStep = 0.1;
    public double TimeStep {
        get => _timeStep;
        set { if (_timeStep == value) return; _timeStep = value; OnPropertyChanged(); }
    }

    private double _initialMoisture = 0.5;
    public double InitialMoisture {
        get => _initialMoisture;
        set { if (_initialMoisture == value) return; _initialMoisture = value; OnPropertyChanged(); }
    }

    private double _crackProbability;
    public double CrackProbability {
        get => _crackProbability;
        set { if (_crackProbability == value) return; _crackProbability = value; OnPropertyChanged(); }
    }

    private double _thetaS = 0.43;
    public double ThetaS {
        get => _thetaS;
        set { if (_thetaS == value) return; _thetaS = value; OnPropertyChanged(); }
    }

    private double _thetaR = 0.078;
    public double ThetaR {
        get => _thetaR;
        set { if (_thetaR == value) return; _thetaR = value; OnPropertyChanged(); }
    }

    private double _alpha = 0.036;
    public double Alpha {
        get => _alpha;
        set { if (_alpha == value) return; _alpha = value; OnPropertyChanged(); }
    }

    private double _n = 1.56;
    public double N {
        get => _n;
        set { if (_n == value) return; _n = value; OnPropertyChanged(); }
    }

    private double _ks = 5.8e-3;
    public double Ks {
        get => _ks;
        set { if (_ks == value) return; _ks = value; OnPropertyChanged(); }
    }

    public MoistureSimulationConfig()
    {
        GridWidth = 100;
        GridHeight = 100;
        Conductivity = 0.1;
        TimeStep = 0.1;
        InitialMoisture = 0.5;
        CrackProbability = 0.0;
    }
}
