using System;
using Aurora.Modules.HardwareMonitor;
using Aurora.Modules.ProcessMonitor;
using Aurora.Utils;
using Common.Utils;
using NAudio.CoreAudioApi;

namespace Aurora.Nodes;

/// <summary>
/// Class representing local computer information
/// </summary>
public class LocalPcInformation : Node
{
    public static IHardwareMonitor HardwareMonitor { get; set; } = new NoopHardwareMonitor();

    //TODO Time Node and remap old values
    #region Time Properties

    public int CurrentMonth => Time.GetMonths();
    public int CurrentDay => Time.GetDays();

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

    //TODO Audio node
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

    private static CelestialData? _celestialData;
    public CelestialData CelestialData => _celestialData ??= new CelestialData();

    private DesktopNode? _desktop;
    public DesktopNode Desktop => _desktop ??= new DesktopNode();
}