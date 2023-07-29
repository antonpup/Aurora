using System;
using System.Diagnostics;
using System.Windows.Automation;
using Microsoft.Collections.Extensions;

namespace Aurora.Modules.ProcessMonitor;

public class WindowProcess : IEquatable<WindowProcess>
{
    public int ProcessId { get; protected internal set; }
    public string ProcessName { get; protected internal set; } = string.Empty;
    public int WindowHandle { get; }

    public WindowProcess(int windowHandle)
    {
        WindowHandle = windowHandle;
    }

    public bool Equals(WindowProcess? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return WindowHandle == other.WindowHandle;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((WindowProcess)obj);
    }

    public override int GetHashCode()
    {
        return WindowHandle;
    }
}

public sealed class WindowListener : IDisposable
{
    public static WindowListener Instance { get; set; }

    // event that sends process id of new window
    public event EventHandler<int>? WindowCreated;
    public event EventHandler<int>? WindowDestroyed;

    public readonly MultiValueDictionary<string, WindowProcess> ProcessWindowsMap = new();

    public void StartListening()
    {
        Automation.AddAutomationEventHandler(WindowPattern.WindowOpenedEvent, AutomationElement.RootElement, TreeScope.Descendants, WindowDetected);
    }

    private void WindowDetected(object? sender, AutomationEventArgs e)
    {
        try
        {
            var element = (AutomationElement)sender;
            var process = Process.GetProcessById(element.Current.ProcessId);
            if (process.ProcessName.StartsWith("Aurora"))
            {
                return;
            }

            var name = process.ProcessName + ".exe";
            var windowHandle = element.Current.NativeWindowHandle;

            Automation.AddAutomationEventHandler(WindowPattern.WindowClosedEvent, element, TreeScope.Element, (_, _) =>
            {
                ProcessWindowsMap.Remove(name, new WindowProcess(windowHandle));
                WindowDestroyed?.Invoke(this, windowHandle);
            });

            //To make sure window close event can be fired, we fire open event after subscribing to close event
            ProcessWindowsMap.Add(name,
                new WindowProcess(windowHandle) { ProcessId = element.Current.ProcessId, ProcessName = name, });
            WindowCreated?.Invoke(this, windowHandle);
        }
        catch
        {
            //ignored
        }
    }

    public void Dispose()
    {
        Automation.RemoveAutomationEventHandler(WindowPattern.WindowOpenedEvent, AutomationElement.RootElement,
            WindowDetected);
    }
}