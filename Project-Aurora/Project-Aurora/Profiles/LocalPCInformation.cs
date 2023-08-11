using Aurora.Devices.Dualshock;
using Aurora.Utils;
using NAudio.CoreAudioApi;
using System.Linq;
using System.Windows.Forms;
using Aurora.Modules.HardwareMonitor;
using Aurora.Modules.Media;
using Aurora.Modules.ProcessMonitor;
using JetBrains.Annotations;

namespace Aurora.Profiles {
    /// <summary>
    /// Class representing local computer information
    /// </summary>
    public class LocalPcInformation : Node
    {
        public static IHardwareMonitor HardwareMonitor { get; set; } = new NoopHardwareMonitor();
        
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

        private MMDevice? CaptureDevice => Global.CaptureProxy?.Device;

        private MMDevice? RenderDevice => Global.RenderProxy?.Device;

        /// <summary>
        /// Current system volume (as set from the speaker icon)
        /// </summary>
        // Note: Manually checks if muted to return 0 since this is not taken into account with the MasterVolumeLevelScalar.
        public float SystemVolume => SystemVolumeIsMuted ? 0 : RenderDevice?.AudioEndpointVolume?.MasterVolumeLevelScalar ?? 0 * 100;

        /// <summary>
        /// Gets whether the system volume is muted.
        /// </summary>
        public bool SystemVolumeIsMuted => RenderDevice?.AudioEndpointVolume?.Mute ?? true;

        /// <summary>
        /// The volume level that is being recorded by the default microphone even when muted.
        /// </summary>
        public float MicrophoneLevel => CaptureDevice?.AudioMeterInformation?.MasterPeakValue ?? 0 * 100;

        /// <summary>
        /// The volume level that is being emitted by the default speaker even when muted.
        /// </summary>
        public float SpeakerLevel => RenderDevice?.AudioMeterInformation?.MasterPeakValue ?? 0 * 100;

        /// <summary>
        /// The volume level that is being recorded by the default microphone if not muted.
        /// </summary>
        public float MicLevelIfNotMuted => MicrophoneIsMuted ? 0 : CaptureDevice?.AudioMeterInformation?.MasterPeakValue ?? 0 * 100;

        /// <summary>
        /// Gets whether the default microphone is muted.
        /// </summary>
        public bool MicrophoneIsMuted => CaptureDevice?.AudioEndpointVolume.Mute ?? true;

        /// <summary>
        /// Selected Audio Device's index.
        /// </summary>
        public string PlaybackDeviceName => Global.RenderProxy?.DeviceName ?? "";
        #endregion

        #region Device Properties

        private readonly DualshockDevice? _ds4Device = DualshockDevice.Instance;
        /// <summary>
        /// Battery level of a dualshock controller
        /// </summary>
        public int DS4Battery => _ds4Device?.Battery ?? -1;
        /// <summary>
        /// Whether or not thr dualshock controller is charging
        /// </summary>
        public bool DS4Charging => _ds4Device?.Charging ?? false;
        /// <summary>
        /// Latency of the controller in ms
        /// </summary>
        public double DS4Latency => _ds4Device?.Latency ?? -1;
        #endregion

        #region CPU Properties
        /// <summary>
        /// Legacy cpu usage prop, DEPRECATED
        /// </summary>
        public float CPUUsage => CPU.Usage;

        private static CPUInfo? _cpuInfo;
        public CPUInfo CPU => _cpuInfo ??= new CPUInfo();
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

        private static RAMInfo? _ramInfo;
        public RAMInfo RAM => _ramInfo ??= new RAMInfo();
        #endregion

        #region GPU Properties
        private static GPUInfo? _gpuInfo;
        public GPUInfo GPU => _gpuInfo ??= new GPUInfo();
        #endregion

        #region NET Properties
        private static NETInfo? _netInfo;
        public NETInfo NET => _netInfo ??= new NETInfo();
        #endregion

