using Aurora.Utils;
using NAudio.CoreAudioApi;
using System.Linq;

namespace Aurora.Profiles {
    /// <summary>
    /// Class representing local computer information
    /// </summary>
    public class LocalPCInformation : Node {
        #region Time Properties
        /// <summary>
        /// The current hour
        /// </summary>
        public int CurrentHour => Time.GetHours();

        /// <summary>
        /// The current minute
        /// </summary>
        public int CurrentMinute => Time.GetMinutes();

        /// <summary>
        /// The current second
        /// </summary>
        public int CurrentSecond => Time.GetSeconds();

        /// <summary>
        /// The current millisecond
        /// </summary>
        public int CurrentMillisecond => Time.GetMilliSeconds();

        /// <summary>
        /// The total number of milliseconds since the epoch
        /// </summary>
        public long MillisecondsSinceEpoch => Time.GetMillisecondsSinceEpoch();
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

        #region NET Properties
        private static NETInfo _netInfo;
        public NETInfo NET => _netInfo ?? (_netInfo = new NETInfo());
        #endregion

        /// <summary>
        /// Returns whether or not the device dession is in a locked state.
        /// </summary>
        public bool IsDesktopLocked => DesktopUtils.IsDesktopLocked;

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

    public class CPUInfo : Node
    {
        /// <summary>
        /// Represents the CPU usage from 0 to 100
        /// </summary>
        public float Usage => HardwareMonitor.CPU.CPULoad;

        /// <summary>
        /// Represents the temperature of the cpu die in celsius
        /// </summary>
        public float Temperature => HardwareMonitor.CPU.CPUTemp;

        /// <summary>
        /// Represents the CPU power draw in watts
        /// </summary>
        public float PowerUsage => HardwareMonitor.CPU.CPUPower;
    }

    public class RAMInfo : Node
    {
        /// <summary>
        /// Used system memory in megabytes
        /// </summary>
        public long Used => (long)(HardwareMonitor.RAM.RAMUsed * 1024f);

        /// <summary>
        /// Free system memory in megabytes
        /// </summary>
        public long Free => (long)(HardwareMonitor.RAM.RAMFree * 1024f);

        /// <summary>
        /// Total system memory in megabytes
        /// </summary>
        public long Total => Free + Used;
    }

    public class GPUInfo : Node
    {
        public float Usage => HardwareMonitor.GPU.GPULoad;
        public float Temperature => HardwareMonitor.GPU.GPUCoreTemp;
        public float PowerUsage => HardwareMonitor.GPU.GPUPower;
        public float FanRPM => HardwareMonitor.GPU.GPUFan;
    }

    public class NETInfo : Node
    {
        public float Usage => HardwareMonitor.NET.BandwidthUsed;
        public float UploadSpeed => HardwareMonitor.NET.UploadSpeedBytes;
        public float DownloadSpeed => HardwareMonitor.NET.DownloadSpeedBytes;
    }
}
