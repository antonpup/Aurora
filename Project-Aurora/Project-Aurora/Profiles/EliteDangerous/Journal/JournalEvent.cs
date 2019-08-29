using System;

namespace Aurora.Profiles.EliteDangerous.Journal
{
    public enum EventType
    {
        FSDTarget,
        StartJump,
        SupercruiseEntry,
        SupercruiseExit,
        Fileheader,
        FSDJump,
        Loadout,
        Music,
        LaunchFighter,
        DockFighter,
        FighterDestroyed,
        FighterRebuilt
    }

    public class JournalEvent
    {
        public DateTime timestamp;
        public EventType @event;
    }
}