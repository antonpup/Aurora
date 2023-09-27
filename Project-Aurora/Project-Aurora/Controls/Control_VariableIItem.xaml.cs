using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Aurora.Utils;
using Xceed.Wpf.Toolkit;

namespace Aurora.Controls;

/// <summary>
/// Interaction logic for Control_VariableItem.xaml
/// </summary>
public partial class Control_VariableItem
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty VariableTitleProperty = DependencyProperty.Register(nameof(VariableTitle), typeof(string), typeof(Control_VariableItem));

    public string VariableTitle
    {
        get => (string)GetValue(VariableTitleProperty);
        init
        {
            SetValue(VariableTitleProperty, value);

            UpdateControls();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty VariableDefaultObjectProperty = DependencyProperty.Register(nameof(VariableDefaultObject), typeof(object), typeof(Control_VariableItem));

    public object? VariableDefaultObject
    {
        get => GetValue(VariableDefaultObjectProperty);
        init => SetValue(VariableDefaultObjectProperty, value);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty VariableObjectProperty = DependencyProperty.Register(nameof(VariableObject), typeof(object), typeof(Control_VariableItem));

    public object? VariableObject
    {
        get => GetValue(VariableObjectProperty);
        set
        {
            SetValue(VariableObjectProperty, value);

            UpdateControls();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty VariableRemarkProperty = DependencyProperty.Register(nameof(VariableRemark), typeof(string), typeof(Control_VariableItem));

    public string VariableRemark
    {
        get => (string)GetValue(VariableRemarkProperty);
        init
        {
            SetValue(VariableRemarkProperty, value);

            UpdateControls();
        }
    }

    public delegate void VariableItemArgs(object? sender, object? newVariable);

    public event VariableItemArgs? VariableUpdated;

    private void UpdateControls()
    {
        if (string.IsNullOrWhiteSpace(VariableTitle))
            txtBlk_name.Visibility = Visibility.Collapsed;
        else
        {
            txtBlk_name.Visibility = Visibility.Visible;
            txtBlk_name.Text = VariableTitle;
        }

        if (string.IsNullOrWhiteSpace(VariableRemark))
            txtBlk_remark.Visibility = Visibility.Collapsed;
        else
        {
            txtBlk_remark.Visibility = Visibility.Visible;
            txtBlk_remark.Text = VariableRemark;
        }

        if (VariableDefaultObject == null)
            btn_reset.Visibility = Visibility.Collapsed;
        else
            btn_reset.Visibility = Visibility.Visible;

        //Create a control here...
        grd_control.Children.Clear();

        if (VariableObject != null)
        {
            var varType = VariableObject.GetType();

            if (varType == typeof(bool))
            {
                var chkbxControl = new CheckBox
                {
                    Content = "",
                    IsChecked = (bool)VariableObject
                };
                chkbxControl.Checked += Chkbx_control_VarChanged;
                chkbxControl.Unchecked += Chkbx_control_VarChanged;

                grd_control.Children.Add(chkbxControl);
            }
            else if (varType == typeof(string))
            {
                var txtbxControl = new TextBox
                {
                    Text = (string)VariableObject
                };
                txtbxControl.TextChanged += Txtbx_control_TextChanged;

                grd_control.Children.Add(txtbxControl);
            }
            else if (varType == typeof(int))
            {
                var intUpDownControl = new IntegerUpDown
                {
                    Value = (int)VariableObject
                };
                intUpDownControl.ValueChanged += IntUpDown_control_ValueChanged;

                grd_control.Children.Add(intUpDownControl);
            }
            else if (varType == typeof(long))
            {
                var longUpDownControl = new LongUpDown
                {
                    Value = (long)VariableObject
                };
                longUpDownControl.ValueChanged += LongUpDown_control_ValueChanged;

                grd_control.Children.Add(longUpDownControl);
            }
            else if (varType == typeof(float))
            {
                var floatUpDownControl = new DoubleUpDown
                {
                    Value = (float)VariableObject
                };
                floatUpDownControl.ValueChanged += FloatUpDown_control_ValueChanged;

                grd_control.Children.Add(floatUpDownControl);
            }
            else if (varType == typeof(double))
            {
                var doubleUpDownControl = new DoubleUpDown
                {
                    Value = (double)VariableObject
                };
                doubleUpDownControl.ValueChanged += DoubleUpDown_control_ValueChanged;

                grd_control.Children.Add(doubleUpDownControl);
            }
            else if (varType == typeof(System.Drawing.Color))
            {
                var colorPickerDrawingControl = new ColorPicker
                {
                    SelectedColor = ColorUtils.DrawingColorToMediaColor((System.Drawing.Color)VariableObject),
                    ColorMode = ColorMode.ColorCanvas
                };
                colorPickerDrawingControl.SelectedColorChanged += ColorPickerDrawing_control_SelectedColorChanged;

                grd_control.Children.Add(colorPickerDrawingControl);
            }
            else if (varType == typeof(Color))
            {
                var colorPickerMediaControl = new ColorPicker
                {
                    SelectedColor = (Color)VariableObject,
                    ColorMode = ColorMode.ColorCanvas
                };
                colorPickerMediaControl.SelectedColorChanged += ColorPickerMedia_control_SelectedColorChanged;

                grd_control.Children.Add(colorPickerMediaControl);
            }
            else if (varType == typeof(Aurora.Settings.KeySequence))
            {
                var ctrl = new KeySequence
                {
                    Sequence = (Aurora.Settings.KeySequence)VariableObject
                };
                ctrl.SequenceUpdated += keySequenceControlValueChanged;
                ctrl.RecordingTag = VariableTitle;

                grd_control.Children.Add(ctrl);
            }
            else if (varType == typeof(RealColor))
            {
                var ctrl = new ColorPicker
                {
                    ColorMode = ColorMode.ColorCanvas
                };
                var clr = (RealColor)VariableObject;

                ctrl.SelectedColor = clr.GetMediaColor();
                ctrl.SelectedColorChanged += colorPickerControlValueChanged;
                grd_control.Children.Add(ctrl);
            }
            else if (varType == typeof(System.Drawing.Color))
            {
                var ctrl = new ColorPicker
                {
                    ColorMode = ColorMode.ColorCanvas
                };
                var clr = ColorUtils.DrawingColorToMediaColor((System.Drawing.Color)VariableObject);

                ctrl.SelectedColor = clr;
                ctrl.SelectedColorChanged += colorPickerControlValueChanged;
                grd_control.Children.Add(ctrl);
            }
            else if (varType.IsEnum)
            {
                var cmbbxEnumControl = new ComboBox
                {
                    ItemsSource = Enum.GetValues(varType),
                    SelectedValue = (Enum)VariableObject
                };
                cmbbxEnumControl.SelectionChanged += CmbbxEnum_control_SelectionChanged;

                grd_control.Children.Add(cmbbxEnumControl);
            }
            else
            {
                var textItem = new TextBlock
                {
                    Text = varType.ToString()
                };
 
                grd_control.Children.Add(textItem);
            }

        }

        grd_control.UpdateLayout();
    }

    private void CmbbxEnum_control_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SetValue(VariableObjectProperty, (sender as ComboBox).SelectedValue);

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void colorPickerControlValueChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        var var_type = VariableObject.GetType();
        var ctrlClr = (Color)((ColorPicker)sender).SelectedColor;
        var clr = (RealColor)VariableObject;
        clr.SetMediaColor(ctrlClr);
        VariableObject = clr;
    }

    private void keySequenceControlValueChanged(object? sender, EventArgs e)
    {
        SetValue(VariableObjectProperty, ((KeySequence)sender).Sequence);

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void ColorPickerMedia_control_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        SetValue(VariableObjectProperty, (sender as ColorPicker).SelectedColor.Value);

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void ColorPickerDrawing_control_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        var picker = sender as ColorPicker;
        if (picker.SelectedColor == null)
            picker.SelectedColor = Color.FromArgb(0, 0, 0, 0);

        SetValue(VariableObjectProperty, ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value));

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void DoubleUpDown_control_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SetValue(VariableObjectProperty, (double)((sender as DoubleUpDown).Value ?? 0.0));

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void FloatUpDown_control_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SetValue(VariableObjectProperty, (float)((sender as DoubleUpDown).Value ?? 0.0f));

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void LongUpDown_control_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SetValue(VariableObjectProperty, (sender as LongUpDown).Value ?? 0L);

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void IntUpDown_control_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SetValue(VariableObjectProperty, (sender as IntegerUpDown).Value ?? 0);

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void Txtbx_control_TextChanged(object? sender, TextChangedEventArgs e)
    {
        SetValue(VariableObjectProperty, (sender as TextBox).Text);

        VariableUpdated?.Invoke(this, VariableObject);
    }

    private void Chkbx_control_VarChanged(object? sender, RoutedEventArgs e)
    {
        SetValue(VariableObjectProperty, (sender as CheckBox).IsChecked.Value);

        VariableUpdated?.Invoke(this, VariableObject);
    }

    public Control_VariableItem()
    {
        InitializeComponent();

        VariableTitle = null;
        VariableDefaultObject = null;
        VariableObject = null;
        VariableRemark = null;
    }

    private void btn_reset_Click(object? sender, RoutedEventArgs e)
    {
        VariableObject = VariableDefaultObject;

        UpdateControls();
    }
}