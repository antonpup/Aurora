using System;
using System.Collections.Generic;
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

    public class ActiveProcessMonitor
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
            catch(Exception exc)
            {

            }
        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (Global.Configuration.detection_mode == Settings.ApplicationDetectionMode.WindowsEvents)
            {
                string active_process = GetActiveWindowsProcessname();

                if (!String.IsNullOrWhiteSpace(active_process))
                {
                    ProcessPath = active_process;
                    //Global.logger.LogLine("Process changed: " + process_path, Logging_Level.Info);
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        // TODO: Move this to own util
        [DllImport("user32.dll")]
        public static extern Boolean GetLastInputInfo(ref tagLASTINPUTINFO plii);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("Oleacc.dll")]
        static extern IntPtr GetProcessHandleFromHwnd(IntPtr whandle);
        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        public string GetActiveWindowsProcessname()
        {
            try
            {
                IntPtr windowHandle = IntPtr.Zero;
                IntPtr processhandle = IntPtr.Zero;
                IntPtr zeroHandle = IntPtr.Zero;
                windowHandle = GetForegroundWindow();
                processhandle = GetProcessHandleFromHwnd(windowHandle);

                StringBuilder sb = new StringBuilder(4048);
                GetModuleFileNameEx(processhandle, zeroHandle, sb, 4048);
                //Global.logger.LogLine("Current Foreground Window: " + sb.ToString(), Logging_Level.Info);

                System.IO.Path.GetFileName(sb.ToString());


                return sb.ToString();
            }
            catch (ArgumentException aex)
            {
                Global.logger.LogLine("Argument Exception: " + aex);
                return "";
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("Exception in GetActiveWindowsProcessname" + exc);
                return "";
            }
        }
    }
}
