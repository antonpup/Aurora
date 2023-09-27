// Based on https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct3D11.1/ScreenCapture/Program.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using Common.Utils;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Aurora.Settings.Layers.Ambilight;

public sealed class DesktopDuplicator : IDisposable
{
    public bool IsDisposed { get; private set; }
    
    private static readonly IDictionary<Adapter1, Device> Devices = new Dictionary<Adapter1, Device>();

    private readonly Device _device;
    private readonly Texture2D _desktopImageTexture;
    private readonly Func<OutputDuplication> _newDesktopDuplication;
    
    private OutputDuplication? _deskDupl;

    public DesktopDuplicator(Output5 output, Adapter1 adapter1)
    {
        WeakEventManager<Output5, EventArgs>.AddHandler(output, nameof(output.Disposing), (_, _) =>
        {
            Dispose();
        });
        WeakEventManager<Adapter1, EventArgs>.AddHandler(adapter1, nameof(adapter1.Disposing), (_, _) =>
        {
            Dispose();
        });
        if (!Devices.TryGetValue(adapter1, out _device))
        {
            _device = new Device(adapter1, DeviceCreationFlags.Debug);
            Devices.Add(adapter1, _device);
        }
        Global.logger.Information("Starting desktop duplicator");
        _device.ExceptionMode = 1;
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

        _newDesktopDuplication = () =>
        {
            try
            {
                return output.DuplicateOutput1(_device, 0, 1, new[] { Format.B8G8R8A8_UNorm });
            }
            catch
            {
                DesktopUtils.ResetDpiAwareness();
                throw;
            }
        };
        _deskDupl = _newDesktopDuplication.Invoke();
        _desktopImageTexture = new Texture2D(_device, textureDesc);
    }

    public Bitmap? Capture(Rectangle desktopRegion, int timeout)
    {
        if (IsDisposed || _deskDupl is { IsDisposed: true } || _device.IsDisposed) 
            return null;

        try
        {
            _deskDupl ??= _newDesktopDuplication.Invoke();
            var tryAcquireNextFrame =
                _deskDupl.TryAcquireNextFrame(timeout, out var frameInformation, out var desktopResource);
            _deskDupl.ReleaseFrame();
            if (tryAcquireNextFrame.Failure || frameInformation.LastPresentTime == 0)
            {
                return null;
            }

            tryAcquireNextFrame.CheckError();
            using var tempTexture = desktopResource.QueryInterface<Texture2D>();
            _device.ImmediateContext.CopyResource(tempTexture, _desktopImageTexture);
            desktopResource.Dispose();

            var mapSource = _device.ImmediateContext.MapSubresource(_desktopImageTexture, 0, MapMode.Read, MapFlags.None);
            return mapSource.IsEmpty ? null : ProcessFrame(mapSource, desktopRegion);
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.WaitTimeout)
        {
            Global.logger.Information("Timeout of {Timeout}ms exceeded while acquiring next frame", timeout);
            return null;
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.AccessLost)
        {
            // Can happen when going fullscreen / exiting fullscreen
            Global.logger.Information(e, "DesktopDuplicator access lost");
            return null;
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.AccessDenied)
        {
            // Happens when locking PC
            return null;
        }
        catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.InvalidCall)
        {
            Global.logger.Information(e, "DesktopDuplicator InvalidCall");
            // Already released?
            _deskDupl?.Dispose();
            _deskDupl = null;
            return null;
        }
        catch (SharpDXException e) when (e.ResultCode.Failure)
        {
            Global.logger.Warning(e, "SharpDX exception in DesktopDuplicator");
            return null;
        }
        finally
        {
            if (_device is { IsDisposed: false, ImmediateContext.IsDisposed: false } && !_desktopImageTexture.IsDisposed)
            {
                _device.ImmediateContext.UnmapSubresource(_desktopImageTexture, 0);
            }
            else
            {
                Global.logger.Information("Not unmapping desktop texture");
            }
        }
    }

    Bitmap ProcessFrame(DataBox mapSource, Rectangle rect)
    {
        IntPtr sourcePtr = mapSource.DataPointer;
        int sourceRowPitch = mapSource.RowPitch;

        var frame = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb);
        // Copy pixels from screen capture Texture to GDI bitmap
        var mapDest = frame.LockBits(rect with {X = 0, Y = 0}, ImageLockMode.WriteOnly, frame.PixelFormat);

        var destPtr = mapDest.Scan0;
        sourcePtr = IntPtr.Add(sourcePtr, rect.Y * sourceRowPitch);
        sourcePtr = IntPtr.Add(sourcePtr, rect.X * 4);
        for (var y = 0; y < rect.Height; y++)
        {
            // Copy a single line 
            Utilities.CopyMemory(destPtr, sourcePtr, mapDest.Stride);

            // Advance pointers
            sourcePtr = IntPtr.Add(sourcePtr, sourceRowPitch);
            destPtr = IntPtr.Add(destPtr, mapDest.Stride);
        }
        // Release source and dest locks
        frame.UnlockBits(mapDest);

        return frame;
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        IsDisposed = true;

        if (_deskDupl != null)
        {
            _deskDupl.ReleaseFrame();
            if (!_deskDupl.IsDisposed)
            {
                _deskDupl.Dispose();
            }
        }
        if (!_desktopImageTexture.IsDisposed)
        {
            _desktopImageTexture.Dispose();
        }

        if (!_desktopImageTexture.IsDisposed)
        {
            _desktopImageTexture.Dispose();
        }
    }
}