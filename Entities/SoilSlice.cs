namespace Common.Entities;
public class SoilSlice
{
    public double Moisture;
    public bool IsCracked;

    public double ThetaS = 0.43; 
    public double ThetaR = 0.078; 
    public double Alpha = 0.036;
    public double N = 1.56; 
    public double Ks = 5.8e-3; 

    public double Se => Math.Clamp((Moisture - ThetaR) / (ThetaS - ThetaR + 1e-12), 0.0, 1.0);
    public double WaterPotential() 
        => -(1 / Alpha) * Math.Pow(Math.Pow(Se, -1 / N) - 1, 1 - 1 / N);

    public double HydraulicConductivity() 
    {
        double m = 1.0 - 1.0 / N;
        return Ks * Math.Sqrt(Se) * Math.Pow(1 - Math.Pow(1 - Math.Pow(Se, 1 / m), m), 2);
    }
}

