using Aurora.Profiles.EliteDangerous.Journal;
using Aurora.Profiles.EliteDangerous.Journal.Events;

namespace Aurora.Profiles.EliteDangerous.GSI.Nodes
{
    public enum FighterStatus
    {
        None, Launched, Unmanned
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
        }
    }
}