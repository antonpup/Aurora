using System;
using System.Diagnostics;
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
	public string ProcessTitle { get; private set; } = string.Empty;

	public int ProcessId { get; private set; }
	
	public event EventHandler? ActiveProcessChanged;

	private readonly User32.WinEventDelegate _dele;

	private ActiveProcessMonitor()
	{
		_dele = WinEventProc;
		if (Global.Configuration.DetectionMode != ApplicationDetectionMode.WindowsEvents)
		{
			return;
		}
		User32.SetWinEventHook(EventSystemForeground, EventSystemForeground, IntPtr.Zero, _dele, 0, 0, WinEventOutOfContext);
		User32.SetWinEventHook(EventSystemMinimizeEnd, EventSystemMinimizeEnd, IntPtr.Zero, _dele, 0, 0, WinEventOutOfContext);
	}

	private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr windowHandle, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
	{
		SetWindowCurrent(windowHandle);
	}

	public void UpdateActiveProcessPolling()
	{
		var windowHandle = User32.GetForegroundWindow();
		SetWindowCurrent(windowHandle);
	}

	private void SetWindowCurrent(IntPtr windowHandle)
	{
		try
		{
			if (User32.GetWindowThreadProcessId(windowHandle, out var pid) > 0)
			{
				using var process = Process.GetProcessById((int)pid);
				ProcessTitle = process.MainWindowTitle;
				ProcessName = process.ProcessName + ".exe";
				ProcessId = (int)pid;
				return;
			}
		}
		catch (Exception exc)
		{
			Global.logger.Error(exc, "Exception in GetActiveWindowsProcessName");
		}

		ProcessTitle = string.Empty;
		ProcessName = string.Empty;
		ProcessId = 0;
	}
}