        #region Cursor Position
        private static CursorPositionNode? _cursorPosition;
        public CursorPositionNode CursorPosition => _cursorPosition ??= new CursorPositionNode();
        #endregion

        #region Battery Properties
        private static BatteryNode? _battery;
        public BatteryNode Battery => _battery ??= new BatteryNode();
        #endregion

        #region Media Properties
        private static MediaNode? _media;
        public MediaNode Media => _media ??= new MediaNode();
        #endregion

        /// <summary>
        /// Returns whether or not the device dession is in a locked state.
        /// </summary>
        public bool IsDesktopLocked => DesktopUtils.IsDesktopLocked;

        /// <summary>
        /// Returns focused window's name.
        /// </summary>
        public string ActiveWindowName => ActiveProcessMonitor.Instance.ProcessTitle;

        /// <summary>
        /// Returns focused window's process name.
        /// </summary>
        public string ActiveProcess => ActiveProcessMonitor.Instance.ProcessName;
    }

    public class CPUInfo : Node
    {
        /// <summary>
        /// Represents the CPU usage from 0 to 100
        /// </summary>
        public float Usage => LocalPcInformation.HardwareMonitor.Cpu.CPULoad;

        /// <summary>
        /// Represents the temperature of the cpu die in celsius
        /// </summary>
        public float Temperature => LocalPcInformation.HardwareMonitor.Cpu.CPUTemp;

        /// <summary>
        /// Represents the CPU power draw in watts
        /// </summary>
        public float PowerUsage => LocalPcInformation.HardwareMonitor.Cpu.CPUPower;
    }

    public class RAMInfo : Node
    {
        /// <summary>
        /// Used system memory in megabytes
        /// </summary>
        public long Used => (long)(LocalPcInformation.HardwareMonitor.Ram.RAMUsed * 1024f);

        /// <summary>
        /// Free system memory in megabytes
        /// </summary>
        public long Free => (long)(LocalPcInformation.HardwareMonitor.Ram.RAMFree * 1024f);

        /// <summary>
        /// Total system memory in megabytes
        /// </summary>
        public long Total => Free + Used;
    }

    public class GPUInfo : Node
    {
        public float Usage => LocalPcInformation.HardwareMonitor.Gpu.GPULoad;
        public float Temperature => LocalPcInformation.HardwareMonitor.Gpu.GPUCoreTemp;
        public float PowerUsage => LocalPcInformation.HardwareMonitor.Gpu.GPUPower;
        public float FanRPM => LocalPcInformation.HardwareMonitor.Gpu.GPUFan;
    }

    public class NETInfo : Node
    {
        public float Usage => LocalPcInformation.HardwareMonitor.Net.BandwidthUsed;
        public float UploadSpeed => LocalPcInformation.HardwareMonitor.Net.UploadSpeedBytes;
        public float DownloadSpeed => LocalPcInformation.HardwareMonitor.Net.DownloadSpeedBytes;
    }

    public class CursorPositionNode : Node
    {
        public float X => Cursor.Position.X;
        public float Y => Cursor.Position.Y;
    }

    public class BatteryNode : Node
    {
        public BatteryChargeStatus ChargeStatus => SystemInformation.PowerStatus.BatteryChargeStatus;
        public bool PluggedIn => SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Offline; //If it is unknown I assume it is plugedIn
        public float LifePercent => SystemInformation.PowerStatus.BatteryLifePercent;
        public int SecondsRemaining => SystemInformation.PowerStatus.BatteryLifeRemaining;
    }

    public class MediaNode : Node
    {
        public bool MediaPlaying => MediaMonitor.MediaPlaying;
        public bool HasMedia => MediaMonitor.HasMedia;
        public bool HasNextMedia => MediaMonitor.HasNextMedia;
        public bool HasPreviousMedia => MediaMonitor.HasPreviousMedia;
    }
}
