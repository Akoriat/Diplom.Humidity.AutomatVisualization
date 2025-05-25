namespace Common.Configs;

public class MoistureSimulationConfig
{
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }
    public double Conductivity { get; set; }
    public double TimeStep { get; set; }
    public double InitialMoisture { get; set; }
    public double CrackProbability { get; set; }

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
