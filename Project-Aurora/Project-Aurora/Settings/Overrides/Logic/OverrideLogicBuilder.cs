using System;
using System.Collections.Generic;

namespace Aurora.Settings.Overrides.Logic.Builder {

    /// <summary>
    /// This class helps to build override logic instead of needing to manually
    /// create and instantiate the various logic evaluatables.
    /// </summary>
    public class OverrideLogicBuilder {

        private Dictionary<string, IOverrideLogic> overrideProperties = new Dictionary<string, IOverrideLogic>();

        /// <summary>
        /// Creates a new override logic builder.
        /// </summary>
        public OverrideLogicBuilder() { }

        /// <summary>
        /// Returns the created override property table for using with a layer's _OverrideLogic property.
        /// </summary>
        public Dictionary<string, IOverrideLogic> Create() => overrideProperties;

        #region Dynamic Value Methods
        /// <summary>
        /// Interal helper function to simplify/shorten the public API methods.
        /// </summary>
        private OverrideLogicBuilder SetDynamic(Type type, string layerPropertyName, IEvaluatable evaluatable) {
            overrideProperties[layerPropertyName] = new OverrideDynamicValue(type, new Dictionary<string, IEvaluatable> { { "Value", evaluatable } });
            return this;
        }

        /// <summary>Creates/replaces the override entry for the given boolean layer property with the given evaluatable boolean.</summary>
        /// <param name="layerPropertyName">The name of a bool layer property (e.g. "_Enabled").</param>
        /// <param name="evaluatable">The evaluatable boolean that will be used for this property.</param>
        public OverrideLogicBuilder SetDynamicBoolean(string layerPropertyName, Evaluatable<bool> evaluatable) => SetDynamic(typeof(bool), layerPropertyName, evaluatable);

        /// <summary>Creates/replaces the override entry for the given integer layer property with the given evaluatable number.</summary>
        /// <param name="layerPropertyName">The name of an int layer property (e.g. "_EffectWidth").</param>
        /// <param name="evaluatable">The evaluatable number that will be used for this property.</param>
        public OverrideLogicBuilder SetDynamicInt(string layerPropertyName, Evaluatable<double> evaluatable) => SetDynamic(typeof(int), layerPropertyName, evaluatable);

        /// <summary>Creates/replaces the override entry for the given long integer layer property with the given evaluatable number.</summary>
        /// <param name="layerPropertyName">The name of an long layer property.</param>
        /// <param name="evaluatable">The evaluatable number that will be used for this property.</param>
        public OverrideLogicBuilder SetDynamicLong(string layerPropertyName, Evaluatable<double> evaluatable) => SetDynamic(typeof(long), layerPropertyName, evaluatable);

        /// <summary>Creates/replaces the override entry for the given floating-point number layer property with the given evaluatable number.</summary>
        /// <param name="layerPropertyName">The name of an float layer property (e.g. "_Opacity").</param>
        /// <param name="evaluatable">The evaluatable number that will be used for this property.</param>
        public OverrideLogicBuilder SetDynamicFloat(string layerPropertyName, Evaluatable<double> evaluatable) => SetDynamic(typeof(float), layerPropertyName, evaluatable);

        /// <summary>Creates/replaces the override entry for the given double-precision number layer property with the given evaluatable number.</summary>
        /// <param name="layerPropertyName">The name of an double layer property.</param>
        /// <param name="evaluatable">The evaluatable number that will be used for this property.</param>
        public OverrideLogicBuilder SetDynamicDouble(string layerPropertyName, Evaluatable<double> evaluatable) => SetDynamic(typeof(double), layerPropertyName, evaluatable);

        /// <summary>Creates/replaces the override entry for the given color layer property with the given evaluatable numbers.</summary>
        /// <param name="layerPropertyName">The name of an color layer property (e.g. "_PrimaryColor").</param>
        /// <param name="a">The evaluatable number that will be used as the alpha property for this color.</param>
        /// <param name="r">The evaluatable number that will be used as the red property for this color.</param>
        /// <param name="g">The evaluatable number that will be used as the green property for this color.</param>
        /// <param name="b">The evaluatable number that will be used as the blue property for this color.</param>
        public OverrideLogicBuilder SetDynamicColor(string layerPropertyName, Evaluatable<double> a, Evaluatable<double>r, Evaluatable<double>g, Evaluatable<double>b) {
            overrideProperties[layerPropertyName] = new OverrideDynamicValue(typeof(System.Drawing.Color), new Dictionary<string, IEvaluatable> {
                { "Alpha", a }, { "Red", r }, { "Green", g }, { "Blue", b }
            });
            return this;
        }

        /// <summary>Creates/replaces the override entry for the given color layer property with the given evaluatable numbers.</summary>
        /// <param name="layerPropertyName">The name of an color layer property (e.g. "_PrimaryColor").</param>
        /// <param name="r">The evaluatable number that will be used as the red property for this color.</param>
        /// <param name="g">The evaluatable number that will be used as the green property for this color.</param>
        /// <param name="b">The evaluatable number that will be used as the blue property for this color.</param>
        public OverrideLogicBuilder SetDynamicColor(string layerPropertyName, Evaluatable<double>r, Evaluatable<double>g, Evaluatable<double>b) => SetDynamicColor(layerPropertyName, new NumberConstant { Value = 1 }, r, g, b);
        #endregion

        #region Lookup Table Methods
        public OverrideLogicBuilder SetLookupTable(string layerPropertyName, Type t, IEnumerable<OverrideLookupTable.LookupTableEntry> entries) {
            overrideProperties[layerPropertyName] = new OverrideLookupTable(t, entries);
            return this;
        }

        public OverrideLogicBuilder SetLookupTable<T>(string layerPropertyName, OverrideLookupTableBuilder<T> entryBuilder) => SetLookupTable(layerPropertyName, typeof(T), entryBuilder.Create());
        #endregion
    }

    /// <summary>
    /// Builder to help construct an entry set for the OverrideLookupTable.
    /// </summary>
    /// <typeparam name="T">The type of variable expected for this table.</typeparam>
    public class OverrideLookupTableBuilder<T> {

        private List<OverrideLookupTable.LookupTableEntry> entries = new List<OverrideLookupTable.LookupTableEntry>();

        /// <summary>Adds an entry to the table with the given value that will be used as an override when the `when` condition is true.</summary>
        /// <param name="value">The value to override a layer property with.</param>
        /// <param name="when">The condition that must be met for the override property to be used.</param>
        public OverrideLookupTableBuilder<T> AddEntry(T value, Evaluatable<bool> when) {
            entries.Add(new OverrideLookupTable.LookupTableEntry(value, when));
            return this;
        }

        public List<OverrideLookupTable.LookupTableEntry> Create() => entries;
    }
}
