using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Lombok.NET;

namespace Aurora.Utils
{
	public struct tagLASTINPUTINFO
	{
		public uint cbSize;
		public Int32 dwTime;
	}

	[Singleton]
	public sealed partial class ActiveProcessMonitor
	{
		private const uint WinEventOutOfContext = 0;
		private const uint EventSystemForeground = 3;
		private const uint EventSystemMinimizeStart = 0x0016;
		private const uint EventSystemMinimizeEnd = 0x0017;

		private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
		private string _processPath = string.Empty;
		public string ProcessPath {
			get => _processPath;
			private set
			{
				_processPath = value; ActiveProcessChanged?.Invoke(this, EventArgs.Empty);
			}
		}
		public static event EventHandler ActiveProcessChanged;

		private WinEventDelegate dele;

		private ActiveProcessMonitor()
		{
			dele = WinEventProc;
			SetWinEventHook(EventSystemForeground, EventSystemForeground, IntPtr.Zero, dele, 0, 0, WinEventOutOfContext);
			SetWinEventHook(EventSystemMinimizeStart, EventSystemMinimizeEnd, IntPtr.Zero, dele, 0, 0, WinEventOutOfContext);
		}

		private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (Global.Configuration.DetectionMode == Settings.ApplicationDetectionMode.WindowsEvents)
			{
				GetActiveWindowsProcessName();
			}
		}

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

		// TODO: Move this to own util
		[DllImport("user32.dll")]
		public static extern Boolean GetLastInputInfo(ref tagLASTINPUTINFO plii);

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public void GetActiveWindowsProcessName()
		{
			string activeProcess = getActiveWindowsProcessname();

			if (!String.IsNullOrWhiteSpace(activeProcess))
				ProcessPath = activeProcess;
		}

		[Flags]
		private enum ProcessAccessFlags : uint
		{
			All = 0x001F0FFF,
			Terminate = 0x00000001,
			CreateThread = 0x00000002,
			VirtualMemoryOperation = 0x00000008,
			VirtualMemoryRead = 0x00000010,
			DuplicateHandle = 0x00000040,
			CreateProcess = 0x000000080,
			SetQuota = 0x00000100,
			SetInformation = 0x00000200,
			QueryInformation = 0x00000400,
			QueryLimitedInformation = 0x00001000,
			Synchronize = 0x00100000
		}

		[DllImport("kernel32.dll")]
		private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags,
			StringBuilder lpExeName, out int size);
		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
			bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr hHandle);

        public static TimeSpan GetTimeSinceLastInput() {
            var inf = new tagLASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<tagLASTINPUTINFO>() };
            return !GetLastInputInfo(ref inf) ?
	            new TimeSpan(0) :
	            new TimeSpan(0, 0, 0, 0, Environment.TickCount - inf.dwTime);
        }

        private static string GetExecutablePath(Process Process)
		{
			return GetExecutablePathAboveVista(Process.Id);
		}

		private static string GetExecutablePathAboveVista(int processId)
		{
			var buffer = new StringBuilder(1024);
			var hprocess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processId);
			if (hprocess == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
			try
			{
				var size = buffer.Capacity;
				if (QueryFullProcessImageName(hprocess, 0, buffer, out size))
				{
					return buffer.ToString();
				}
			}
			finally
			{
				CloseHandle(hprocess);
			}
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}


		private string getActiveWindowsProcessname()
		{
			IntPtr windowHandle = IntPtr.Zero;

			try
			{
				if (windowHandle.Equals(IntPtr.Zero))
					windowHandle = GetForegroundWindow();
				uint pid;
				if (GetWindowThreadProcessId(windowHandle, out pid) > 0)
				{
					Process proc = Process.GetProcessById((int)pid);
					string path = GetExecutablePath(proc);
					if (!System.IO.File.Exists(path))
						throw new Exception($"Found file path does not exist! '{path}'");
					return path;
				}
			}
			catch (Exception exc)
			{
				Global.logger.Error("Exception in GetActiveWindowsProcessname" + exc);
			}

			return "";
		}

        public string GetActiveWindowsProcessTitle() {
            try {
                // Based on https://stackoverflow.com/a/115905
                IntPtr windowHandle = GetForegroundWindow();
                StringBuilder text = new StringBuilder(256);
                if (GetWindowText(windowHandle, text, text.Capacity) > 0)
                    return text.ToString();
            } catch (Exception exc) {
                Global.logger.Error("Exception in GetActiveWindowsProcessTitle" + exc);
            }
            return "";
        }

    }
}
