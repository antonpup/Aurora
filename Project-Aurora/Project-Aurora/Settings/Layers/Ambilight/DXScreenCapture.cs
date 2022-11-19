using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace Aurora.Settings.Layers.Ambilight;

internal class DxScreenCapture : IScreenCapture
{
    private static readonly Semaphore Semaphore = new(1, 1);
    private Rectangle _currentBounds;
    private DesktopDuplicator _desktopDuplicator;

    public Bitmap Capture(Rectangle desktopRegion)
    {
        SetTarget(desktopRegion);
        try{
            Semaphore.WaitOne();
            return _currentBounds.IsEmpty ? null : _desktopDuplicator?.Capture(5000);
        }finally{
            Semaphore.Release();
        }
    }

    private void SetTarget(Rectangle desktopRegion)
    {
        var outputs = GetAdapters();
        Adapter1 currentAdapter = null;
        Output1 currentOutput = null;
        if (desktopRegion.Left < 0 || desktopRegion.Top < 0)
        {
            (Adapter1 Adapter, Output1 Output) firstScreen = outputs.First();
            currentAdapter = firstScreen.Adapter;
            currentOutput = firstScreen.Output;
        }
        else
        {
            foreach (var (adapter, output) in outputs)
            {
                if (!RectangleContains(output.Description.DesktopBounds, desktopRegion)) continue;
                currentAdapter = adapter;
                currentOutput = output;
                break;
            }
        }

        if (currentAdapter == null)
        {
            return;
        }

        var desktopBounds = currentOutput.Description.DesktopBounds;
        var screenWindowRectangle = new Rectangle(
            Math.Max(0, desktopRegion.Left - desktopBounds.Left),
            Math.Max(0, desktopRegion.Top - desktopBounds.Top),
            Math.Min(desktopRegion.Width, desktopBounds.Right - desktopRegion.Left),
            Math.Min(desktopRegion.Height, desktopBounds.Bottom - desktopRegion.Top)
        );
            
        if (screenWindowRectangle == _currentBounds && _desktopDuplicator != null)
        {
            return;
        }

        _currentBounds = screenWindowRectangle;

        try
        {
            Semaphore.WaitOne();
            _desktopDuplicator?.Dispose();
            _desktopDuplicator = new DesktopDuplicator(currentAdapter, currentOutput, _currentBounds);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public IEnumerable<string> GetDisplays() => GetAdapters().Select((s, index) =>
    {
        var b = s.Output.Description.DesktopBounds;

        return $"Display {index + 1}: X:{b.Left}, Y:{b.Top}, W:{b.Right - b.Left}, H:{b.Bottom - b.Top}";
    });

    private static IEnumerable<(Adapter1 Adapter, Output1 Output)> GetAdapters()
    {
        using var fac = new Factory1();
        return fac.Adapters1.SelectMany(m => m.Outputs.Select(n => (M: m, n.QueryInterface<Output1>())));
    }

    private static bool RectangleContains(RawRectangle containingRactangle, Rectangle rec)
    {
        return containingRactangle.Left <= rec.X && containingRactangle.Right > rec.X &&
               containingRactangle.Top <= rec.Y && containingRactangle.Bottom > rec.Y;
    }

    public void Dispose()
    {
        try
        {
            Semaphore.WaitOne();
            _desktopDuplicator?.Dispose();
        }
        finally
        {
            Semaphore.Release();
        }
        _desktopDuplicator = null;
    }
}