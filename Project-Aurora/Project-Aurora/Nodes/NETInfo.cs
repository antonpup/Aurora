namespace Aurora.Nodes;

public class NETInfo : Node
{
    public float Usage => LocalPcInformation.HardwareMonitor.Net.BandwidthUsed;
    public float UploadSpeed => LocalPcInformation.HardwareMonitor.Net.UploadSpeedBytes;
    public float DownloadSpeed => LocalPcInformation.HardwareMonitor.Net.DownloadSpeedBytes;
}