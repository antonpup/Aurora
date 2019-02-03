using Aurora.Profiles;
using Aurora.Settings.Conditions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace Aurora.Settings.Layers {

    public class OverrideLogic {

        public Type VarType { get; set; }
        public ObservableCollection<LookupTableEntry> LookupTable { get; set; }

        public OverrideLogic(Type type, ObservableCollection<LookupTableEntry> lookupTable = null) {
            VarType = type;
            LookupTable = lookupTable ?? new ObservableCollection<LookupTableEntry>();
        }

        /// <summary>
        /// Adds a new entry to the LookupTable based on the current VarType
        /// </summary>
        public void CreateNewLookup() {
            LookupTable.Add(new LookupTableEntry(Activator.CreateInstance(VarType), new ConditionTrue()));
        }

        /// <summary>
        /// Evalutes this logic and returns the value of the first lookup which has a truthy condition.
        /// Will return `null` if there are no true conditions.
        /// </summary>
        public object Evaluate(IGameState gameState) {
            foreach (var tup in LookupTable)
                if (tup.Condition.Evaluate(gameState))
                    return tup.Value;
            return null;
        }

        /// <summary>Returns whether or not the logic is "empty", I.E. if nothing will happen when evaluated and can be deleted.</summary>
        [JsonIgnore]
        public bool IsEmpty => LookupTable.Count == 0;


        public class LookupTableEntry {
            public object Value { get; set; }
            public ICondition Condition { get; set; }
            public LookupTableEntry(object value, ICondition condition) {
                Value = value;
                Condition = condition;
            }
        }
    }
}