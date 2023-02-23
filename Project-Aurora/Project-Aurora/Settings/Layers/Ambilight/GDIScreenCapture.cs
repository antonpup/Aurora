using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Aurora.Settings.Layers.Ambilight;

internal sealed class GdiScreenCapture : IScreenCapture
{
    private Bitmap _targetBitmap = new(8, 8);
    private Size _targetSize = new(8, 8);

    private Graphics _graphics = Graphics.FromImage(new Bitmap(8, 8));

    public void Capture(Rectangle desktopRegion, Action<Bitmap?> action)
    {
        if (_targetSize != desktopRegion.Size)
        {
            _targetBitmap = new Bitmap(desktopRegion.Width, desktopRegion.Height);
            _targetSize = desktopRegion.Size;
            _graphics.Dispose();
            _graphics = Graphics.FromImage(_targetBitmap);
        }
        _graphics.CompositingMode = CompositingMode.SourceCopy;
        _graphics.CopyFromScreen(desktopRegion.Location, Point.Empty, _targetSize);

        action.Invoke(_targetBitmap);
    }

    public IEnumerable<string> GetDisplays() =>
        Screen.AllScreens.Select((s, index) =>
            $"Display {index + 1}: X:{s.Bounds.X}, Y:{s.Bounds.Y}, W:{s.Bounds.Width}, H:{s.Bounds.Height}");

    public void Dispose()
    {
        _graphics.Dispose();
    }
}