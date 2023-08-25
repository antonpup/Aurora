using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace Aurora.Settings.Layers.Ambilight;

internal sealed class DxScreenCapture : IScreenCapture
{
    public event EventHandler<Bitmap>? ScreenshotTaken;
    
    private static readonly IDictionary<Output5, DesktopDuplicator> Duplicators = new Dictionary<Output5, DesktopDuplicator>();

    private static readonly Semaphore Semaphore = new(1, 1);
    private Rectangle _currentBounds = Rectangle.Empty;
    private DesktopDuplicator? _desktopDuplicator;

    public void Capture(Rectangle desktopRegion)
    {
        SetTarget(desktopRegion);
        try{
            Semaphore.WaitOne();
            var bitmap = _currentBounds.IsEmpty ? null : _desktopDuplicator?.Capture(_currentBounds, 5000);
            if (bitmap == null)
            {
                if (_desktopDuplicator?.IsDisposed ?? false)
                {
                    _desktopDuplicator = null;
                }
                return;
            }
            ScreenshotTaken?.Invoke(this, bitmap);
            bitmap.Dispose();
        }finally{
            Semaphore.Release();
        }
    }

    private void SetTarget(Rectangle desktopRegion)
    {
        var outputs = GetAdapters();
        Adapter1? currentAdapter = null;
        Output5? currentOutput = null;
        
        foreach (var (adapter, output) in outputs)
        {
            if (!RectangleContains(output.Description.DesktopBounds, desktopRegion))
            {
                continue;
            }

            currentAdapter = adapter;
            currentOutput = output;
            break;
        }

        if (currentAdapter == null || currentOutput == null)
        {
            return;
        }

        var desktopBounds = currentOutput.Description.DesktopBounds;
        var x = Math.Max(0, desktopRegion.Left - desktopBounds.Left);
        var y = Math.Max(0, desktopRegion.Top - desktopBounds.Top);
        var screenWindowRectangle = new Rectangle(
            x,
            y,
            Math.Min(desktopRegion.Width, desktopBounds.Right - x),
            Math.Min(desktopRegion.Height, desktopBounds.Bottom - y)
        );
            
        if (screenWindowRectangle == _currentBounds && _desktopDuplicator != null)
        {
            return;
        }

        _currentBounds = screenWindowRectangle;

        try
        {
            Semaphore.WaitOne();

            if (Duplicators.TryGetValue(currentOutput, out _desktopDuplicator)) return;
            _desktopDuplicator = new DesktopDuplicator(currentOutput, currentAdapter);
            Duplicators.Add(currentOutput, _desktopDuplicator);
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

    private static Factory1? _factory1;
    private static List<(Adapter1 Adapter, Output5 Output)>? _outputs;
    private static IEnumerable<(Adapter1 Adapter, Output5 Output)> GetAdapters()
    {
        if (_factory1 != null && _factory1.IsCurrent && _outputs != null) return _outputs;
        _factory1?.Dispose();
        _factory1 = new Factory1();
        WeakEventManager<Factory1, EventArgs>.AddHandler(_factory1, nameof(_factory1.Disposing), FactoryDisposed);
        _outputs = _factory1.Adapters1
            .SelectMany(m => m.Outputs.Select(n =>
            {
                var o = n.QueryInterface<Output5>();
                WeakEventManager<Output5, EventArgs>.AddHandler(o, nameof(o.Disposing), OutputDisposed);
                return (M: m, o);
            }))
            .ToList();

        return _outputs;
    }

    private static void FactoryDisposed(object? sender, EventArgs e)
    {
        try
        {
            Semaphore.WaitOne();
            _outputs = null;
            Duplicators.Clear();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static void OutputDisposed(object? sender, EventArgs e)
    {
        try
        {
            Semaphore.WaitOne();
            var output5 = sender as Output5;
            Duplicators.Remove(output5);
            _outputs = null;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static bool RectangleContains(RawRectangle containingRectangle, Rectangle rec)
    {
        var center = rec.Location + rec.Size / 2;
        return containingRectangle.Left <= center.X && containingRectangle.Right > center.X &&
               containingRectangle.Top <= center.Y && containingRectangle.Bottom > center.Y;
    }

    public void Dispose()
    {
        try
        {
            Semaphore.WaitOne();
            _desktopDuplicator = null;
        }
        finally
        {
            Semaphore.Release();
        }
    }
}