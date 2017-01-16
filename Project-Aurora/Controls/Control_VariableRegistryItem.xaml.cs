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
using Xceed.Wpf.Toolkit;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_VariableRegistryItem.xaml
    /// </summary>
    public partial class Control_VariableRegistryItem : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register("VariableName", typeof(string), typeof(Control_VariableRegistryItem));

        public string VariableName
        {
            get
            {
                return (string)GetValue(VariableNameProperty);
            }
            set
            {
                SetValue(VariableNameProperty, value);

                UpdateControls();
            }
        }

        private void UpdateControls()
        {
            string var_title = Global.Configuration.VarRegistry.GetTitle(VariableName);

            if (String.IsNullOrWhiteSpace(var_title))
                this.txtBlk_name.Text = VariableName;
            else
                this.txtBlk_name.Text = var_title;

            string var_remark = Global.Configuration.VarRegistry.GetRemark(VariableName);

            if (String.IsNullOrWhiteSpace(var_remark))
                this.txtBlk_remark.Visibility = Visibility.Collapsed;
            else
                this.txtBlk_remark.Text = var_remark;

            //Create a control here...
            Type var_type = Global.Configuration.VarRegistry.GetVariableType(VariableName);

            grd_control.Children.Clear();

            if (var_type == typeof(bool))
            {
                CheckBox chkbx_control = new CheckBox();
                chkbx_control.Content = "";
                chkbx_control.IsChecked = Global.Configuration.VarRegistry.GetVariable<bool>(VariableName);
                chkbx_control.Checked += Chkbx_control_VarChanged;
                chkbx_control.Unchecked += Chkbx_control_VarChanged;

                grd_control.Children.Add(chkbx_control);
            }
            else if (var_type == typeof(string))
            {
                TextBox txtbx_control = new TextBox();
                txtbx_control.Text = Global.Configuration.VarRegistry.GetVariable<string>(VariableName);
                txtbx_control.TextChanged += Txtbx_control_TextChanged;

                grd_control.Children.Add(txtbx_control);
            }
            else if (var_type == typeof(int))
            {
                IntegerUpDown intUpDown_control = new IntegerUpDown();
                intUpDown_control.Value = Global.Configuration.VarRegistry.GetVariable<int>(VariableName);
                int max_val, min_val = 0;
                if (Global.Configuration.VarRegistry.GetVariableMax<int>(VariableName, out max_val))
                    intUpDown_control.Maximum = max_val;
                if (Global.Configuration.VarRegistry.GetVariableMin<int>(VariableName, out min_val))
                    intUpDown_control.Minimum = min_val;

                intUpDown_control.ValueChanged += IntUpDown_control_ValueChanged;

                grd_control.Children.Add(intUpDown_control);
            }
            else if (var_type == typeof(long))
            {
                LongUpDown longUpDown_control = new LongUpDown();
                longUpDown_control.Value = Global.Configuration.VarRegistry.GetVariable<long>(VariableName);
                long max_val, min_val = 0;
                if (Global.Configuration.VarRegistry.GetVariableMax<long>(VariableName, out max_val))
                    longUpDown_control.Maximum = max_val;
                if (Global.Configuration.VarRegistry.GetVariableMin<long>(VariableName, out min_val))
                    longUpDown_control.Minimum = min_val;

                longUpDown_control.ValueChanged += LongUpDown_control_ValueChanged; ;

                grd_control.Children.Add(longUpDown_control);
            }

            grd_control.UpdateLayout();
        }

        private void LongUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Global.Configuration.VarRegistry.SetVariable(VariableName, (sender as LongUpDown).Value);
        }

        private void IntUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Global.Configuration.VarRegistry.SetVariable(VariableName, (sender as IntegerUpDown).Value);
        }

        private void Txtbx_control_TextChanged(object sender, TextChangedEventArgs e)
        {
            Global.Configuration.VarRegistry.SetVariable(VariableName, (sender as TextBox).Text);
        }

        private void Chkbx_control_VarChanged(object sender, RoutedEventArgs e)
        {
            Global.Configuration.VarRegistry.SetVariable(VariableName, (sender as CheckBox).IsChecked.Value);
        }

        public Control_VariableRegistryItem()
        {
            InitializeComponent();
        }

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            Global.Configuration.VarRegistry.ResetVariable(VariableName);

            UpdateControls();
        }
    }
}
