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
        private SoilSlice[,] slices;
        public SoilSlice[,] Grid => slices;

        public double RainVolumeLiters { get; set; }

        public double Conductivity { get; set; } = 0.1;

        public SoilSimulator(int width, int height)
        {
            Width = width;
            Height = height;
            slices = new SoilSlice[Height, Width];
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    slices[r, c] = new SoilSlice();
            RainVolumeLiters = 0.0;
        }

        public void Step()
        {
            double[,] newMoisture = new double[Height, Width];
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    newMoisture[r, c] = slices[r, c].Moisture;

            if (RainVolumeLiters > 0.0)
            {
                int available = 0;
                for (int c = 0; c < Width; c++)
                    if (!slices[0, c].IsCracked)
                        available++;
                if (available > 0)
                {
                    double addVolume = Math.Min(1.0, RainVolumeLiters);
                    double perColumn = addVolume / available;
                    for (int c = 0; c < Width; c++)
                    {
                        if (!slices[0, c].IsCracked)
                            newMoisture[0, c] += perColumn;
                    }
                    RainVolumeLiters -= addVolume;
                }
            }

            for (int c = 0; c < Width; c++)
            {
                for (int r = 0; r < Height - 1; r++)
                {
                    double currentH = slices[r, c].Moisture;
                    double belowH = slices[r + 1, c].Moisture;
                    double diff = currentH - belowH;
                    double flow = Conductivity * diff;
                    newMoisture[r, c] -= flow;
                    newMoisture[r + 1, c] += flow;
                }
            }

            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    if (newMoisture[r, c] < 0.0)
                        newMoisture[r, c] = 0.0;
                    slices[r, c].Moisture = newMoisture[r, c];
                }
            }
        }

        public void SaveState(string filePath) {  }
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
