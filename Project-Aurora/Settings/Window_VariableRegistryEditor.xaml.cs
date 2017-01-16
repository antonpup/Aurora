using Aurora.Controls;
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

namespace Aurora.Settings
{
    /// <summary>
    /// Interaction logic for Window_VariableRegistryEditor.xaml
    /// </summary>
    public partial class Window_VariableRegistryEditor : Window
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty RegisteredVariablesProperty = DependencyProperty.Register("RegisteredVariables", typeof(VariableRegistry), typeof(Window_VariableRegistryEditor));

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

        private void UpdateControls()
        {
            stack_Options.Children.Clear();

            foreach (var variablename in RegisteredVariables.GetRegisteredVariableKeys())
            {
                Control_VariableRegistryItem varItem = new Control_VariableRegistryItem();
                varItem.VariableName = variablename;

                stack_Options.Children.Add(varItem);
                stack_Options.Children.Add(new Separator() { Height = 5, Opacity = 0 });
            }
        }

        public Window_VariableRegistryEditor()
        {
            InitializeComponent();
        }
    }
}
