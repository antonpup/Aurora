namespace Aurora.Profiles.EliteDangerous.Journal.Events
{
    public class SystemFaction
    {
        public string Name;
        public string FactionState;
    }
    public class FSDJump : JournalEvent
    {
        public string StarSystem;
        public long SystemAddress;
        public double[] StarPos;

        public string SystemAllegiance;
        public string SystemEconomy;
        public string SystemEconomy_Localised;
        public string SystemSecondEconomy;
        public string SystemSecondEconomy_Localised;
        public string SystemGovernment;
        public string SystemGovernment_Localised;
        public string SystemSecurity;
        public string SystemSecurity_Localised;

        public long Population;
        public double JumpDist;
        public double FuelUsed;
        public double FuelLevel;

//        public SystemFaction SystemFaction;
    }
}