namespace Aurora.Profiles.EliteDangerous.GSI.Nodes
{
    public enum FighterStatus
    {
        None, Launched, Unmanned
    }
    
    public class Journal : Node<Controls>
    {
        public FighterStatus fighterStatus = FighterStatus.None;
        public bool hasChaff = false;
        public bool hasHeatSink = false;
        public bool hasShieldCellBank = false;
    }
}