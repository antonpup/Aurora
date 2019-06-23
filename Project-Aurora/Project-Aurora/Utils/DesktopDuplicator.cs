// Based on https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct3D11.1/ScreenCapture/Program.cs

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Aurora
{
    public class DesktopDuplicator : IDisposable
    {
        #region Fields
        private readonly Device _device;
        private readonly OutputDuplication _deskDupl;

        private readonly Texture2D _desktopImageTexture;

        private Rectangle _rect;

        #endregion

        public DesktopDuplicator(Adapter1 adapter, Output1 output, Rectangle Rect)
        {
            Global.logger.Info("Starting desktop duplicator");
            _rect = Rect;
            _device = new Device(adapter);
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = _rect.Width,
                Height = _rect.Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };

             _deskDupl = output.DuplicateOutput(_device);
            _desktopImageTexture = new Texture2D(_device, textureDesc);
        }

        public Bitmap Capture(int timeout)
        {
            SharpDX.DXGI.Resource desktopResource;
            if (_deskDupl.IsDisposed || _device.IsDisposed) 
                return null;

            try {
                _deskDupl.AcquireNextFrame(timeout, out OutputDuplicateFrameInformation _frameInfo, out desktopResource);
            }
            catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.WaitTimeout)
            {
                Global.logger.Debug(String.Format("Timeout of {0}ms exceeded while acquiring next frame", timeout));
                return null;
            }
            catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.AccessLost)
            {
                // Can happen when going fullscreen / exiting fullscreen
                Global.logger.Warn(e.Message);
                throw e;
            }
            catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.AccessDenied)
            {
                // Happens when locking PC
                Global.logger.Debug(e.Message);
                throw e;
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

        Bitmap ProcessFrame(IntPtr SourcePtr, int SourceRowPitch)
        {
            var frame = new Bitmap(_rect.Width, _rect.Height, PixelFormat.Format32bppRgb);
            // Copy pixels from screen capture Texture to GDI bitmap
            var mapDest = frame.LockBits(new Rectangle(0, 0, _rect.Width, _rect.Height), ImageLockMode.WriteOnly, frame.PixelFormat);
            for (int y = 0, sizeInBytesToCopy = _rect.Width * 4; y < _rect.Height; y++)
            {
                Utilities.CopyMemory(mapDest.Scan0 + y * mapDest.Stride, SourcePtr + y * SourceRowPitch, sizeInBytesToCopy);
            }
            // Release source and dest locks
            frame.UnlockBits(mapDest);

            return frame;
        }

        bool ReleaseFrame()
        {
            try
            {
                _deskDupl?.ReleaseFrame();
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
            try
            {
                _deskDupl?.Dispose();
                _desktopImageTexture?.Dispose();
                _device?.Dispose();
            }
            catch { }
        }
    }
}
