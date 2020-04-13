namespace Aurora.Profiles.EliteDangerous.Journal.Events
{
    public enum JumpType {
        Hyperspace, Supercruise
    }
    public class StartJump : JournalEvent
    {
        public JumpType JumpType;
        public string StarSystem;
        public long SystemAddress;
        public string StarClass;
    }
}