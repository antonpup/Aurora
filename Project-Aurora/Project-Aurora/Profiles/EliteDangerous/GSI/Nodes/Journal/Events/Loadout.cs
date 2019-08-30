namespace Aurora.Profiles.EliteDangerous.Journal.Events
{
    public class LoadoutModuleEngineeringModifier {
        public string Label;
        public double Value;
        public double OriginalValue;
        public int LessIsGood;
    }
    public class LoadoutModuleEngineering {
        public string Engineer;
        public long EngineerID;
        public long BlueprintID;
        public string BlueprintName;
        public int Level;
        public double Quality;
        public LoadoutModuleEngineeringModifier[] Modifiers;
    }
    public class LoadoutModule {
        public string Slot;
        public string Item;
        public bool On;
        public int Priority;
        public double Health;
        public long Value;
        public LoadoutModuleEngineering Engineering;
    }
    public class Loadout : JournalEvent
    {
        public string Ship;
        public int ShipID;
        public string ShipName;
        public string ShipIdent;
        public long HullValue;
        public long ModulesValue;
        public double HullHealth;
        public long Rebuy;
        public LoadoutModule[] Modules;
    }
}