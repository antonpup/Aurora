using Aurora.Settings.Layers;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings {
    /// <summary>
    /// Interaction logic for Window_OverridesEditor.xaml
    /// </summary>
    public partial class Window_OverridesEditor : Window {
        
        public Window_OverridesEditor(OverrideLogic overrideLogic = null) {
            InitializeComponent();
            OverrideLogic = Test;// overrideLogic;
        }

        private OverrideLogic overrideLogic;
        public OverrideLogic OverrideLogic {
            get => overrideLogic;
            set => DataContext = overrideLogic = value;
        }

        private void AddNewLookup_Click(object sender, RoutedEventArgs e) {
            overrideLogic?.CreateNewLookup();
        }

        public static OverrideLogic Test { get; } = new OverrideLogic(typeof(Color), new ObservableCollection<OverrideLogic.LookupTableEntry> {
                new OverrideLogic.LookupTableEntry(Color.Red, new Conditions.ConditionGSINumeric { Operand1Path = "LocalPCInfo/CurrentSecond", Operator = Conditions.ComparisonOperator.LT, Operand2Path = "30" }),
                new OverrideLogic.LookupTableEntry(Color.Blue, new Conditions.ConditionGSINumeric { Operand1Path = "LocalPCInfo/CurrentSecond", Operator = Conditions.ComparisonOperator.GT, Operand2Path = "45" })
            });
            /*VarType = typeof(float),
            LookupTable = new ObservableCollection<OverrideLogic.LookupTableEntry> {
                new OverrideLogic.LookupTableEntry(1f, new Conditions.ConditionGSINumeric { Operand1Path = "LocalPCInfo/CurrentSecond", Operator = Conditions.ComparisonOperator.LT, Operand2Path = "30" }),
                new OverrideLogic.LookupTableEntry(10f, new Conditions.ConditionTrue()),
            }*/

        private void DeleteLookupEntry_Click(object sender, RoutedEventArgs e) {
            var dc = (OverrideLogic.LookupTableEntry)((Button)sender).DataContext;
            OverrideLogic.LookupTable.Remove(dc);
        }
    }
}
