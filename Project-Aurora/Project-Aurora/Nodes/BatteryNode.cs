using System.Windows.Forms;

namespace Aurora.Nodes;

public class BatteryNode : Node
{
    public BatteryChargeStatus ChargeStatus => SystemInformation.PowerStatus.BatteryChargeStatus;
    public bool PluggedIn => SystemInformation.PowerStatus.PowerLineStatus != PowerLineStatus.Offline; //If it is unknown I assume it is plugedIn
    public float LifePercent => SystemInformation.PowerStatus.BatteryLifePercent;
    public int SecondsRemaining => SystemInformation.PowerStatus.BatteryLifeRemaining;
}