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

        #region Memory Properties
        /// <summary>
        /// Used RAM
        /// </summary>
        public long MemoryUsed => (long)((Utils.HardwareMonitor.RAMUsed?.Value ?? 0f) * 1024f);

        /// <summary>
        /// Available RAM
        /// </summary>
        public long MemoryFree => (long)((Utils.HardwareMonitor.RAMFree?.Value ?? 0f) * 1024f);

        /// <summary>
        /// Total RAM
        /// </summary>
        public long MemoryTotal => MemoryFree + MemoryUsed;
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
        /// Current CPU Usage
        /// </summary>
        public float CPUUsage => Utils.HardwareMonitor.CPUTotalLoad?.Value ?? 0;

        /// <summary>
        /// Current temperature of the CPU die
        /// </summary>
        public float CPUTemp => Utils.HardwareMonitor.CPUDieTemp?.Value ?? 0;

        /// <summary>
        /// Current power usage in watts
        /// </summary>
        public float CPUPowerUsage => Utils.HardwareMonitor.CPUPower?.Value ?? 0;

        /// <summary>
        /// Current RPM of the CPU fan
        /// </summary>
        public float CPUFanRPM => Utils.HardwareMonitor.CPUFan?.Value ?? 0;
        #endregion

        #region GPU Properties
        public float GPUCoreTemp => Utils.HardwareMonitor.GPUCoreTemp?.Value ?? 0;
        public float GPUFan => Utils.HardwareMonitor.GPUFan?.Value ?? 0;
        public float GPUCoreClock => Utils.HardwareMonitor.GPUCoreClock?.Value ?? 0;
        public float GPUMemoryCClock => Utils.HardwareMonitor.GPUMemoryClock?.Value ?? 0;
        public float GPUShaderClock => Utils.HardwareMonitor.GPUShaderClock?.Value ?? 0;
        public float GPUCoreLoad => Utils.HardwareMonitor.GPUCoreLoad?.Value ?? 0;
        public float GPUMemoryCLoad => Utils.HardwareMonitor.GPUMemoryCLoad?.Value ?? 0;
        public float GPUVideoEngineLoad => Utils.HardwareMonitor.GPUVideoEngineLoad?.Value ?? 0;
        public float GPUMemoryTotal => Utils.HardwareMonitor.GPUMemoryTotal?.Value ?? 0;
        public float GPUMemoryUsed => Utils.HardwareMonitor.GPUMemoryUsed?.Value ?? 0;
        public float GPUPower => Utils.HardwareMonitor.GPUPower?.Value ?? 0;
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
}
