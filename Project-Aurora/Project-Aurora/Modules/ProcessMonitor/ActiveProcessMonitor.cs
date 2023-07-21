using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Lombok.NET;

namespace Aurora.Modules.ProcessMonitor;

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
	private const uint EventSystemMinimizeEnd = 0x0017;

	private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
	private string _processPath = string.Empty;
	public string ProcessName {
		get => _processPath;
		private set
		{
			_processPath = value;
			ActiveProcessChanged?.Invoke(null, EventArgs.Empty);
			var processName = GetActiveWindowsProcessName();
			if (processName != null)
			{
				ActiveProcessName = processName;
			}
		}
	}
	public string ProcessTitle { get; private set; }
	public event EventHandler? ActiveProcessChanged;

	public string ActiveProcessName { get; private set; } = "";

	private readonly WinEventDelegate _dele;

	private ActiveProcessMonitor()
	{
		_dele = WinEventProc;
		SetWinEventHook(EventSystemForeground, EventSystemForeground, IntPtr.Zero, _dele, 0, 0, WinEventOutOfContext);
		SetWinEventHook(EventSystemMinimizeEnd, EventSystemMinimizeEnd, IntPtr.Zero, _dele, 0, 0, WinEventOutOfContext);
	}

	private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
	{
		if (Global.Configuration.DetectionMode != Settings.ApplicationDetectionMode.WindowsEvents) return; //TODO unhook instead
		ProcessName = GetWindowProcessName(hwnd);
		
		var text = new StringBuilder(256);
		if (GetWindowText(hwnd, text, text.Capacity) > 0)
			ProcessTitle = text.ToString();
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

	public void UpdateActiveProcessPolling()
	{
		ProcessName = GetActiveWindowProcessName();
	}

	public static TimeSpan GetTimeSinceLastInput() {
		var inf = new tagLASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<tagLASTINPUTINFO>() };
		return !GetLastInputInfo(ref inf) ?
			new TimeSpan(0) :
			new TimeSpan(0, 0, 0, 0, Environment.TickCount - inf.dwTime);
	}

	private string GetActiveWindowProcessName()
	{
		var windowHandle = GetForegroundWindow();
		
		var text = new StringBuilder(256);
		if (GetWindowText(windowHandle, text, text.Capacity) > 0)
			ProcessTitle = text.ToString();

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

	private string GetWindowProcessName(IntPtr windowHandle)
	{
		if (GetWindowThreadProcessId(windowHandle, out var pid) <= 0) return "";
		var proc = Process.GetProcessById((int)pid);
		return proc.ProcessName + ".exe";
	}

	private string? GetActiveWindowsProcessName() {
		try {
			var windowHandle = GetForegroundWindow();
			if (windowHandle == IntPtr.Zero)
			{
				return null;
			}

			if (GetWindowThreadProcessId(windowHandle, out var processId) > 0)
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