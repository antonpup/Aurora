using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Aurora.Utils {

    /// <summary>
    /// Class that monitors running processes using the <see cref="ManagementEventWatcher"/> to update when new processes are started
    /// or existing ones are terminated. This means that it is not relying on <see cref="Process.GetProcesses()"/> which is a much
    /// more intensive task and causes lag when run often. The only minor downside is that this will not instantly detected when a
    /// process closes and can delay by about 2 seconds, though this really shouldn't be an issue since this isn't required for the
    /// profile switching - only the overlay toggling.
    /// </summary>
    public class RunningProcessMonitor {

        /// <summary>A list of all currently running processes (and how many instances are running).</summary>
        /// <remarks>The reason for the count is so that if two processes of the same file are running and one is closed, we can know
        /// that the other is still running.</remarks>
        private readonly Dictionary<string, int> runningProcesses;

        /// <summary>
        /// Creates a new instance of the <see cref="RunningProcessMonitor"/>, which performs an initial scan of running
        /// processes and then sets up the watchers with their relevant commands.
        /// </summary>
        public RunningProcessMonitor() {
            // Fetch all processes running now
            runningProcesses = Process.GetProcesses()
                .Select(p => {
                    try { return System.IO.Path.GetFileName(p.MainModule.FileName).ToLower(); }
                    catch { return p.ProcessName.ToLower(); }
                })
                .GroupBy(name => name)
                .ToDictionary(g => g.First(), g => g.Count());

            // Listen for new processes
            ManagementEventWatcher startWatcher = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
            startWatcher.EventArrived += (sender, e) => {
                // Get the name of the started process
                var name = e.NewEvent.Properties["ProcessName"].Value.ToString().ToLower();
                // Set the dictionary to be the existing value + 1 or simply 1 if it doesn't exist already.
                runningProcesses[name] = runningProcesses.TryGetValue(name, out int i) ? i + 1 : 1;
            };
            startWatcher.Start();

            // Listen for closed processes
            ManagementEventWatcher stopWatcher = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStopTrace");
            stopWatcher.EventArrived += (sender, e) => {
                // Get the name of the terminated process
                var name = e.NewEvent.Properties["ProcessName"].Value.ToString().ToLower();
                // Ensure the process exists in our dictionary
                if (runningProcesses.TryGetValue(name, out int count)) {
                    if (count == 1) // If there is only 1 process currently running, remove it (since that must've been the one that terminated)
                        runningProcesses.Remove(name);
                    else // Else, simply decrement the process count number
                        runningProcesses[name]--;
                }
            };
            stopWatcher.Start();
        }

        /// <summary>
        /// Returns whether the given process name is detected as running or not.
        /// </summary>
        public bool IsProcessRunning(string name) => runningProcesses.ContainsKey(name.ToLower());
    }
}
