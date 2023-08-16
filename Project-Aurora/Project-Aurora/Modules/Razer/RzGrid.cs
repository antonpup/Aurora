using System;
using RazerSdkReader;
using RazerSdkReader.Structures;

namespace Aurora.Modules.Razer;

public interface IRzGrid
{
    static readonly ChromaColor EmptyColor = new();

    public bool IsDirty { get; set; }
    public IColorProvider Provider { get; set; }
    
    public ChromaColor this[int index] => throw new NotImplementedException();
}

public class ConnectedGrid : IRzGrid
{
    public bool IsDirty { get; set; } = true;
    public IColorProvider? Provider { get; set; }

    public ChromaColor this[int index]
    {
        get
        {
            if (IsDirty)
            {
                //_provider?.Update();
                IsDirty = false;
            }
            return Provider?.GetColor(index) ?? IRzGrid.EmptyColor;
        }
    }
}

public class EmptyGrid : IRzGrid
{
    public bool IsDirty { get; set; } = true;
    public IColorProvider? Provider { get; set; }

    public ChromaColor this[int index] => IRzGrid.EmptyColor;
}