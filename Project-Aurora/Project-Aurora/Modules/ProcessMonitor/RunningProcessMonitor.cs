using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using Lombok.NET;

namespace Aurora.Modules.ProcessMonitor;

public class RunningProcessChanged: EventArgs
{
    public string ProcessName { get; }

    public RunningProcessChanged(string processName)
    {
        ProcessName = processName;
    }
}

/// <summary>
/// Class that monitors running processes using the <see cref="ManagementEventWatcher"/> to update when new processes are started
/// or existing ones are terminated. This means that it is not relying on <see cref="Process.GetProcesses()"/> which is a much
/// more intensive task and causes lag when run often. The only minor downside is that this will not instantly detected when a
/// process closes and can delay by about 2 seconds, though this really shouldn't be an issue since this isn't required for the
/// profile switching - only the overlay toggling.
/// </summary>
[Singleton]
public sealed partial class RunningProcessMonitor : IDisposable {
    public event EventHandler<RunningProcessChanged>? RunningProcessesChanged;

    /// <summary>A list of all currently running processes (and how many instances are running).</summary>
    /// <remarks>The reason for the count is so that if two processes of the same file are running and one is closed, we can know
    /// that the other is still running.</remarks>
    private readonly Dictionary<string, int> _runningProcesses;

    private readonly ManagementEventWatcher _startWatcher;
    private readonly ManagementEventWatcher _stopWatcher;

    /// <summary>
    /// Creates a new instance of the <see cref="RunningProcessMonitor"/>, which performs an initial scan of running
    /// processes and then sets up the watchers with their relevant commands.
    /// </summary>
    private RunningProcessMonitor() {
        // Fetch all processes running now
        _runningProcesses = Process.GetProcesses()
            .Select(p => {
                try { return System.IO.Path.GetFileName(p.MainModule.FileName).ToLower(); }
                catch { return p.ProcessName.ToLower(); }
            })
            .GroupBy(name => name)
            .ToDictionary(g => g.First(), g => g.Count());

        // Listen for new processes
        _startWatcher = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
        _startWatcher.EventArrived += ProcessStarted;
        _startWatcher.Start();

        // Listen for closed processes
        _stopWatcher = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStopTrace");
        _stopWatcher.EventArrived += ProcessStopped;
        _stopWatcher.Start();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void ProcessStarted(object sender, EventArrivedEventArgs e)
    {
        // Get the name of the started process
        var name = e.NewEvent.Properties["ProcessName"].Value.ToString().ToLower();
        // Set the dictionary to be the existing value + 1 or simply 1 if it doesn't exist already.
        _runningProcesses[name] = _runningProcesses.TryGetValue(name, out int i) ? i + 1 : 1;
        RunningProcessesChanged?.Invoke(sender, new RunningProcessChanged(name));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void ProcessStopped(object sender, EventArrivedEventArgs e)
    {
        // Get the name of the terminated process
        var name = e.NewEvent.Properties["ProcessName"].Value.ToString().ToLower();
        // Ensure the process exists in our dictionary
        if (_runningProcesses.TryGetValue(name, out int count))
        {
            if (count == 1) // If there is only 1 process currently running, remove it (since that must've been the one that terminated)
                _runningProcesses.Remove(name);
            else // Else, simply decrement the process count number
                _runningProcesses[name]--;
        }

        RunningProcessesChanged?.Invoke(sender, new RunningProcessChanged(name));
    }

    /// <summary>
    /// Returns whether the given process name is detected as running or not.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool IsProcessRunning(string name) => _runningProcesses.ContainsKey(name.ToLower());

    public void Dispose()
    {
        _startWatcher.EventArrived -= ProcessStarted;
        _startWatcher.Stop();
        _startWatcher.Dispose();
        _stopWatcher.EventArrived -= ProcessStopped;
        _stopWatcher.Stop();
        _stopWatcher.Dispose();
    }
}