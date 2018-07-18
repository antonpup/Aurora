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
        readonly Device _device;
        readonly OutputDuplication _deskDupl;

        readonly Texture2D _desktopImageTexture;
        OutputDuplicateFrameInformation _frameInfo;

        Rectangle _rect;

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

            try
            {
                _deskDupl = output.DuplicateOutput(_device);
            }
            catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable)
            {
                throw new Exception("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.", e);
            }
            catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.Unsupported)
            {
                throw new NotSupportedException("Desktop Duplication is not supported on this system.\nIf you have multiple graphic cards, try running on integrated graphics.", e);
            }

            _desktopImageTexture = new Texture2D(_device, textureDesc);
        }

        public Bitmap Capture(int timeout)
        {
            SharpDX.DXGI.Resource desktopResource;
            if (_deskDupl.IsDisposed || _device.IsDisposed) {
                return null;
            }
            try
            {
                _deskDupl.AcquireNextFrame(timeout, out _frameInfo, out desktopResource);
            }
            catch (SharpDXException e) when (e.Descriptor == SharpDX.DXGI.ResultCode.WaitTimeout)
            {
                Global.logger.Debug(String.Format("Timeout of {0}ms exceeded while acquiring next frame", timeout));
                return null;
            }
            catch (SharpDXException e) when (e.ResultCode.Failure)
            {
                // Can happen when going fullscreen / exiting fullscreen
                Global.logger.Warn(e.Message);
                throw e;
            }

            using (desktopResource)
            {
                using (var tempTexture = desktopResource.QueryInterface<Texture2D>())
                {
                    _device.ImmediateContext.CopyResource(tempTexture, _desktopImageTexture);
                }
            }

            ReleaseFrame();

            var mapSource = _device.ImmediateContext.MapSubresource(_desktopImageTexture, 0, MapMode.Read, MapFlags.None);

            try
            {
                return ProcessFrame(mapSource.DataPointer, mapSource.RowPitch);
            }
            finally
            {
                _device.ImmediateContext.UnmapSubresource(_desktopImageTexture, 0);
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

        void ReleaseFrame()
        {
            try
            {
                _deskDupl.ReleaseFrame();
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Failure)
                {
                    Global.logger.Warn(e.Message);
                }
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
