namespace Common.Entities;

using System;

public sealed record SimulationSettings(
    int Width,
    int Height,
    int RandomSeed,
    int CrackDensity,
    double ForkChance,
    double ZigzagChance,
    byte DripAmount,
    byte DiffuseAmount)
{
    public static SimulationSettings CreateDefault(int width, int height) => new(
        width,
        height,
        Environment.TickCount,
        CrackDensity: 10,
        ForkChance: 0.05,
        ZigzagChance: 0.20,
        DripAmount: 4,
        DiffuseAmount: 2);
}