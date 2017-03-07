using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MirrSharp.Driver
{
	public class DesktopMirror : IDisposable
	{
		#region External Constants

		private const int Map = 1030;
		private const int UnMap = 1031;
		private const int TestMapped = 1051;

		private const int IGNORE = 0;
		private const int BLIT = 12;
		private const int TEXTOUT = 18;
		private const int MOUSEPTR = 48;

		private const int CDS_UPDATEREGISTRY = 0x00000001;
		private const int CDS_TEST = 0x00000002;
		private const int CDS_FULLSCREEN = 0x00000004;
		private const int CDS_GLOBAL = 0x00000008;
		private const int CDS_SET_PRIMARY = 0x00000010;
		private const int CDS_RESET = 0x40000000;
		private const int CDS_SETRECT = 0x20000000;
		private const int CDS_NORESET = 0x10000000;
		private const int MAXIMUM_ALLOWED = 0x02000000;
		private const int DM_BITSPERPEL = 0x40000;
		private const int DM_PELSWIDTH = 0x80000;
		private const int DM_PELSHEIGHT = 0x100000;
		private const int DM_POSITION = 0x00000020;
		#endregion

		#region External Methods

		[DllImport("user32.dll")]
		private static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DeviceMode mode, IntPtr hwnd, uint dwflags, IntPtr lParam);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr pointer);

		[DllImport("user32.dll")]
		private static extern bool EnumDisplayDevices(string lpDevice, uint ideviceIndex, ref DisplayDevice lpdevice, uint dwFlags);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern int ExtEscape(IntPtr hdc, int nEscape, int cbInput, IntPtr lpszInData, int cbOutput, IntPtr lpszOutData);

		[DllImport("user32.dll", EntryPoint = "GetDC")]
		private static extern IntPtr GetDC(IntPtr ptr);

		[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
		private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

		#endregion

		public event EventHandler<DesktopChangeEventArgs> DesktopChange;
		public class DesktopChangeEventArgs : EventArgs
		{
			public int x1;
			public int y1;
			public int x2;
			public int y2;
			public OperationType type;

			public DesktopChangeEventArgs(int x1, int y1, int x2, int y2, OperationType type)
			{
				this.x1 = x1;
				this.y1 = y1;
				this.x2 = x2;
				this.y2 = y2;
				this.type = type;
			}
		}

		private string driverInstanceName = "";
		private IntPtr _getChangesBuffer = IntPtr.Zero;
		private Thread _pollingThread = null;

		private static void SafeChangeDisplaySettingsEx(string lpszDeviceName, ref DeviceMode mode, IntPtr hwnd, uint dwflags, IntPtr lParam)
		{
			int result = ChangeDisplaySettingsEx(lpszDeviceName, ref mode, hwnd, dwflags, lParam);
			switch (result)
			{
				case 0: return; //DISP_CHANGE_SUCCESSFUL
				case 1: throw new Exception("The computer must be restarted for the graphics mode to work."); //DISP_CHANGE_RESTART
				case -1: throw new Exception("The display driver failed the specified graphics mode."); // DISP_CHANGE_FAILED
				case -2: throw new Exception("The graphics mode is not supported."); // DISP_CHANGE_BADMODE
				case -3: throw new Exception("Unable to write settings to the registry."); // DISP_CHANGE_NOTUPDATED
				case -4: throw new Exception("An invalid set of flags was passed in."); // DISP_CHANGE_BADFLAGS
				case -5: throw new Exception("An invalid parameter was passed in. This can include an invalid flag or combination of flags."); // DISP_CHANGE_BADPARAM
				case -6: throw new Exception("The settings change was unsuccessful because the system is DualView capable."); // DISP_CHANGE_BADDUALVIEW
			}
		}

		public enum MirrorState
		{
			Idle,
			Loaded,
			Connected,
			Running
		}

		public MirrorState State { get; private set; }

		private const string driverDeviceNumber = "DEVICE0";
		private const string driverMiniportName = "dfmirage";
		private const string driverName = "Mirage Driver";
		private const string driverRegistryPath = "SYSTEM\\CurrentControlSet\\Hardware Profiles\\Current\\System\\CurrentControlSet\\Services";
		private RegistryKey _registryKey;



		public bool Load()
		{
			if (State != MirrorState.Idle)
				throw new InvalidOperationException("You may call Load only if the state is Idle");

			var device = new DisplayDevice();
			var deviceMode = new DeviceMode { dmDriverExtra = 0 };

			device.CallBack = Marshal.SizeOf(device);
			deviceMode.dmSize = (short)Marshal.SizeOf(deviceMode);
			deviceMode.dmBitsPerPel = Screen.PrimaryScreen.BitsPerPixel;

			if (deviceMode.dmBitsPerPel == 24)
				deviceMode.dmBitsPerPel = 32;

			_bitmapBpp = deviceMode.dmBitsPerPel;

			deviceMode.dmDeviceName = string.Empty;
			deviceMode.dmFields = (DM_BITSPERPEL | DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION);
			_bitmapHeight = deviceMode.dmPelsHeight = Screen.PrimaryScreen.Bounds.Height;
			_bitmapWidth = deviceMode.dmPelsWidth = Screen.PrimaryScreen.Bounds.Width;

			bool deviceFound;
			uint deviceIndex = 0;

			while (deviceFound = EnumDisplayDevices(null, deviceIndex, ref device, 0))
			{
				if (device.DeviceString == driverName)
					break;
				deviceIndex++;
			}

			if (!deviceFound) return false;

			driverInstanceName = device.DeviceName;

			_registryKey = Registry.LocalMachine.OpenSubKey(driverRegistryPath, true);
			if (_registryKey != null) 
				_registryKey = _registryKey.CreateSubKey(driverMiniportName);
			else 
				throw new Exception("Couldn't open registry key");

			if (_registryKey != null) 
				_registryKey = _registryKey.CreateSubKey(driverDeviceNumber);
			else
				throw new Exception("Couldn't open registry key");

//			_registryKey.SetValue("Cap.DfbBackingMode", 0);
//			_registryKey.SetValue("Order.BltCopyBits.Enabled", 1);
			_registryKey.SetValue("Attach.ToDesktop", 1);

			#region This was CommitDisplayChanges

			SafeChangeDisplaySettingsEx(device.DeviceName, ref deviceMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
			SafeChangeDisplaySettingsEx(device.DeviceName, ref deviceMode, IntPtr.Zero, 0, IntPtr.Zero);

			#endregion

			State = MirrorState.Loaded;

			return true;
		}

		public bool Connect()
		{
			if (State != MirrorState.Loaded)
				throw new InvalidOperationException("You may call Connect only if the state is Loaded");

			bool result = mapSharedBuffers(); // Adjusts _running
			if (result)
			{
				State = MirrorState.Connected;
			}

			return result;
		}

		public void Start()
		{
			if (State != MirrorState.Connected)
				throw new InvalidOperationException("You may call Start only if the state is Connected");

			if (_terminatePollingThread == null)
				_terminatePollingThread = new ManualResetEvent(false);
			else
				_terminatePollingThread.Reset();

			_pollingThread = new Thread(pollingThreadProc) {IsBackground = true};
			_pollingThread.Start();

			State = MirrorState.Running;
		}

		/// <summary>
		/// Driver buffer polling interval, in msec.
		/// </summary>
		private const int PollInterval = 500;

		private void pollingThreadProc()
		{
			long oldCounter = long.MaxValue;
			while (true)
			{
				var getChangesBuffer = (GetChangesBuffer) Marshal.PtrToStructure(_getChangesBuffer, typeof (GetChangesBuffer));
				var buffer = (ChangesBuffer)Marshal.PtrToStructure(getChangesBuffer.Buffer, typeof(ChangesBuffer));

				// Initialize oldCounter
				if (oldCounter == long.MaxValue)
					oldCounter = buffer.counter;

				if (oldCounter != buffer.counter)
				{
					Trace.WriteLine(string.Format("Counter changed. Old is {0} new is {1}", oldCounter, buffer.counter));
					for (long currentChange = oldCounter; currentChange != buffer.counter; currentChange++ )
					{
						if (currentChange >= ChangesBuffer.MAXCHANGES_BUF) 
							currentChange = 0;

						if (DesktopChange != null)
							DesktopChange(this,
							              new DesktopChangeEventArgs(buffer.pointrect[currentChange].rect.x1,
							                                         buffer.pointrect[currentChange].rect.y1,
							                                         buffer.pointrect[currentChange].rect.x2,
							                                         buffer.pointrect[currentChange].rect.y2,
																	 (OperationType) buffer.pointrect[currentChange].type));
					}

					oldCounter = buffer.counter;
				}

				// Just to prevent 100-percent CPU load and to provide thread-safety use manual reset event instead of simple in-memory flag.
				if (_terminatePollingThread.WaitOne(PollInterval, false))
				{
					Trace.WriteLine("The thread now exits");
					break;
				}
			}

			// We can be sure that _pollingThreadTerminated exists
			_pollingThreadTerminated.Set();
		}

		private ManualResetEvent _terminatePollingThread;
		private ManualResetEvent _pollingThreadTerminated;

		private const int PollingThreadTerminationTimeout = 10000;

		public void Stop()
		{
			if (State != MirrorState.Running) return;

			if (_pollingThreadTerminated == null)
				_pollingThreadTerminated = new ManualResetEvent(false);
			else
				_pollingThreadTerminated.Reset();

			// Terminate polling thread
			_terminatePollingThread.Set();

			// Wait for it...
			if (!_pollingThreadTerminated.WaitOne(PollingThreadTerminationTimeout, false))
				_pollingThread.Abort();

			State = MirrorState.Connected;
		}

		public void Disconnect()
		{
			if (State == MirrorState.Running)
				Stop();

			if (State != MirrorState.Connected)
				return;

			unmapSharedBuffers();
			State = MirrorState.Loaded;
		}

		public void Unload()
		{
			if (State == MirrorState.Running)
				Stop();
			if (State == MirrorState.Connected)
				Disconnect();

			if (State != MirrorState.Loaded)
				return;

			var deviceMode = new DeviceMode();
			deviceMode.dmSize = (short)Marshal.SizeOf(typeof(DeviceMode));
			deviceMode.dmDriverExtra = 0;
			deviceMode.dmFields = (DM_BITSPERPEL | DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION);

			var device = new DisplayDevice();
			device.CallBack = Marshal.SizeOf(device);
			deviceMode.dmDeviceName = string.Empty;
			uint deviceIndex = 0;
			while (EnumDisplayDevices(null, deviceIndex, ref device, 0))
			{
				if (device.DeviceString.Equals(driverName))
					break;

				deviceIndex++;
			}

			Debug.Assert(_registryKey != null);

			_registryKey.SetValue("Attach.ToDesktop", 0);
			_registryKey.Close();

			deviceMode.dmDeviceName = driverMiniportName;

			if (deviceMode.dmBitsPerPel == 24) deviceMode.dmBitsPerPel = 32;

			#region This was CommitDisplayChanges

			SafeChangeDisplaySettingsEx(device.DeviceName, ref deviceMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
			SafeChangeDisplaySettingsEx(device.DeviceName, ref deviceMode, IntPtr.Zero, 0, IntPtr.Zero);

			#endregion

			State = MirrorState.Idle;
		}

		private IntPtr _globalDC;

		private bool mapSharedBuffers()
		{
			_globalDC = CreateDC(driverInstanceName, null, null, IntPtr.Zero);
			if (_globalDC == IntPtr.Zero)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}

			if (_getChangesBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(_getChangesBuffer);
			}

			_getChangesBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (GetChangesBuffer)));

			int res = ExtEscape(_globalDC, Map, 0, IntPtr.Zero, Marshal.SizeOf(typeof(GetChangesBuffer)), _getChangesBuffer);
			if (res > 0)
				return true;

			return false;
		}

		private void unmapSharedBuffers()
		{
			int res = ExtEscape(_globalDC, UnMap, Marshal.SizeOf(typeof(GetChangesBuffer)), _getChangesBuffer, 0, IntPtr.Zero);
			if (res < 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			Marshal.FreeHGlobal(_getChangesBuffer);
			_getChangesBuffer = IntPtr.Zero;

			ReleaseDC(IntPtr.Zero, _globalDC);
		}

		private int _bitmapWidth, _bitmapHeight, _bitmapBpp;

		public Bitmap GetScreen()
		{
			if (State != MirrorState.Connected && State != MirrorState.Running)
				throw new InvalidOperationException("In order to get current screen you must at least be connected to the driver");

			PixelFormat format;
			if (_bitmapBpp == 16)
				format = PixelFormat.Format16bppRgb565;
			else if (_bitmapBpp == 24)
				format = PixelFormat.Format24bppRgb;
			else if (_bitmapBpp == 32)
				format = PixelFormat.Format32bppArgb;
			else
			{
				Debug.Fail("Unknown pixel format");
				throw new Exception("Unknown pixel format");
			}

			var result = new Bitmap(_bitmapWidth, _bitmapHeight, format);

			var rect = new System.Drawing.Rectangle(0, 0, _bitmapWidth, _bitmapHeight);
			BitmapData bmpData = result.LockBits(rect, ImageLockMode.WriteOnly, format);

			// Get the address of the first line.
			IntPtr ptr = bmpData.Scan0;
	        // Declare an array to hold the bytes of the bitmap.
			int bytes = bmpData.Stride * _bitmapHeight;

			var getChangesBuffer = (GetChangesBuffer) Marshal.PtrToStructure(_getChangesBuffer, typeof (GetChangesBuffer));
			var data = new byte[bytes];
			Marshal.Copy(getChangesBuffer.UserBuffer, data, 0, bytes);
	        // Copy the RGB values into the bitmap.
			Marshal.Copy(data, 0, ptr, bytes);

			result.UnlockBits(bmpData);

			return result;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			if (State != MirrorState.Idle)
				Unload();
		}
	}
}
