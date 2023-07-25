using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Aurora.Settings;
using Aurora.Utils;
using Lombok.NET;

namespace Aurora.Modules.ProcessMonitor;

[Singleton]
public sealed partial class ActiveProcessMonitor
{
	private const uint WinEventOutOfContext = 0;
	private const uint EventSystemForeground = 3;
	private const uint EventSystemMinimizeEnd = 0x0017;

	private string _processPath = string.Empty;
	public string ProcessName {
		get => _processPath;
		private set
		{
			_processPath = value;
			ActiveProcessChanged?.Invoke(null, EventArgs.Empty);
		}
	}
	public string ProcessTitle { get; private set; }
	public event EventHandler? ActiveProcessChanged;

	public string ActiveProcessName { get; private set; } = "";

	private readonly User32.WinEventDelegate _dele;

	private ActiveProcessMonitor()
	{
		if (Global.Configuration.DetectionMode != ApplicationDetectionMode.WindowsEvents)
		{
			return;
		}
		_dele = WinEventProc;
		User32.SetWinEventHook(EventSystemForeground, EventSystemForeground, IntPtr.Zero, _dele, 0, 0, WinEventOutOfContext);
		User32.SetWinEventHook(EventSystemMinimizeEnd, EventSystemMinimizeEnd, IntPtr.Zero, _dele, 0, 0, WinEventOutOfContext);
	}

	private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
	{
		UpdateProcessTitle(hwnd);
		ProcessName = GetWindowProcessName(hwnd);
		
	}

	public void UpdateActiveProcessPolling()
	{
		var windowHandle = User32.GetForegroundWindow();
		UpdateProcessTitle(windowHandle);
		ProcessName = GetActiveWindowProcessName(windowHandle);
	}

	public static TimeSpan GetTimeSinceLastInput() {
		var inf = new User32.tagLASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<User32.tagLASTINPUTINFO>() };
		return !User32.GetLastInputInfo(ref inf) ?
			new TimeSpan(0) :
			new TimeSpan(0, 0, 0, 0, Environment.TickCount - inf.dwTime);
	}

	private string GetActiveWindowProcessName(IntPtr windowHandle)
	{
		try
		{
			return GetWindowProcessName(windowHandle);
		}
		catch (Exception exc)
		{
			Global.logger.Error("Exception in GetActiveWindowsProcessname" + exc);
		}

		return string.Empty;
	}

	private void UpdateProcessTitle(IntPtr windowHandle)
	{
		var text = new StringBuilder(256);
		if (User32.GetWindowText(windowHandle, text, text.Capacity) > 0)
			ProcessTitle = text.ToString();
	}

	private string GetWindowProcessName(IntPtr windowHandle)
	{
		if (User32.GetWindowThreadProcessId(windowHandle, out var pid) <= 0) return "";
		var proc = Process.GetProcessById((int)pid);
		return proc.ProcessName + ".exe";
	}

	private string? GetActiveWindowsProcessName() {
		try {
			var windowHandle = User32.GetForegroundWindow();
			if (windowHandle == IntPtr.Zero)
			{
				return null;
			}

			if (User32.GetWindowThreadProcessId(windowHandle, out var processId) > 0)
			{
				var process = Process.GetProcessById((int)processId);
				return process.ProcessName + ".exe";
			}
		} catch (Exception exc) {
			Global.logger.Error("Exception in GetActiveWindowsProcessTitle", exc);
		}
		return null;
	}

}