using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
	public struct tagLASTINPUTINFO
	{
		public uint cbSize;
		public Int32 dwTime;
	}

	public sealed class ActiveProcessMonitor
	{
		private const uint WINEVENT_OUTOFCONTEXT = 0;
		private const uint EVENT_SYSTEM_FOREGROUND = 3;
		private const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
		private const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;
		delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
		private string processPath = string.Empty;
		public string ProcessPath { get { return processPath; } private set { processPath = value; ActiveProcessChanged?.Invoke(this, null); } }
		public event EventHandler ActiveProcessChanged;

		static WinEventDelegate dele;

		public ActiveProcessMonitor()
		{
			try
			{
				dele = new WinEventDelegate(WinEventProc);
				SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
				SetWinEventHook(EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZEEND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
			}
			catch (Exception exc)
			{

			}
		}

		public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (Global.Configuration.detection_mode == Settings.ApplicationDetectionMode.WindowsEvents)
			{
				GetActiveWindowsProcessname();
			}
		}

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

		// TODO: Move this to own util
		[DllImport("user32.dll")]
		public static extern Boolean GetLastInputInfo(ref tagLASTINPUTINFO plii);

		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		[DllImport("Oleacc.dll")]
		static extern IntPtr GetProcessHandleFromHwnd(IntPtr whandle);

		[DllImport("psapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		static extern uint GetModuleFileNameExW(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

		public void GetActiveWindowsProcessname()
		{
			string active_process = getActiveWindowsProcessname();

			if (!String.IsNullOrWhiteSpace(active_process))
				ProcessPath = active_process;
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

		private static string GetExecutablePath(Process Process)
		{
			//If running on Vista or later use the new function
			if (Environment.OSVersion.Version.Major >= 6)
			{
				return GetExecutablePathAboveVista(Process.Id);
			}

			return Process.MainModule.FileName;
		}

		private static string GetExecutablePathAboveVista(int ProcessId)
		{
			var buffer = new StringBuilder(1024);
			IntPtr hprocess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation,
				false, ProcessId);
			if (hprocess != IntPtr.Zero)
			{
				try
				{
					int size = buffer.Capacity;
					if (QueryFullProcessImageName(hprocess, 0, buffer, out size))
					{
						return buffer.ToString();
					}
				}
				finally
				{
					CloseHandle(hprocess);
				}
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

			/*try
            {
                IntPtr processhandle = IntPtr.Zero;
                IntPtr zeroHandle = IntPtr.Zero;
                if (windowHandle.Equals(IntPtr.Zero))
                    windowHandle = GetForegroundWindow();
                processhandle = GetProcessHandleFromHwnd(windowHandle);
                
                StringBuilder sb = new StringBuilder(4048);
                uint error = GetModuleFileNameExW(processhandle, zeroHandle, sb, 4048);
                string path = sb.ToString();
                System.IO.Path.GetFileName(path);
                if (!System.IO.File.Exists(path))
                    throw new Exception($"Found file path does not exist! '{path}'");


                return path;
            }
            catch (ArgumentException aex)
            {
                Global.logger.LogLine("Argument Exception: " + aex, Logging_Level.Error);
                //if (Global.isDebug)
                    //throw aex;
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception in GetActiveWindowsProcessname" + exc, Logging_Level.Error);
                //if (Global.isDebug)
                    //throw exc;
            }*/

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
