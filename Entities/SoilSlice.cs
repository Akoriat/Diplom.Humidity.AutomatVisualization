namespace Common.Entities;
public class SoilSlice
{
    public double Moisture { get; set; }

    public bool IsCracked { get; set; }

    public SoilSlice()
    {
        Moisture = 0.0;
        IsCracked = false;
    }
}
