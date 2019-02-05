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
            foreach (var entry in LookupTable)
                if (entry.Condition.Evaluate(gameState))
                    return entry.Value;
            return null;
        }

        /// <summary>Returns whether or not the logic is "empty", I.E. if nothing will happen when evaluated and can be deleted.</summary>
        [JsonIgnore]
        public bool IsEmpty => LookupTable.Count == 0;


        /// <summary>
        /// Represents a single entry in a LookupTable.
        /// </summary>
        public class LookupTableEntry {
            // The reason for applying a custom converter to this property is so that it is able to handle structs correctly.
            // Structs (atleast in the case of System.Drawing.Color) are simply converted to a string and so when being deserialized
            // into an object field, Newtonsoft JSON converter does not know what kind of struct to parse it as and so it is string
            // type instead of an instance of the struct. The custom converter forcefully wraps a $type parameter around the JSON of
            // the struct so that it can always know the correct class/struct to deserialize the JSON as.
            /// <summary>The value of this entry.</summary>
            [JsonConverter(typeof(Utils.TypeAnnotatedObjectConverter))]
            public object Value { get; set; }

            /// <summary>A boolean condition that should be met for this entry to be valid.</summary>
            public ICondition Condition { get; set; }
            
            public LookupTableEntry(object value, ICondition condition) {
                Value = value;
                Condition = condition;
            }
        }
    }
}