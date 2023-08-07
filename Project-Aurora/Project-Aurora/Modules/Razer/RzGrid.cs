using System;
using RazerSdkWrapper.Data;

namespace Aurora.Modules.Razer;

public interface IRzGrid
{
    static readonly RzColor EmptyColor = new(0, 0, 0, 0);
    public bool IsDirty { get; set; }
    public RzColor this[int index] => throw new NotImplementedException();
    public RzColor this[int grid, int index] => throw new NotImplementedException();
}

public class ConnectedGrid : IRzGrid
{
    public bool IsDirty { get; set; } = true;
    
    private readonly int _size;
    private readonly AbstractColorDataProvider? _provider;

    public ConnectedGrid(int size, AbstractColorDataProvider? provider)
    {
        _size = size;
        _provider = provider;
    }

    public RzColor this[int index]
    {
        get
        {
            if (IsDirty)
            {
                _provider?.Update();
                IsDirty = false;
            }
            return _provider?.GetZoneColor(index) ?? IRzGrid.EmptyColor;
        }
    }

    public RzColor this[int grid, int index]
    {
        get
        {
            if (IsDirty)
            {
                _provider?.Update();
                IsDirty = false;
            }
            return _provider?.GetZoneColor(index, grid) ?? IRzGrid.EmptyColor;
        }
    }
}

public class EmptyGrid : IRzGrid
{
    public bool IsDirty { get; set; } = true;
    public RzColor this[int grid, int index] => IRzGrid.EmptyColor;
    public RzColor this[int index] => IRzGrid.EmptyColor;
}