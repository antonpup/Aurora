using Aurora.Profiles.EliteDangerous.Journal;
using Aurora.Profiles.EliteDangerous.Journal.Events;

namespace Aurora.Profiles.EliteDangerous.GSI.Nodes
{
    public enum FighterStatus
    {
        None, Launched, Unmanned
    }

    public enum FSDState
    {
        Idle, JumpingSupercruise, JumpingHyperspace
    }

    public class SimpleShipLoadout
    {
        public bool hasChaff;
        public bool hasHeatSink;
        public bool hasShieldCellBank;
    }
    
    public class Journal : Node<Controls>
    {
        public FighterStatus fighterStatus = FighterStatus.None;
        
        public SimpleShipLoadout shipLoadout = new SimpleShipLoadout();
        public SimpleShipLoadout fighterLoadout = new SimpleShipLoadout();
        
        public FSDState fsdState;
        private long fsdChargeStartTime = -1;
        public string jumpStarType = null;
        
        private bool nextLoadoutIsFighter = false;
        
        private void SetModulesFromLoadout(Loadout loadout)
        {
            SimpleShipLoadout loadoutToChange = nextLoadoutIsFighter ? fighterLoadout : shipLoadout;
            
            bool hasChaff = false;
            bool hasHeatSink = false;
            bool hasShieldCellBank = false;

            foreach(LoadoutModule module in loadout.Modules) {
                if(module.Item.StartsWith("hpt_chafflauncher_")) {
                    hasChaff = true;
                } else if(module.Item.StartsWith("hpt_heatsinklauncher_")) {
                    hasHeatSink = true;
                } else if(module.Item.StartsWith("int_shieldcellbank_")) {
                    hasShieldCellBank = true;
                }
            }
            
            loadoutToChange.hasChaff = hasChaff;
            loadoutToChange.hasHeatSink = hasHeatSink;
            loadoutToChange.hasShieldCellBank = hasShieldCellBank;

            nextLoadoutIsFighter = false;
        }

        private void ResetFsd()
        {
            fsdState = FSDState.Idle;
            fsdChargeStartTime = -1;
        }

        public void ProcessEventForFSD(JournalEvent journalEvent)
        {
            switch(journalEvent.@event) {
                case EventType.StartJump:
                    StartJump startJump = (StartJump) journalEvent;
                    if(startJump.JumpType == JumpType.Hyperspace)
                    {
                        fsdState = FSDState.JumpingHyperspace;
                        jumpStarType = startJump.StarClass.ToLower();
                    } else {
                        fsdState = FSDState.JumpingSupercruise;
                        jumpStarType = null;
                    }

                    //Should start FSD countdown animation
                    fsdChargeStartTime = Utils.Time.GetMillisecondsSinceEpoch();
                    break;
                case EventType.SupercruiseEntry:
                    ResetFsd();
                    break;
                case EventType.SupercruiseExit:
                    ResetFsd();
                    break;
                case EventType.FSDJump:
                    ResetFsd();
                    //Should stop hyperspace animation
                    break;
                case EventType.Music:
                    if (fsdState != FSDState.JumpingHyperspace && ((Music) journalEvent).MusicTrack.Equals("NoTrack"))
                    {
                        ResetFsd();
                        //Should start hyperspace animation
                    }
                    break;
            }
        }

        public void ProcessEvent(JournalEvent journalEvent)
        {
            switch (journalEvent.@event) {
                case EventType.LaunchFighter:
                    fighterStatus = FighterStatus.Launched;
                    nextLoadoutIsFighter = true;
                    break;
                case EventType.FighterDestroyed:
                case EventType.DockFighter:
                    fighterStatus = FighterStatus.None;
                    break;
                case EventType.Loadout:
                    SetModulesFromLoadout((Loadout) journalEvent);
                    break;
            }
            
            ProcessEventForFSD(journalEvent);
        }
    }
}