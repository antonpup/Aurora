using Aurora.Controls;
using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Window_VariableRegistryEditor.xaml
    /// </summary>
    public partial class Control_VariableRegistryEditor : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty RegisteredVariablesProperty = DependencyProperty.Register("RegisteredVariables", typeof(VariableRegistry), typeof(Control_VariableRegistryEditor));

        public VariableRegistry RegisteredVariables
        {
            get
            {
                return (VariableRegistry)GetValue(RegisteredVariablesProperty);
            }
            set
            {
                SetValue(RegisteredVariablesProperty, value);
                UpdateControls();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty VarRegistrySourceProperty = DependencyProperty.Register("VarRegistrySource", typeof(VariableRegistry), typeof(Control_VariableRegistryEditor), new PropertyMetadata { PropertyChangedCallback = new PropertyChangedCallback(VarRegistrySourceChanged) });

        public VariableRegistry VarRegistrySource
        {
            get
            {
                return GetValue(VarRegistrySourceProperty) as VariableRegistry ?? Global.Configuration.VarRegistry;
            }
            set
            {
                SetValue(VarRegistrySourceProperty, value);
            }
        }

        private void UpdateControls()
        {
            stack_Options.Children.Clear();
            if (RegisteredVariables == null)
                return;

            foreach (var variablename in RegisteredVariables.GetRegisteredVariableKeys())
            {
                Control_VariableRegistryItem varItem = new Control_VariableRegistryItem {
                    VariableName = variablename,
                    VarRegistry = VarRegistrySource,
                    //HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    //VerticalContentAlignment = VerticalAlignment.Stretch
                };
                

                stack_Options.Children.Add(varItem);
                stack_Options.Children.Add(new Separator() { Height = 5, Opacity = 0 });
            }
        }

        private static void VarRegistrySourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Control_VariableRegistryEditor self = (Control_VariableRegistryEditor)obj;

            if (e.OldValue == e.NewValue || self.stack_Options.Children.Count == 0)
                return;

            VariableRegistry newRegistry = (VariableRegistry)e.NewValue;
            foreach (UIElement child in self.stack_Options.Children)
            {
                if (child is Control_VariableRegistryItem)
                    ((Control_VariableRegistryItem)child).VarRegistry = newRegistry;
            }
        }

        public Control_VariableRegistryEditor()
        {
            InitializeComponent();
        }
    }
}
