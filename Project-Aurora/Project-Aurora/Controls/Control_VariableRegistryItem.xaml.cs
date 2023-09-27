using Aurora.Settings;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Common.Devices;
using Xceed.Wpf.Toolkit;

namespace Aurora.Controls;

/// <summary>
/// Interaction logic for Control_VariableRegistryItem.xaml
/// </summary>
public partial class Control_VariableRegistryItem
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register(nameof(VariableName), typeof(string), typeof(Control_VariableRegistryItem), new PropertyMetadata(VariableNameChanged));

    private static void VariableNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue.Equals(e.OldValue))
            return;

        var self = (Control_VariableRegistryItem)d;
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
    public static readonly DependencyProperty VarRegistryProperty = DependencyProperty.Register(nameof(VarRegistry), typeof(VariableRegistry), typeof(Control_VariableRegistryItem));

    public VariableRegistry VarRegistry
    {
        get => (VariableRegistry)GetValue(VarRegistryProperty);
        set
        {
            SetValue(VarRegistryProperty, value);

            if (IsLoaded)
                UpdateControls();
        }
    }

    public Control_VariableRegistryItem()
    {
        InitializeComponent();
        Loaded += (sender, e) => { UpdateControls(); };
    }

    private void UpdateControls()
    {
        var varTitle = VarRegistry.GetTitle(VariableName);

        if (string.IsNullOrWhiteSpace(varTitle))
            txtBlk_name.Text = VariableName;
        else
            txtBlk_name.Text = varTitle;

        var varRemark = VarRegistry.GetRemark(VariableName);

        if (string.IsNullOrWhiteSpace(varRemark))
            txtBlk_remark.Visibility = Visibility.Collapsed;
        else
            txtBlk_remark.Text = varRemark;

        //Create a control here...
        var varType = VarRegistry.GetVariableType(VariableName);

        grd_control.Children.Clear();

        if (varType == typeof(bool))
        {
            var chkbxControl = new CheckBox();
            chkbxControl.Content = "";
            chkbxControl.IsChecked = VarRegistry.GetVariable<bool>(VariableName);
            chkbxControl.Checked += ChkbxControl_VarChanged;
            chkbxControl.Unchecked += ChkbxControl_VarChanged;

            grd_control.Children.Add(chkbxControl);
        }
        else if (varType == typeof(string))
        {
            var txtbxControl = new TextBox();
            txtbxControl.Text = VarRegistry.GetVariable<string>(VariableName);
            txtbxControl.TextChanged += Txtbx_control_TextChanged;

            grd_control.Children.Add(txtbxControl);
        }
        else if (varType == typeof(int))
        {
            if (VarRegistry.GetFlags(VariableName).HasFlag(VariableFlags.UseHEX))
            {
                var hexBox = new TextBox();
                hexBox.PreviewTextInput += HexBoxOnPreviewTextInput;
                hexBox.Text = $"{VarRegistry.GetVariable<int>(VariableName):X}";
                hexBox.TextChanged += HexBox_TextChanged;
                grd_control.Children.Add(hexBox);
            }
            else
            {
                var intUpDownControl = new IntegerUpDown();
                intUpDownControl.Value = VarRegistry.GetVariable<int>(VariableName);
                if (VarRegistry.GetVariableMax<int>(VariableName, out var maxVal))
                    intUpDownControl.Maximum = maxVal;
                if (VarRegistry.GetVariableMin<int>(VariableName, out var minVal))
                    intUpDownControl.Minimum = minVal;

                intUpDownControl.ValueChanged += IntUpDown_control_ValueChanged;

                grd_control.Children.Add(intUpDownControl);
            }
        }
        else if (varType == typeof(long))
        {
            var longUpDownControl = new LongUpDown();
            longUpDownControl.Value = VarRegistry.GetVariable<long>(VariableName);
            if (VarRegistry.GetVariableMax<long>(VariableName, out var maxVal))
                longUpDownControl.Maximum = maxVal;
            if (VarRegistry.GetVariableMin<long>(VariableName, out var minVal))
                longUpDownControl.Minimum = minVal;

            longUpDownControl.ValueChanged += LongUpDown_control_ValueChanged;

            grd_control.Children.Add(longUpDownControl);
        }
        else if (varType == typeof(double))
        {
            var doubleUpDownControl = new DoubleUpDown();
            doubleUpDownControl.Value = VarRegistry.GetVariable<double>(VariableName);
            if (VarRegistry.GetVariableMax<double>(VariableName, out var maxVal))
                doubleUpDownControl.Maximum = maxVal;
            if (VarRegistry.GetVariableMax<double>(VariableName, out var minVal))
                doubleUpDownControl.Minimum = minVal;
            doubleUpDownControl.ValueChanged += DoubleUpDown_control_ValueChanged;
            grd_control.Children.Add(doubleUpDownControl);
        }
        else if (varType == typeof(float))
        {
            var doubleUpDownControl = new DoubleUpDown();
            doubleUpDownControl.Value = VarRegistry.GetVariable<float>(VariableName);
            if (VarRegistry.GetVariableMax<float>(VariableName, out var maxVal))
                doubleUpDownControl.Maximum = maxVal;
            if (VarRegistry.GetVariableMax<float>(VariableName, out var minVal))
                doubleUpDownControl.Minimum = minVal;
            doubleUpDownControl.ValueChanged += DoubleUpDown_control_ValueChanged;
            grd_control.Children.Add(doubleUpDownControl);
        }
        else if (varType == typeof(Aurora.Settings.KeySequence))
        {
            var ctrl = new KeySequence
            {
                RecordingTag = varTitle,
                Sequence = VarRegistry.GetVariable<Aurora.Settings.KeySequence>(VariableName)
            };

            ctrl.SequenceUpdated += keySequenceControlValueChanged;

            grd_control.Children.Add(ctrl);
        }
        else if (varType == typeof(Utils.RealColor))
        {
            var clr = VarRegistry.GetVariable<Utils.RealColor>(VariableName);
            
            var ctrl = new ColorPicker
            {
                ColorMode = ColorMode.ColorCanvas,
                SelectedColor = clr.GetMediaColor()
            };
            ctrl.SelectedColorChanged += colorPickerControlValueChanged;

            grd_control.Children.Add(ctrl);
        }
        else if (varType == typeof(DeviceKeys))
        {
            var ctrl = new ComboBox
            {
                ItemsSource = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>().ToList(),
                SelectedValue = VarRegistry.GetVariable<DeviceKeys>(VariableName)
            };
            ctrl.SelectionChanged += CmbbxEnum_control_SelectionChanged;

            grd_control.Children.Add(ctrl);
        }
        else if (varType.IsEnum)
        {
            var cmbbxEnumControl = new ComboBox
            {
                ItemsSource = Enum.GetValues(varType),
                SelectedValue = VarRegistry.GetVariable<object>(VariableName)
            };
            cmbbxEnumControl.SelectionChanged += CmbbxEnum_control_SelectionChanged;

            grd_control.Children.Add(cmbbxEnumControl);
        }
        //else
        //throw new Exception($"Type {var_type} is not supported!");

        grd_control.UpdateLayout();
    }

    private void CmbbxEnum_control_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        VarRegistry.SetVariable(VariableName, (sender as ComboBox).SelectedItem);
    }

    private void colorPickerControlValueChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        var var_type = VarRegistry.GetVariableType(VariableName);
        var ctrlClr = (Color)((ColorPicker)sender).SelectedColor;
        var clr = VarRegistry.GetVariable<Utils.RealColor>(VariableName);
        clr.SetMediaColor(ctrlClr);
        VarRegistry.SetVariable(VariableName, clr);
    }

    private void keySequenceControlValueChanged(object? sender, EventArgs e)
    {
        VarRegistry.SetVariable(VariableName, ((KeySequence)sender).Sequence);
    }

    private void LongUpDown_control_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        VarRegistry.SetVariable(VariableName, (sender as LongUpDown).Value);
    }

    private void IntUpDown_control_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        VarRegistry.SetVariable(VariableName, (sender as IntegerUpDown).Value);
    }

    private void DoubleUpDown_control_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e) {
        VarRegistry.SetVariable(VariableName, (sender as DoubleUpDown).Value);
    }

    private void Txtbx_control_TextChanged(object? sender, TextChangedEventArgs e)
    {
        VarRegistry.SetVariable(VariableName, (sender as TextBox).Text);
    }

    private void HexBoxOnPreviewTextInput(object? sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var number);
    }

    private void HexBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = (sender as TextBox).Text;
        //Hacky fix to stop error when nothing is entered
        if (string.IsNullOrWhiteSpace(text))
            text = "0";
        VarRegistry.SetVariable(VariableName, int.Parse(text, NumberStyles.HexNumber, CultureInfo.CurrentCulture));
    }

    private void ChkbxControl_VarChanged(object? sender, RoutedEventArgs e)
    {
        VarRegistry.SetVariable(VariableName, (sender as CheckBox).IsChecked.Value);
    }

    private void btn_reset_Click(object? sender, RoutedEventArgs e)
    {
        VarRegistry.ResetVariable(VariableName);

        UpdateControls();
    }
}