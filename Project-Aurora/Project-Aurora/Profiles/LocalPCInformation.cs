using Aurora.Utils;
using NAudio.CoreAudioApi;
using System.Linq;
using System.Runtime.InteropServices;

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
        private static readonly AudioDeviceProxy captureProxy;
        private static readonly AudioDeviceProxy renderProxy;

        private MMDevice CaptureDevice {
            get {
                if (captureProxy != null)
                    captureProxy.DeviceId = Global.Configuration.GSIAudioCaptureDevice;
                return captureProxy?.Device;
            }
        }

        private MMDevice RenderDevice {
            get {
                if (renderProxy != null)
                    renderProxy.DeviceId = Global.Configuration.GSIAudioRenderDevice;
                return renderProxy?.Device;
            }
        }

        /// <summary>
        /// Current system volume (as set from the speaker icon)
        /// </summary>
        // Note: Manually checks if muted to return 0 since this is not taken into account with the MasterVolumeLevelScalar.
        public float SystemVolume => SystemVolumeIsMuted ? 0 : RenderDevice?.AudioEndpointVolume.MasterVolumeLevelScalar * 100 ?? 0;

        /// <summary>
        /// Gets whether the system volume is muted.
        /// </summary>
        public bool SystemVolumeIsMuted => RenderDevice?.AudioEndpointVolume.Mute ?? true;

        /// <summary>
        /// The volume level that is being recorded by the default microphone even when muted.
        /// </summary>
        public float MicrophoneLevel => CaptureDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// The volume level that is being emitted by the default speaker even when muted.
        /// </summary>
        public float SpeakerLevel => RenderDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// The volume level that is being recorded by the default microphone if not muted.
        /// </summary>
        public float MicLevelIfNotMuted => MicrophoneIsMuted ? 0 : CaptureDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// Gets whether the default microphone is muted.
        /// </summary>
        public bool MicrophoneIsMuted => CaptureDevice?.AudioEndpointVolume.Mute ?? true;
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

        #region Cursor Position
        private static CursorPositionNode _cursorPosition;
        public CursorPositionNode CursorPosition => _cursorPosition ?? (_cursorPosition = new CursorPositionNode());
        #endregion

        /// <summary>
        /// Returns whether or not the device dession is in a locked state.
        /// </summary>
        public bool IsDesktopLocked => DesktopUtils.IsDesktopLocked;

        private bool pendingAudioDeviceUpdate = false;

        static LocalPCInformation() {
            // Do not create a capture device if audio capture is disabled. Otherwise it will create a mic icon in win 10 and people will think we're spies.
            try
            {
                if (Global.Configuration.EnableAudioCapture)
                    captureProxy = new AudioDeviceProxy(Global.Configuration.GSIAudioCaptureDevice, DataFlow.Capture);
                renderProxy = new AudioDeviceProxy(Global.Configuration.GSIAudioRenderDevice, DataFlow.Render);
            }
            catch(COMException e)
            {
                Global.logger.Error("Error initializing audio device proxy in LocalPCInfo, this is probably caused by an incompatible audio software: " + e);
            }
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

    public class CursorPositionNode : Node
    {
        public float X => System.Windows.Forms.Cursor.Position.X;
        public float Y => System.Windows.Forms.Cursor.Position.Y;
    }
}
