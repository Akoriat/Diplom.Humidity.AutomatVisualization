public sealed class HumidityModel
{
    private readonly byte _drip;           // сколько «капель» переносим за тик

    public HumidityModel(byte drip = 5)    // передаём из настроек
    {
        _drip = drip;
    }

    public byte Eval(byte current, ref byte below)
    {
        if (current == 0 || below == 255)
            return current;                // сухо либо «чаша» заполнена

        byte moved = (byte)Math.Min(_drip, current);
        if (moved > 255 - below)           // не даём нижней ячейке переполниться
            moved = (byte)(255 - below);

        below += moved;
        current -= moved;
        return current;
    }
}
