using Aurora.Profiles;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NAudio.CoreAudioApi;

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
    public interface IGameState
    {
        /// <summary>
        /// Information about the local system
        /// </summary>
        //LocalPCInformation LocalPCInfo { get; }

        Newtonsoft.Json.Linq.JObject _ParsedData { get; set; }
        string json { get; set; }

        String GetNode(string name);
    }

    public class GameState<T> : StringProperty<T>, IGameState where T : GameState<T>
    {
        private static LocalPCInformation _localpcinfo;

        /// <summary>
        /// Information about the local system
        /// </summary>
        public LocalPCInformation LocalPCInfo => _localpcinfo ?? (_localpcinfo = new LocalPCInformation());

        public JObject _ParsedData { get; set; }
        public string json { get; set; }

        /// <summary>
        /// Creates a default GameState instance.
        /// </summary>
        public GameState() : base()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState(string json_data) : base()
        {
            if (String.IsNullOrWhiteSpace(json_data))
                json_data = "{}";

            json = json_data;
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json_data);
        }

        /// <summary>
        /// A copy constructor, creates a GameState instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState(IGameState other_state) : base()
        {
            _ParsedData = other_state._ParsedData;
            json = other_state.json;
        }

        public String GetNode(string name)
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

    public class GameState : GameState<GameState>
    {
        public GameState() : base() { }
        public GameState(IGameState gs) : base(gs) { }
        public GameState(string json) : base(json) { }
    }

    /// <summary>
    /// Class representing local computer information
    /// </summary>
    public class LocalPCInformation : Node<LocalPCInformation> {
        #region Time Properties
        /// <summary>
        /// The current hour
        /// </summary>
        public int CurrentHour => Utils.Time.GetHours();

        /// <summary>
        /// The current minute
        /// </summary>
        public int CurrentMinute => Utils.Time.GetMinutes();

        /// <summary>
        /// The current second
        /// </summary>
        public int CurrentSecond => Utils.Time.GetSeconds();

        /// <summary>
        /// The current millisecond
        /// </summary>
        public int CurrentMillisecond => Utils.Time.GetMilliSeconds();

        /// <summary>
        /// The total number of milliseconds since the epoch
        /// </summary>
        public long MillisecondsSinceEpoch => Utils.Time.GetMillisecondsSinceEpoch();
        #endregion

        #region Audio Properties
        private static readonly MMDeviceEnumerator mmDeviceEnumerator = new MMDeviceEnumerator();
        private static readonly NAudio.Wave.WaveInEvent waveInEvent = new NAudio.Wave.WaveInEvent();

        /// <summary>
        /// Gets the default endpoint for output (playback) devices e.g. speakers, headphones, etc.
        /// This will return null if there are no playback devices available.
        /// </summary>
        private MMDevice DefaultAudioOutDevice {
            get {
                try { return mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console); }
                catch { return null; }
            }
        }

        /// <summary>
        /// Gets the default endpoint for input (recording) devices e.g. microphones.
        /// This will return null if there are no recording devices available.
        /// </summary>
        private MMDevice DefaultAudioInDevice {
            get {
                try { return mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console); }
                catch { return null; }
            }
        }

        /// <summary>
        /// Current system volume (as set from the speaker icon)
        /// </summary>
        // Note: Manually checks if muted to return 0 since this is not taken into account with the MasterVolumeLevelScalar.
        public float SystemVolume => SystemVolumeIsMuted ? 0 : DefaultAudioOutDevice?.AudioEndpointVolume.MasterVolumeLevelScalar * 100 ?? 0;

        /// <summary>
        /// Gets whether the system volume is muted.
        /// </summary>
        public bool SystemVolumeIsMuted => DefaultAudioOutDevice?.AudioEndpointVolume.Mute ?? true;

        /// <summary>
        /// The volume level that is being recorded by the default microphone even when muted.
        /// </summary>
        public float MicrophoneLevel => DefaultAudioInDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// The volume level that is being emitted by the default speaker even when muted.
        /// </summary>
        public float SpeakerLevel => DefaultAudioOutDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// The volume level that is being recorded by the default microphone if not muted.
        /// </summary>
        public float MicLevelIfNotMuted => MicrophoneIsMuted ? 0 : DefaultAudioInDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// Gets whether the default microphone is muted.
        /// </summary>
        public bool MicrophoneIsMuted => DefaultAudioInDevice?.AudioEndpointVolume.Mute ?? true;
        #endregion

        #region Device Properties
        /// <summary>
        /// Battery level of a dualshock controller
        /// </summary>
        public int DS4Battery => Global.dev_manager.GetInitializedDevices().OfType<Devices.Dualshock.DualshockDevice>().FirstOrDefault()?.Battery ?? 0;
        /// <summary>
        /// Whether or not thr dualshock controller is charging
        /// </summary>
        public bool DS4Charging => Global.dev_manager.GetInitializedDevices().OfType<Devices.Dualshock.DualshockDevice>().FirstOrDefault()?.Charging ?? false;
        #endregion

        #region CPU Properties
        /// <summary>
        /// Legacy cpu usage prop, DEPRECATED
        /// </summary>
        public float CPUUsage => CPU.Usage;

        private static CPUInfo _cpuInfo;
        public CPUInfo CPU => _cpuInfo ?? (_cpuInfo = new CPUInfo());
        #endregion

        #region RAM Properties
        /// <summary>
        /// Used RAM, DEPRECATED
        /// </summary>
        public long MemoryUsed => RAM.Used;

        /// <summary>
        /// Available RAM, DEPRECATED
        /// </summary>
        public long MemoryFree => RAM.Free;

        /// <summary>
        /// Total RAM, DEPRECATED
        /// </summary>
        public long MemoryTotal => MemoryFree + MemoryUsed;

        private static RAMInfo _ramInfo;
        public RAMInfo RAM => _ramInfo ?? (_ramInfo = new RAMInfo());
        #endregion

        #region GPU Properties
        private static GPUInfo _gpuInfo;
        public GPUInfo GPU => _gpuInfo ?? (_gpuInfo = new GPUInfo());
        #endregion

        /// <summary>
        /// Returns whether or not the device dession is in a locked state.
        /// </summary>
        public bool IsDesktopLocked => Utils.DesktopUtils.IsDesktopLocked;

        static LocalPCInformation() {
            void StartStopRecording() {
                // We must start recording to be able to capture audio in, but only do this if the user has the option set. Allowing them
                // to turn it off will give them piece of mind we're not spying on them and will stop the Windows 10 mic icon appearing.
                try {
                    if (Global.Configuration.EnableAudioCapture)
                        waveInEvent.StartRecording();
                    else
                        waveInEvent.StopRecording();
                } catch { }
            }

            StartStopRecording();
            Global.Configuration.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "EnableAudioCapture")
                    StartStopRecording();
            };
        }
    }

    public class CPUInfo : Node<CPUInfo>
    {
        /// <summary>
        /// Represents the CPU usage from 0 to 100
        /// </summary>
        public float Usage => Utils.HardwareMonitor.CPU.CPUTotalLoad;

        /// <summary>
        /// Represents the temperature of the cpu die in celsius
        /// </summary>
        public float Temperature => Utils.HardwareMonitor.CPU.CPUDieTemp;

        /// <summary>
        /// Represents the CPU power draw in watts
        /// </summary>
        public float PowerUsage => Utils.HardwareMonitor.CPU.CPUPower;
    }

    public class RAMInfo : Node<RAMInfo>
    {
        /// <summary>
        /// Used system memory in megabytes
        /// </summary>
        public long Used => (long)(Utils.HardwareMonitor.RAM.RAMUsed * 1024f);

        /// <summary>
        /// Free system memory in megabytes
        /// </summary>
        public long Free => (long)(Utils.HardwareMonitor.RAM.RAMFree * 1024f);

        /// <summary>
        /// Total system memory in megabytes
        /// </summary>
        public long Total => Free + Used;
    }

    public class GPUInfo : Node<GPUInfo>
    {
        public float Usage => Utils.HardwareMonitor.GPU.GPUCoreLoad;
        public float Temperature => Utils.HardwareMonitor.GPU.GPUCoreTemp;
        public float PowerUsage => Utils.HardwareMonitor.GPU.GPUPower;
        public float FanRPM => Utils.HardwareMonitor.GPU.GPUFan;
        public float CoreClock => Utils.HardwareMonitor.GPU.GPUCoreClock;
        public float MemoryClock => Utils.HardwareMonitor.GPU.GPUMemoryClock;
        public float ShaderClock => Utils.HardwareMonitor.GPU.GPUShaderClock;
        public float MemoryControllerUsage => Utils.HardwareMonitor.GPU.GPUMemoryCLoad;
        public float VideoEngineUsage => Utils.HardwareMonitor.GPU.GPUVideoEngineLoad;
        public float MemoryUsed => Utils.HardwareMonitor.GPU.GPUMemoryUsed;
        public float MemoryFree => MemoryTotal - MemoryUsed;
        public float MemoryTotal => Utils.HardwareMonitor.GPU.GPUMemoryTotal;
    }
}
