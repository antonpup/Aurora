using Aurora.Settings.Layers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Settings {

    public partial class Control_LayerList : UserControl, INotifyPropertyChanged {
        
        public Control_LayerList() {
            InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        #region PropertyChanged Event (and helpers)
        /// <summary>Event that fires when a property changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets a field to a specific value and raises change events for the property and all additional properties if the value is different.
        /// </summary>
        private void SetFieldNotify<T>(ref T field, T val, string[] additionalProperties = null, [CallerMemberName] string propName = null) {
            if (field.Equals(val)) return;
            field = val;
            Notify(additionalProperties, propName);
        }

        /// <summary>
        /// Sets a dependency property to a specific value and raises change events for the property and all additional properties if the value is different.
        /// </summary>
        private void SetValueNotify(DependencyProperty dp, object val, string[] additionalProperties = null, [CallerMemberName] string propName = null) {
            if (GetValue(dp).Equals(val)) return;
            SetValue(dp, val);
            Notify(additionalProperties, propName);
        }

        /// <summary>
        /// Raises change events for the property and all additional properties.
        /// </summary>
        private void Notify(string[] additionalProperties, string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            Notify(additionalProperties);
        }

        /// <summary>
        /// Raises change events for the properties.
        /// </summary>
        /// <param name="properties"></param>
        private void Notify(params string[] properties) {
            foreach (var prop in properties)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Helper funtion that creates a PropertyChangedCallback for PropertyMetadata that notifies the provided property names have changed.
        /// </summary>
        private static PropertyChangedCallback NotifyCb(params string[] properties) {
            return (trg, e) => ((Control_LayerList)trg).Notify(properties);
        }
        #endregion

        #region LayerCollection Property
        /// <summary>
        /// This collection is a primary set of layers to display to the user.
        /// </summary>
        public ObservableCollection<Layer> LayerCollection {
            get => (ObservableCollection<Layer>)GetValue(LayerCollectionProperty);
            set => SetValue(LayerCollectionProperty, value);
        }
        
        public static readonly DependencyProperty LayerCollectionProperty =
            DependencyProperty.Register("LayerCollection", typeof(ObservableCollection<Layer>), typeof(Control_LayerList), new PropertyMetadata(null, NotifyCb("ActiveLayerCollection")));
        #endregion

        #region NighttimeLayerCollection Property
        /// <summary>
        /// This collection of layers is a secondary set of layers to display to the user. If this is given a value and the relevant setting is enabled, a pair of checkboxs
        /// will show that allow the user to choose between day and night time layers. This collection is the nighttime collection.
        /// </summary>
        public ObservableCollection<Layer> NighttimeLayerCollection {
            get => (ObservableCollection<Layer>)GetValue(NighttimeLayerCollectionProperty);
            set => SetValueNotify(NighttimeLayerCollectionProperty, value, new[] { "ActiveLayerCollection" });
        }

        public static readonly DependencyProperty NighttimeLayerCollectionProperty =
            DependencyProperty.Register("NighttimeLayerCollection", typeof(ObservableCollection<Layer>), typeof(Control_LayerList), new PropertyMetadata(null, NotifyCb("ActiveLayerCollection")));
        #endregion

        #region ActiveLayerCollection Property
        /// <summary>
        /// This returns the currently selected layer collection.
        /// </summary>
        public ObservableCollection<Layer> ActiveLayerCollection => LayerCollection;
        #endregion

        #region SelectedLayer Property
        /// <summary>
        /// The currently selected layer.
        /// </summary>
        public Layer SelectedLayer {
            get => (Layer)GetValue(SelectedLayerProperty);
            set => SetValue(SelectedLayerProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedLayer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLayerProperty =
            DependencyProperty.Register("SelectedLayer", typeof(Layer), typeof(Control_LayerList), new PropertyMetadata(null));
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e) {
        }
    }
}
