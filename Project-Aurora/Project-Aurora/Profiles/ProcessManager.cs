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
        private static Dictionary<string, string> EventProcesses { get; set; } = new Dictionary<string, string>();
        private static Dictionary<string, string> EventTitles { get; set; } = new Dictionary<string, string>();
        private static string PreviousActiveProcess = "";
        private static List<string> RunningBackgroundProcess = new List<string>();

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


        public void SubsribeForChange(string key, string[] processNames, string[] processTitles)
        {
            //Global.logger.LogLine("ProcessManager::SubsribeForChange()" + key);
            if (processNames != null)
            {
                foreach (string exe in processNames)
                {
                    //if (!exe.Equals(key))
                        EventProcesses.Add(exe.ToLower(), key);
                }
            }
            if (processTitles != null)
                foreach (string titleRx in processTitles)
                    EventTitles.Add(titleRx, key);
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
                    if (EventProcesses.ContainsKey(process_name) || EventTitles.Where(title => Regex.IsMatch(process_title, title.Key, RegexOptions.IgnoreCase)).Any())
                    {
                        if (process_name != PreviousActiveProcess)
                        {
                            Listener.ActiveProcessChanged(EventProcesses[process_name]);
                            PreviousActiveProcess = process_name;
                        }
                    }
                    else
                    {
                        if (PreviousActiveProcess != string.Empty)
                        {
                            PreviousActiveProcess = string.Empty;
                            Listener.ActiveProcessChanged(null);
                        }
                    }
                }
                Thread.Sleep(SleepTime);
            }
            
        }
        private void UpdateBackgroundProcess()
        {
            while (!IsExit)
            {
                foreach (var process in EventProcesses)
                {
                    if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.Key)).Length > 0)
                    {
                        if (!RunningBackgroundProcess.Contains(process.Value))
                        {
                            RunningBackgroundProcess.Add(process.Value);
                            Listener.OpenBackgroundProcess(process.Value);
                        }
                    }
                    else
                    {
                        if (RunningBackgroundProcess.Contains(process.Value))
                        {
                            RunningBackgroundProcess.Remove(process.Value);
                            Listener.CloseBackgroundProcess(process.Value);
                        }
                    }
                }
                Thread.Sleep(SleepTime);
            }

        }
    }
}

