namespace Aurora.Profiles.EliteDangerous.Journal.Events
{
    public class SupercruiseExit : JournalEvent
    {
        public string StarSystem;
        public long SystemAddress;
        public string Body;
        public int BodyID;
        public string BodyType;
    }
}