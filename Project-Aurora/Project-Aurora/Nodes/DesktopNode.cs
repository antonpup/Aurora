using Aurora.Utils;

namespace Aurora.Nodes;

public class DesktopNode : Node
{
    public int AccentA { get; private set; }
    public int AccentR { get; private set; }
    public int AccentG { get; private set; }
    public int AccentB { get; private set; }
    
    private readonly RegistryWatcher _accentColorWatcher = new(
        RegistryHiveOpt.CurrentUser, @"SOFTWARE\\Microsoft\\Windows\\DWM", "AccentColor");

    public DesktopNode()
    {
        _accentColorWatcher.RegistryChanged += UpdateAccentColor;
        _accentColorWatcher.StartWatching();
    }

    private void UpdateAccentColor(object? sender, RegistryChangedEventArgs registryChangedEventArgs)
    {
        var data = registryChangedEventArgs.Data;
        switch (data)
        {
            case null:
                return;
            case int color:
                var a = (byte)((color >> 24) & 0xFF);
                var b = (byte)((color >> 16) & 0xFF);
                var g = (byte)((color >> 8) & 0xFF);
                var r = (byte)((color >> 0) & 0xFF);

                AccentA = a;
                AccentR = r;
                AccentG = g;
                AccentB = b;
                break;
        }
    }
}