using Common.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Implementations
{
    public class SoilSimulator
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public double Dz { get; set; }
        public double Dx { get; set; }
        private SoilSlice[,] slices;
        public SoilSlice[,] Grid => slices;

        public double RainVolumeLiters { get; set; }

        public double Conductivity { get; set; } = 0.1;
        public double TimeStep { get; set; } = 1.0;

        public event Action<double>? StepCompleted;

        public SoilSimulator(int width, int height, double dz = 0.02, double dx = 0.03)
        {
            Width = width;
            Height = height;
            slices = new SoilSlice[Height, Width];
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    slices[r, c] = new SoilSlice();
            RainVolumeLiters = 0.0;
            Dz = dz;
            if (dx <= 0.0) dx = dz;
            Dx = dx;
        }
        public void ExportCsv(string filePath)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            for (int y = 0; y < Height; y++)
            {
                var values = new string[Width];
                for (int x = 0; x < Width; x++)
                    values[x] = slices[y, x].Moisture.ToString("F4", CultureInfo.InvariantCulture);
                writer.WriteLine(string.Join(';', values));
            }
        }
        public void Step()
        {
            double[,] newMoisture = new double[Height, Width];
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    newMoisture[r, c] = slices[r, c].Moisture;

            if (RainVolumeLiters > 0.0)
            {
                int cracked = 0;
                for (int c = 0; c < Width; c++)
                    if (slices[0, c].IsCracked)
                        cracked++;

                if (cracked > 0)
                {
                    double addVolume = Math.Min(1.0, RainVolumeLiters);
                    double perCrack = addVolume / cracked;

                    for (int c = 0; c < Width; c++)
                        if (slices[0, c].IsCracked)
                            newMoisture[0, c] += perCrack;

                    RainVolumeLiters -= addVolume;
                }
            }

            double dxy = Math.Sqrt(Dx * Dx + Dz * Dz);

            for (int c = 0; c < Width; c++)
                for (int r = 0; r < Height - 1; r++)
                {
                    double kUp = slices[r, c].HydraulicConductivity();
                    double kDown = slices[r + 1, c].HydraulicConductivity();
                    double hDiff = kUp - kDown;
                    double k = Math.Max(1e-10, 0.5 * (kUp + kDown));
                    double diff = slices[r, c].WaterPotential() - slices[r + 1, c].WaterPotential();
                    if (double.IsInfinity(hDiff))
                        continue;
                    double flow = k * diff * TimeStep / Dz;
                    //double diff = slices[r, c].Moisture - slices[r + 1, c].Moisture;
                    //double k = slices[r, c].IsCracked || slices[r + 1, c].IsCracked ? Conductivity * 10 : Conductivity;
                    //double flow = k * diff * TimeStep;
                    newMoisture[r, c] -= flow;
                    newMoisture[r + 1, c] += flow;
                }

            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width - 1; c++)
                {
                    double kUp = slices[r, c].HydraulicConductivity();
                    double kDown = slices[r, c + 1].HydraulicConductivity();
                    double hDiff = kUp - kDown;
                    double k = Math.Max(1e-10, 0.5 * (kUp + kDown));
                    double diff = slices[r, c].WaterPotential() - slices[r, c + 1].WaterPotential();
                    if (double.IsInfinity(hDiff))
                        continue;
                    double flow = k * diff * TimeStep / Dx;
                    //double diff = slices[r, c].Moisture - slices[r, c + 1].Moisture;
                    //double k = slices[r, c].IsCracked || slices[r, c + 1].IsCracked ? Conductivity * 10 : Conductivity;
                    //double flow = k * diff * TimeStep;

                    newMoisture[r, c] -= flow;
                    newMoisture[r, c + 1] += flow;
                }

            for (int r = 0; r < Height - 1; r++)
                for (int c = 0; c < Width - 1; c++)
                {
                    double k1 = slices[r, c].HydraulicConductivity();
                    double k2 = slices[r + 1, c + 1].HydraulicConductivity();
                    double k = Math.Max(1e-10, 0.5 * (k1 + k2));

                    double diff = slices[r, c].WaterPotential() -
                                  slices[r + 1, c + 1].WaterPotential();
                    if (double.IsInfinity(diff) || double.IsNaN(diff))
                        continue;

                    double flow = k * diff * TimeStep / dxy;

                    newMoisture[r, c] -= flow;
                    newMoisture[r + 1, c + 1] += flow;
                }

            for (int r = 0; r < Height - 1; r++)
                for (int c = 1; c < Width; c++)
                {
                    double k1 = slices[r, c].HydraulicConductivity();
                    double k2 = slices[r + 1, c - 1].HydraulicConductivity();
                    double k = Math.Max(1e-10, 0.5 * (k1 + k2));

                    double diff = slices[r, c].WaterPotential() -
                                  slices[r + 1, c - 1].WaterPotential();
                    if (double.IsInfinity(diff) || double.IsNaN(diff))
                        continue;

                    double flow = k * diff * TimeStep / dxy;

                    newMoisture[r, c] -= flow;
                    newMoisture[r + 1, c - 1] += flow;
                }

            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                {
                    //if (newMoisture[r, c] < 0) newMoisture[r, c] = 0;
                    //slices[r, c].Moisture = newMoisture[r, c];
                    double θmin = slices[r, c].ThetaR;     
                    double θmax = slices[r, c].ThetaS;          
                    if (newMoisture[r, c] < θmin) newMoisture[r, c] = θmin;
                    if (newMoisture[r, c] > θmax) newMoisture[r, c] = θmax;
                    slices[r, c].Moisture = newMoisture[r, c];
                }

            double mean = slices
                .Cast<SoilSlice>()
                .Average(s => s.Moisture);

            StepCompleted?.Invoke(mean);
        }


        public void SaveState(string filePath)
        {
            using var writer = new BinaryWriter(File.Open(filePath, FileMode.Create));

            writer.Write(Width);              
            writer.Write(Height);             
            writer.Write(Conductivity);       
            writer.Write(RainVolumeLiters);   

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var slice = slices[y, x];
                    writer.Write(slice.Moisture);  
                    writer.Write(slice.IsCracked); 
                }
        }

        public static SoilSimulator LoadState(string filePath)
        {
            using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                double conductivity = reader.ReadDouble();
                double rainVolume = reader.ReadDouble();

                var sim = new SoilSimulator(width, height)
                {
                    Conductivity = conductivity,
                    RainVolumeLiters = rainVolume
                };

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        sim.Grid[y, x].Moisture = reader.ReadDouble();
                        sim.Grid[y, x].IsCracked = reader.ReadBoolean();
                    }

                return sim;
            }
        }
    }

}
