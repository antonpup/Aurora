using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles
{
    public class GameStateIgnoreAttribute : Attribute
    { }

    public class RangeAttribute : Attribute
    {
        public int Start { get; set; }

        public int End { get; set; }

        public RangeAttribute(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    /// <summary>
    /// A class representing various information retaining to the game.
    /// </summary>
    public class GameState
    {
        private static LocalPCInformation _localpcinfo;

        /// <summary>
        /// Information about the local system
        /// </summary>
        public LocalPCInformation LocalPCInfo
        {
            get
            {
                if (_localpcinfo == null)
                    _localpcinfo = new LocalPCInformation();

                return _localpcinfo;
            }
        }

        protected Newtonsoft.Json.Linq.JObject _ParsedData;
        protected string json;

        /// <summary>
        /// Creates a default GameState instance.
        /// </summary>
        public GameState()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState(string json_data)
        {
            if (String.IsNullOrWhiteSpace(json_data))
            {
                json_data = "{}";
            }

            json = json_data;
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json_data);
        }

        /// <summary>
        /// A copy constructor, creates a GameState instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState(GameState other_state)
        {
            _ParsedData = other_state._ParsedData;
            json = other_state.json;
        }

        internal String GetNode(string name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(name, out value))
                return value.ToString();
            else
                return "";
        }

        /// <summary>
        /// Displays the JSON, representative of the GameState data
        /// </summary>
        /// <returns>JSON String</returns>
        public override string ToString()
        {
            return json;
        }
    }

    static class PerformanceInfo
    {
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        public static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            ulong availableMemory = new ComputerInfo().AvailablePhysicalMemory;
            return Convert.ToInt64(availableMemory / 1048576);
        }

        public static Int64 GetTotalMemoryInMiB()
        {
            ulong availableMemory = new ComputerInfo().TotalPhysicalMemory;
            return Convert.ToInt64(availableMemory / 1048576);

        }
    }

    /// <summary>
    /// Class representing local computer information
    /// </summary>
    public class LocalPCInformation : Node
    {
        /// <summary>
        /// The current hour
        /// </summary>
        public static int CurrentHour { get { return Utils.Time.GetHours(); } }

        /// <summary>
        /// The current minute
        /// </summary>
        public static int CurrentMinute { get { return Utils.Time.GetMinutes(); } }

        /// <summary>
        /// The current second
        /// </summary>
        public static int CurrentSecond { get { return Utils.Time.GetSeconds(); } }

        /// <summary>
        /// The current millisecond
        /// </summary>
        public static int CurrentMillisecond { get { return Utils.Time.GetMilliSeconds(); } }

        /// <summary>
        /// Used RAM
        /// </summary>
        public static long MemoryUsed { get { return PerformanceInfo.GetTotalMemoryInMiB() - PerformanceInfo.GetPhysicalAvailableMemoryInMiB(); } }

        /// <summary>
        /// Available RAM
        /// </summary>
        public static long MemoryFree { get { return PerformanceInfo.GetPhysicalAvailableMemoryInMiB(); } }

        /// <summary>
        /// Total RAM
        /// </summary>
        public static long MemoryTotal { get { return PerformanceInfo.GetTotalMemoryInMiB(); } }

        private static PerformanceCounter _CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        private static float _CPUUsage = 0.0f;
        private static float _SmoothCPUUsage = 0.0f;

        private static System.Timers.Timer cpuCounterTimer;

        /// <summary>
        /// Current CPU Usage
        /// </summary>
        public static float CPUUsage
        {
            get
            {
                //Global.logger.LogLine($"_CPUUsage = {_CPUUsage}\t\t_SmoothCPUUsage = {_SmoothCPUUsage}");

                if (_SmoothCPUUsage < _CPUUsage)
                    _SmoothCPUUsage += (_CPUUsage - _SmoothCPUUsage) / 10.0f;
                else if (_SmoothCPUUsage > _CPUUsage)
                    _SmoothCPUUsage -= (_SmoothCPUUsage - _CPUUsage) / 10.0f;

                return _SmoothCPUUsage;
            }
        }

        internal LocalPCInformation() : base()
        {
            if (cpuCounterTimer == null)
            {
                cpuCounterTimer = new System.Timers.Timer(1000);
                cpuCounterTimer.Elapsed += CpuCounterTimer_Elapsed; ;
                cpuCounterTimer.Start();
            }
        }

        private void CpuCounterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _CPUUsage = (_CPUUsage + _CPUCounter.NextValue()) / 2.0f;
            }
            catch (Exception exc)
            {
                Global.logger.LogLine("PerformanceCounter exception: " + exc, Logging_Level.Error);
            }
        }
    }
}
