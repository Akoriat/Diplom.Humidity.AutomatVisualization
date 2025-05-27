using Common.Entities;
using System;
using System.Collections.Generic;
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
        public double Dz { get; private set; }
        public double Dx { get; private set; }
        private SoilSlice[,] slices;
        public SoilSlice[,] Grid => slices;

        public double RainVolumeLiters { get; set; }

        public double Conductivity { get; set; } = 0.1;
        public double TimeStep { get; set; } = 1.0;


        public SoilSimulator(int width, int height, double dz = 0.02, double dx = 0.02)
        {
            Width = width;
            Height = height;
            slices = new SoilSlice[Height, Width];
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    slices[r, c] = new SoilSlice();
            RainVolumeLiters = 0.0;
            Dz = dz;
            Dx = dx == 0 ? dz : dx;
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

            for (int c = 0; c < Width; c++)
                for (int r = 0; r < Height - 1; r++)
                {
                    double kUp = slices[r, c].HydraulicConductivity();
                    double kDown = slices[r + 1, c].HydraulicConductivity();
                    double k = 0.5 * (kUp + kDown);
                    double diff = slices[r, c].WaterPotential() - slices[r + 1, c].WaterPotential();
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
                    double k = 0.5 * (kUp + kDown);
                    double diff = slices[r, c].WaterPotential() - slices[r, c + 1].WaterPotential();
                    double flow = k * diff * TimeStep / Dx;
                    //double diff = slices[r, c].Moisture - slices[r, c + 1].Moisture;
                    //double k = slices[r, c].IsCracked || slices[r, c + 1].IsCracked ? Conductivity * 10 : Conductivity;
                    //double flow = k * diff * TimeStep;

                    newMoisture[r, c] -= flow;
                    newMoisture[r, c + 1] += flow;
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
        }


        public void SaveState(string filePath) { }
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
