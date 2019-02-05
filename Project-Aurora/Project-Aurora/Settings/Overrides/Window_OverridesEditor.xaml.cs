using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings.Overrides {
    /// <summary>
    /// Interaction logic for Window_OverridesEditor.xaml
    /// </summary>
    public partial class Window_OverridesEditor : Window {
        
        public Window_OverridesEditor(Layer layer) {
            // Store the layer and ensure it has an override logic assigned to it.
            Layer = layer;
            if (Layer.OverrideLogic == null)
                Layer.OverrideLogic = new Dictionary<string, OverrideLogic>();

            // Setup UI and databinding stuff
            InitializeComponent();
            OverridablePropList.ItemsSource = GetAllOverridableProperties(layer);
            DataContext = this;
        }

        /// <summary>
        /// Creates a new lookup entry and adds it to the currently selected lookup.
        /// </summary>
        private void AddNewLookup_Click(object sender, RoutedEventArgs e) {
            SelectedLogic?.CreateNewLookup();
        }

        /// <summary>
        /// Deletes an entry in the lookup table based on the DataContext of the clicked button.
        /// </summary>
        private void DeleteLookupEntry_Click(object sender, RoutedEventArgs e) {
            var dc = (OverrideLogic.LookupTableEntry)((Button)sender).DataContext;
            SelectedLogic.LookupTable.Remove(dc);
        }

        /// <summary>
        /// For the given layer, returns a list of all properties on the handler of that layer that have the OverridableAttribute 
        /// applied (i.e. have been marked overridable for the overrides system).
        /// </summary>
        private List<Tuple<string, string, Type>> GetAllOverridableProperties (Layer layer) {
            return layer.Handler.Properties.GetType().GetProperties() // Get all properties on the layer handler's property list
                .Where(prop => prop.GetCustomAttributes(typeof(LogicOverridableAttribute), true).Length > 0) // Filter to only return the PropertyInfos that have Overridable
                .Select(prop => new Tuple<string, string, Type>( // Return the name and type of these properties.
                    prop.Name,
                    ((LogicOverridableAttribute)prop.GetCustomAttributes(typeof(LogicOverridableAttribute), true)[0]).Name, // Get the name specified in the attribute (so it is prettier for the user)
                    Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType // If the property is a nullable type (e.g. bool?), will instead return the non-nullable type (bool)
                ))
                .ToList();
        }
    }

    /// <summary>
    /// State properties for the Window_OverridesEditor class.
    /// </summary>
    public partial class Window_OverridesEditor : INotifyPropertyChanged {

        // Property change event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(params string[] affectedProperties) {
            foreach (var prop in affectedProperties)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        // The layer being edited by the window
        public Layer Layer { get; }

        // The name of the selected property that is being edited
        private Tuple<string, string, Type> _selectedProperty;
        public Tuple<string, string, Type> SelectedProperty {
            get => _selectedProperty;
            set {
                _selectedProperty = value;
                OnPropertyChanged("SelectedProperty", "SelectedLogic");
            }
        }

        // The override logic for the currently selected property
        public OverrideLogic SelectedLogic {
            get {
                if (_selectedProperty == null) // Return nothing if nothing in the list is selected
                    return null;
                if (!Layer.OverrideLogic.ContainsKey(_selectedProperty.Item1)) // Create a new logic for this property if it doesn't already exist
                    Layer.OverrideLogic[_selectedProperty.Item1] = new OverrideLogic(_selectedProperty.Item3);
                return Layer.OverrideLogic[_selectedProperty.Item1];
            }
        }
    }



    /// <summary>
    /// Simple converter to convert a type to it's name (instead of using ToString as that gives the fully qualified name).
    /// </summary>
    public class PrettyTypeNameConverter : System.Windows.Data.IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((Type)value).Name;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

}
