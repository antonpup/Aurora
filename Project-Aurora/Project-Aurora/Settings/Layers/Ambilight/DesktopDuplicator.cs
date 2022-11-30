// Based on https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct3D11.1/ScreenCapture/Program.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Aurora.Settings.Layers.Ambilight;

public class DesktopDuplicator : IDisposable
{
    private static readonly IDictionary<Adapter1, OutputDuplication> Duplicators = new Dictionary<Adapter1, OutputDuplication>();

    private readonly Device _device;
    private readonly OutputDuplication _deskDupl;

    private readonly Texture2D _desktopImageTexture;

    private readonly Rectangle _rect;

    public DesktopDuplicator(Adapter1 adapter, Output1 output, Rectangle rect)
    {
        Global.logger.Info("Starting desktop duplicator");
        _rect = rect;
        _device = new Device(adapter);
        var textureDesc = new Texture2DDescription
        {
            CpuAccessFlags = CpuAccessFlags.Read,
            BindFlags = BindFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Width = output.Description.DesktopBounds.Right - output.Description.DesktopBounds.Left,
            Height = output.Description.DesktopBounds.Bottom - output.Description.DesktopBounds.Top,
            OptionFlags = ResourceOptionFlags.None,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = { Count = 1, Quality = 0 },
            Usage = ResourceUsage.Staging
        };

        if (!Duplicators.TryGetValue(adapter, out _deskDupl))
        {
            _deskDupl = output.DuplicateOutput(_device);
            Duplicators.Add(adapter, _deskDupl);
        }

        _desktopImageTexture = new Texture2D(_device, textureDesc);
    }

    public Bitmap Capture(int timeout)
    {
        SharpDX.DXGI.Resource desktopResource;
        if (_deskDupl.IsDisposed || _device.IsDisposed) 
            return null;

        try {
            _deskDupl.TryAcquireNextFrame(timeout, out _, out desktopResource);
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.WaitTimeout)
        {
            Global.logger.Debug(String.Format("Timeout of {0}ms exceeded while acquiring next frame", timeout));
            return null;
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.AccessLost)
        {
            // Can happen when going fullscreen / exiting fullscreen
            return null;
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.AccessDenied)
        {
            // Happens when locking PC
            return null;
        }
        catch (SharpDXException e) when (e.ResultCode.Failure)
        {
            Global.logger.Warn(e.Message);
            return null;
        }

        using (desktopResource) {
            using (var tempTexture = desktopResource.QueryInterface<Texture2D>())
                _device.ImmediateContext.CopyResource(tempTexture, _desktopImageTexture);
        }

        bool disposed = ReleaseFrame();
        if (disposed)
            return null;

        var mapSource = _device.ImmediateContext.MapSubresource(_desktopImageTexture, 0, MapMode.Read, MapFlags.None);

        try
        {
            return ProcessFrame(mapSource.DataPointer, mapSource.RowPitch);
        }
        finally
        {
            if (!_device.IsDisposed && !_device.ImmediateContext.IsDisposed && !_desktopImageTexture.IsDisposed)
            {
                _device.ImmediateContext.UnmapSubresource(_desktopImageTexture, 0);
            }
        }
    }

    Bitmap ProcessFrame(IntPtr sourcePtr, int SourceRowPitch)
    {
        var frame = new Bitmap(_rect.Width, _rect.Height, PixelFormat.Format32bppRgb);
        // Copy pixels from screen capture Texture to GDI bitmap
        var mapDest = frame.LockBits(_rect with {X = 0, Y = 0}, ImageLockMode.WriteOnly, frame.PixelFormat);
        var screenY = _rect.Top;
        var sizeInBytesToCopy = _rect.Width * 4;
        for (var y = 0; screenY < _rect.Bottom; screenY++, y++)
        {
            var mapDestStride = mapDest.Scan0 + y * mapDest.Stride;
            var sourceRowPitch = sourcePtr + screenY * SourceRowPitch + _rect.Left * 4;
            Utilities.CopyMemory(mapDestStride, sourceRowPitch, sizeInBytesToCopy);
        }
        // Release source and dest locks
        frame.UnlockBits(mapDest);

        return frame;
    }

    bool ReleaseFrame()
    {
        try
        {
            _deskDupl.ReleaseFrame();
            return _deskDupl.IsDisposed;
        }
        catch (SharpDXException e)
        {
            if (e.ResultCode.Failure)
            {
                Global.logger.Warn(e.Message);
            }
            return true;
        }
    }

    public void Dispose()
    {
        _device?.Dispose();
        _deskDupl?.Dispose();
        _desktopImageTexture?.Dispose();
    }
}