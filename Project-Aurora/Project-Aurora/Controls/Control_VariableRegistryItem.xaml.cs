using Aurora.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register("VariableName", typeof(string), typeof(Control_VariableRegistryItem), new PropertyMetadata(new PropertyChangedCallback(VariableNameChanged)));

        private static void VariableNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(e.OldValue))
                return;

            Control_VariableRegistryItem self = (Control_VariableRegistryItem)d;
            if (self.IsLoaded)
                self.UpdateControls();
        }

        public string VariableName
        {
            get
            {
                return (string)GetValue(VariableNameProperty);
            }
            set
            {
                SetValue(VariableNameProperty, value);

            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty VarRegistryProperty = DependencyProperty.Register("VarRegistry", typeof(VariableRegistry), typeof(Control_VariableRegistryItem));

        public VariableRegistry VarRegistry
        {
            get
            {
                return (VariableRegistry)GetValue(VarRegistryProperty);
            }
            set
            {
                SetValue(VarRegistryProperty, value);

                if (this.IsLoaded)
                    UpdateControls();
            }
        }

        public Control_VariableRegistryItem()
        {
            InitializeComponent();
            this.Loaded += (sender, e) => { this.UpdateControls(); };
        }

        private void UpdateControls()
        {
            string var_title = VarRegistry.GetTitle(VariableName);

            if (String.IsNullOrWhiteSpace(var_title))
                this.txtBlk_name.Text = VariableName;
            else
                this.txtBlk_name.Text = var_title;

            string var_remark = VarRegistry.GetRemark(VariableName);

            if (String.IsNullOrWhiteSpace(var_remark))
                this.txtBlk_remark.Visibility = Visibility.Collapsed;
            else
                this.txtBlk_remark.Text = var_remark;

            //Create a control here...
            Type var_type = VarRegistry.GetVariableType(VariableName);

            grd_control.Children.Clear();

            if (var_type == typeof(bool))
            {
                CheckBox chkbx_control = new CheckBox();
                chkbx_control.Content = "";
                chkbx_control.IsChecked = VarRegistry.GetVariable<bool>(VariableName);
                chkbx_control.Checked += Chkbx_control_VarChanged;
                chkbx_control.Unchecked += Chkbx_control_VarChanged;

                grd_control.Children.Add(chkbx_control);
            }
            else if (var_type == typeof(string))
            {
                TextBox txtbx_control = new TextBox();
                txtbx_control.Text = VarRegistry.GetVariable<string>(VariableName);
                txtbx_control.TextChanged += Txtbx_control_TextChanged;

                grd_control.Children.Add(txtbx_control);
            }
            else if (var_type == typeof(int))
            {
                if (VarRegistry.GetFlags(VariableName).HasFlag(VariableFlags.UseHEX))
                {
                    TextBox hexBox = new TextBox();
                    hexBox.PreviewTextInput += HexBoxOnPreviewTextInput;
                    hexBox.Text = string.Format("{0:X}", VarRegistry.GetVariable<int>(VariableName));
                    hexBox.TextChanged += HexBox_TextChanged;
                    grd_control.Children.Add(hexBox);
                }
                else
                {
                    IntegerUpDown intUpDown_control = new IntegerUpDown();

                    intUpDown_control.Value = VarRegistry.GetVariable<int>(VariableName);
                    int max_val, min_val = 0;
                    if (VarRegistry.GetVariableMax<int>(VariableName, out max_val))
                        intUpDown_control.Maximum = max_val;
                    if (VarRegistry.GetVariableMin<int>(VariableName, out min_val))
                        intUpDown_control.Minimum = min_val;

                    intUpDown_control.ValueChanged += IntUpDown_control_ValueChanged;

                    grd_control.Children.Add(intUpDown_control);
                }
            }
            else if (var_type == typeof(long))
            {
                LongUpDown longUpDown_control = new LongUpDown();
                longUpDown_control.Value = VarRegistry.GetVariable<long>(VariableName);
                long max_val, min_val = 0;
                if (VarRegistry.GetVariableMax<long>(VariableName, out max_val))
                    longUpDown_control.Maximum = max_val;
                if (VarRegistry.GetVariableMin<long>(VariableName, out min_val))
                    longUpDown_control.Minimum = min_val;

                longUpDown_control.ValueChanged += LongUpDown_control_ValueChanged;

                grd_control.Children.Add(longUpDown_control);
            }
            else if (var_type == typeof(double))
            {
                DoubleUpDown doubleUpDown_control = new DoubleUpDown();
                doubleUpDown_control.Value = VarRegistry.GetVariable<double>(VariableName);
                if (VarRegistry.GetVariableMax<double>(VariableName, out double max_val))
                    doubleUpDown_control.Maximum = max_val;
                if (VarRegistry.GetVariableMax<double>(VariableName, out double min_val))
                    doubleUpDown_control.Minimum = min_val;
                doubleUpDown_control.ValueChanged += DoubleUpDown_control_ValueChanged;
                grd_control.Children.Add(doubleUpDown_control);
            }
            else if (var_type == typeof(float))
            {
                DoubleUpDown doubleUpDown_control = new DoubleUpDown();
                doubleUpDown_control.Value = VarRegistry.GetVariable<float>(VariableName);
                if (VarRegistry.GetVariableMax<float>(VariableName, out float max_val))
                    doubleUpDown_control.Maximum = max_val;
                if (VarRegistry.GetVariableMax<float>(VariableName, out float min_val))
                    doubleUpDown_control.Minimum = min_val;
                doubleUpDown_control.ValueChanged += DoubleUpDown_control_ValueChanged;
                grd_control.Children.Add(doubleUpDown_control);
            }
            else if (var_type == typeof(Aurora.Settings.KeySequence))
            {
                Aurora.Controls.KeySequence ctrl = new Aurora.Controls.KeySequence();
                ctrl.RecordingTag = var_title;

                ctrl.Sequence = VarRegistry.GetVariable<Aurora.Settings.KeySequence>(VariableName);
                ctrl.SequenceUpdated += keySequenceControlValueChanged;

                grd_control.Children.Add(ctrl);
            }
            else if (var_type == typeof(Aurora.Utils.RealColor))
            {
                ColorPicker ctrl = new ColorPicker();
                ctrl.ColorMode = ColorMode.ColorCanvas;
                Aurora.Utils.RealColor clr = VarRegistry.GetVariable<Aurora.Utils.RealColor>(VariableName);

                ctrl.SelectedColor = clr.GetMediaColor();
                ctrl.SelectedColorChanged += colorPickerControlValueChanged;
                grd_control.Children.Add(ctrl);
            }
            else if (var_type == typeof(Aurora.Devices.DeviceKeys))
            {
                ComboBox ctrl = new ComboBox();
                ctrl.ItemsSource = Enum.GetValues(typeof(Devices.DeviceKeys)).Cast<Devices.DeviceKeys>().ToList();
                ctrl.SelectedValue = VarRegistry.GetVariable<Aurora.Devices.DeviceKeys>(VariableName);
                ctrl.SelectionChanged += CmbbxEnum_control_SelectionChanged;

                grd_control.Children.Add(ctrl);
            }
            else if (var_type.IsEnum)
            {
                ComboBox cmbbxEnum_control = new ComboBox();
                cmbbxEnum_control.ItemsSource = Enum.GetValues(var_type);
                cmbbxEnum_control.SelectedValue = VarRegistry.GetVariable<object>(VariableName);
                cmbbxEnum_control.SelectionChanged += CmbbxEnum_control_SelectionChanged;

                grd_control.Children.Add(cmbbxEnum_control);
            }
            //else
            //throw new Exception($"Type {var_type} is not supported!");

            grd_control.UpdateLayout();
        }

        private void CmbbxEnum_control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VarRegistry.SetVariable(VariableName, (sender as ComboBox).SelectedItem);
        }

        private void colorPickerControlValueChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Type var_type = VarRegistry.GetVariableType(VariableName);
            System.Windows.Media.Color ctrlClr = (System.Windows.Media.Color)((ColorPicker)sender).SelectedColor;
            Aurora.Utils.RealColor clr = VarRegistry.GetVariable<Aurora.Utils.RealColor>(VariableName);
            clr.SetMediaColor(ctrlClr);
            VarRegistry.SetVariable(VariableName, clr);
        }

        private void keySequenceControlValueChanged(object sender, EventArgs e)
        {
            VarRegistry.SetVariable(VariableName, ((Aurora.Controls.KeySequence)sender).Sequence);
        }

        private void LongUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VarRegistry.SetVariable(VariableName, (sender as LongUpDown).Value);
        }

        private void IntUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VarRegistry.SetVariable(VariableName, (sender as IntegerUpDown).Value);
        }

        private void DoubleUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            VarRegistry.SetVariable(VariableName, (sender as DoubleUpDown).Value);
        }

        private void Txtbx_control_TextChanged(object sender, TextChangedEventArgs e)
        {
            VarRegistry.SetVariable(VariableName, (sender as TextBox).Text);
        }

        private void HexBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int number);
        }

        private void HexBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            //Hacky fix to stop error when nothing is entered
            if (string.IsNullOrWhiteSpace(text))
                text = "0";
            VarRegistry.SetVariable(VariableName, int.Parse(text, NumberStyles.HexNumber, CultureInfo.CurrentCulture));
        }

        private void Chkbx_control_VarChanged(object sender, RoutedEventArgs e)
        {
            VarRegistry.SetVariable(VariableName, (sender as CheckBox).IsChecked.Value);
        }

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            VarRegistry.ResetVariable(VariableName);

            UpdateControls();
        }
    }
}
