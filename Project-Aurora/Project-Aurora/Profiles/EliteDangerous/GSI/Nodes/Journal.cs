using Aurora.Profiles.EliteDangerous.Journal;
using Aurora.Profiles.EliteDangerous.Journal.Events;

namespace Aurora.Profiles.EliteDangerous.GSI.Nodes
{
    public enum FighterStatus
    {
        None, Launched, Unmanned
    }
    
    public class Journal : Node<Controls>
    {
        public FighterStatus fighterStatus = FighterStatus.None;
        public bool hasChaff;
        public bool hasHeatSink;
        public bool hasShieldCellBank;
        
        private void SetModulesFromLoadout(Loadout loadout) {
            bool hasChaff = false;
            bool hasHeatSink = false;
            bool hasShieldCellBank = false;
            
            foreach(LoadoutModule module in loadout.Modules) {
                if(module.Item.StartsWith("Hpt_ChaffLauncher_")) {
                    hasChaff = true;
                } else if(module.Item.StartsWith("Hpt_HeatSinkLauncher_")) {
                    hasHeatSink = true;
                } else if(module.Item.StartsWith("Int_ShieldCellBank_")) {
                    hasShieldCellBank = true;
                }
            }

            this.hasChaff = hasChaff;
            this.hasHeatSink = hasHeatSink;
            this.hasShieldCellBank = hasShieldCellBank;
        }

        public void ProcessEvent(JournalEvent journalEvent)
        {
            switch (journalEvent.@event) {
                case EventType.LaunchFighter:
                    fighterStatus = FighterStatus.Launched;
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