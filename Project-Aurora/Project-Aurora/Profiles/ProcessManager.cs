using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using Aurora.Utils;
using Aurora.Settings;

namespace Aurora.Profiles
{
    public interface IProcessChanged
    {
        void ActiveProcessChanged(string key);
        void OpenBackgroundProcess(string key);
        void CloseBackgroundProcess(string key);
    }
    public class ProcessManager
    {

        private static readonly int  SleepTime = 3000;
        private static bool IsExit = false;
        private static Dictionary<string, LightEventConfig> EventConfigs { get; set; } = new Dictionary<string, LightEventConfig>();
        private static List<string> RunningBackgroundProcess = new List<string>();
        private static string PreviousActiveProcessKey = null;

        private static ActiveProcessMonitor ProcessMonitor;
        private static IProcessChanged Listener;

        public ProcessManager()
        {

        }

        public ProcessManager(IProcessChanged listener)
        {
            Listener = listener;
            ProcessMonitor = new ActiveProcessMonitor();
        }


        public void SubsribeForChange(LightEventConfig processConfig)
        {
            string key = processConfig.ID;
            if (string.IsNullOrWhiteSpace(key) || EventConfigs.ContainsKey(key))
                return;
            //Global.logger.LogLine("ProcessManager::SubsribeForChange()" + key);
            EventConfigs.Add(key, processConfig);
            if (processConfig.ProcessNames != null)
            {
                for (int i = 0; i < processConfig.ProcessNames.Length; i++)
                {
                    processConfig.ProcessNames[i] = processConfig.ProcessNames[i].ToLower();
                }
            }
        }
        public void Start()
        {
            IsExit = false;
            Thread thread1 = new Thread(UpdateActiveProcess);
            Thread thread2 = new Thread(UpdateBackgroundProcess);
            thread1.Start();
            thread2.Start();
        }
        public void Finish()
        {
            //Global.logger.LogLine("ProcessManager::Finished()");
            IsExit = true;
        }
        private static bool TryToMatchProcessTitle(string[] processTitles, string processTitle)
        {
            // Is title matching required?
            if (processTitles != null)
            {
                if (processTitles.Where(title => Regex.IsMatch(processTitle, title, RegexOptions.IgnoreCase)).Any())
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        // Used to match a process's name and optional window title to a profile
        private static string GetProfileKeyFromProcessData(string processName, string processTitle = null)
        {
            string processKeyByName = EventConfigs.Where(ec => ec.Value.ProcessNames.Contains(processName)).Select(ec => ec.Key).First();

            if (processKeyByName == null)
                return null;

            if(TryToMatchProcessTitle(EventConfigs[processKeyByName].ProcessTitles, processTitle))
            {
                return processKeyByName;
            }

            return null;
        }

        private static void UpdateActiveProcess()
        {
            while (!IsExit)
            {
                if (Global.Configuration.detection_mode == ApplicationDetectionMode.ForegroroundApp)
                {
                    ProcessMonitor.GetActiveWindowsProcessname();
                    string process_name = Path.GetFileName(ProcessMonitor.ProcessPath).ToLower();
                    string process_title = ProcessMonitor.GetActiveWindowsProcessTitle();

                    //(Global.Configuration.allow_wrappers_in_background && Global.net_listener != null && Global.net_listener.IsWrapperConnected && ((tempProfile = GetProfileFromProcessName(Global.net_listener.WrappedProcess)) != null) && tempProfile.Config.Type == LightEventType.Normal && tempProfile.IsEnabled)
                    string process_key = GetProfileKeyFromProcessData(process_name, process_title);
                    if (process_key != PreviousActiveProcessKey)
                    {
                        Listener.ActiveProcessChanged(process_key);
                        PreviousActiveProcessKey = process_key;
                    }

                }
                Thread.Sleep(SleepTime);
            }
            
        }
        private bool IsProcessRunningBackground(LightEventConfig processConfig)
        {
            foreach (var processName in processConfig.ProcessNames)
            {
                var processArray = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
                if (processArray.Length > 0)
                {
                    foreach (var process in processArray)
                    {
                        if (TryToMatchProcessTitle(processConfig.ProcessTitles, process.MainWindowTitle))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private void UpdateBackgroundProcess()
        {
            while (!IsExit)
            {
                foreach (var config in EventConfigs)
                {
                    if (IsProcessRunningBackground(config.Value))
                    {
                        if (!RunningBackgroundProcess.Contains(config.Key))
                        {
                            RunningBackgroundProcess.Add(config.Key);
                            Listener.OpenBackgroundProcess(config.Key);
                        }
                    }
                    else
                    {
                        if (RunningBackgroundProcess.Contains(config.Key))
                        {
                            RunningBackgroundProcess.Remove(config.Key);
                            Listener.CloseBackgroundProcess(config.Key);
                        }
                    }
                }
                Thread.Sleep(SleepTime);
            }

        }

    }
}

