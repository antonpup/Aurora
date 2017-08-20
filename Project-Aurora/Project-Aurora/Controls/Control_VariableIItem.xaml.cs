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
    /// Interaction logic for Control_VariableItem.xaml
    /// </summary>
    public partial class Control_VariableItem : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty VariableTitleProperty = DependencyProperty.Register("VariableTitle", typeof(string), typeof(Control_VariableItem));

        public string VariableTitle
        {
            get
            {
                return (string)GetValue(VariableTitleProperty);
            }
            set
            {
                SetValue(VariableTitleProperty, value);

                UpdateControls();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty VariableDefaultObjectProperty = DependencyProperty.Register("VariableDefaultObject", typeof(object), typeof(Control_VariableItem));

        public object VariableDefaultObject
        {
            get
            {
                return GetValue(VariableDefaultObjectProperty);
            }
            set
            {
                SetValue(VariableDefaultObjectProperty, value);

                //UpdateControls();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty VariableObjectProperty = DependencyProperty.Register("VariableObject", typeof(object), typeof(Control_VariableItem));

        public object VariableObject
        {
            get
            {
                return GetValue(VariableObjectProperty);
            }
            set
            {
                SetValue(VariableObjectProperty, value);

                UpdateControls();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public static readonly DependencyProperty VariableRemarkProperty = DependencyProperty.Register("VariableRemark", typeof(string), typeof(Control_VariableItem));

        public string VariableRemark
        {
            get
            {
                return (string)GetValue(VariableRemarkProperty);
            }
            set
            {
                SetValue(VariableRemarkProperty, value);

                UpdateControls();
            }
        }

        public delegate void VariableItemArgs(object sender, object newVariable);

        public event VariableItemArgs VariableUpdated;

        private void UpdateControls()
        {
            if (String.IsNullOrWhiteSpace(VariableTitle))
                this.txtBlk_name.Visibility = Visibility.Collapsed;
            else
            {
                this.txtBlk_name.Visibility = Visibility.Visible;
                this.txtBlk_name.Text = VariableTitle;
            }

            if (String.IsNullOrWhiteSpace(VariableRemark))
                this.txtBlk_remark.Visibility = Visibility.Collapsed;
            else
            {
                this.txtBlk_remark.Visibility = Visibility.Visible;
                this.txtBlk_remark.Text = VariableRemark;
            }

            if (VariableDefaultObject == null)
                this.btn_reset.Visibility = Visibility.Collapsed;
            else
                this.btn_reset.Visibility = Visibility.Visible;

            //Create a control here...
            grd_control.Children.Clear();

            if (VariableObject != null)
            {
                Type var_type = VariableObject.GetType();

                if (var_type == typeof(bool))
                {
                    CheckBox chkbx_control = new CheckBox();
                    chkbx_control.Content = "";
                    chkbx_control.IsChecked = (bool)VariableObject;
                    chkbx_control.Checked += Chkbx_control_VarChanged;
                    chkbx_control.Unchecked += Chkbx_control_VarChanged;

                    grd_control.Children.Add(chkbx_control);
                }
                else if (var_type == typeof(string))
                {
                    TextBox txtbx_control = new TextBox();
                    txtbx_control.Text = (string)VariableObject;
                    txtbx_control.TextChanged += Txtbx_control_TextChanged;

                    grd_control.Children.Add(txtbx_control);
                }
                else if (var_type == typeof(int))
                {
                    IntegerUpDown intUpDown_control = new IntegerUpDown();
                    intUpDown_control.Value = (int)VariableObject;
                    intUpDown_control.ValueChanged += IntUpDown_control_ValueChanged;

                    grd_control.Children.Add(intUpDown_control);
                }
                else if (var_type == typeof(long))
                {
                    LongUpDown longUpDown_control = new LongUpDown();
                    longUpDown_control.Value = (long)VariableObject;
                    longUpDown_control.ValueChanged += LongUpDown_control_ValueChanged;

                    grd_control.Children.Add(longUpDown_control);
                }
                else if (var_type == typeof(float))
                {
                    DoubleUpDown floatUpDown_control = new DoubleUpDown();
                    floatUpDown_control.Value = (float)VariableObject;
                    floatUpDown_control.ValueChanged += FloatUpDown_control_ValueChanged;

                    grd_control.Children.Add(floatUpDown_control);
                }
                else if (var_type == typeof(double))
                {
                    DoubleUpDown doubleUpDown_control = new DoubleUpDown();
                    doubleUpDown_control.Value = (double)VariableObject;
                    doubleUpDown_control.ValueChanged += DoubleUpDown_control_ValueChanged;

                    grd_control.Children.Add(doubleUpDown_control);
                }
                else if (var_type == typeof(System.Drawing.Color))
                {
                    ColorPicker colorPickerDrawing_control = new ColorPicker();
                    colorPickerDrawing_control.SelectedColor = Utils.ColorUtils.DrawingColorToMediaColor((System.Drawing.Color)VariableObject);
                    colorPickerDrawing_control.ColorMode = ColorMode.ColorCanvas;
                    colorPickerDrawing_control.SelectedColorChanged += ColorPickerDrawing_control_SelectedColorChanged;

                    grd_control.Children.Add(colorPickerDrawing_control);
                }
                else if (var_type == typeof(System.Windows.Media.Color))
                {
                    ColorPicker colorPickerMedia_control = new ColorPicker();
                    colorPickerMedia_control.SelectedColor = (System.Windows.Media.Color)VariableObject;
                    colorPickerMedia_control.ColorMode = ColorMode.ColorCanvas;
                    colorPickerMedia_control.SelectedColorChanged += ColorPickerMedia_control_SelectedColorChanged;

                    grd_control.Children.Add(colorPickerMedia_control);
                }
                else if (var_type == typeof(Aurora.Settings.KeySequence))
                {
                    Aurora.Controls.KeySequence ctrl = new Aurora.Controls.KeySequence();
                    ctrl.Sequence = (Aurora.Settings.KeySequence)VariableObject;
                    ctrl.SequenceUpdated += keySequenceControlValueChanged;
                    ctrl.RecordingTag = VariableTitle;

                    grd_control.Children.Add(ctrl);
                }
                else if (var_type == typeof(Aurora.Utils.RealColor))
                {
                    ColorPicker ctrl = new ColorPicker();
                    ctrl.ColorMode = ColorMode.ColorCanvas;
                    Aurora.Utils.RealColor clr = (Aurora.Utils.RealColor)VariableObject;

                    ctrl.SelectedColor = clr.GetMediaColor();
                    ctrl.SelectedColorChanged += colorPickerControlValueChanged;
                    grd_control.Children.Add(ctrl);
                }
                else if (var_type.IsEnum)
                {
                    ComboBox cmbbxEnum_control = new ComboBox();
                    cmbbxEnum_control.ItemsSource = Enum.GetValues(var_type);
                    cmbbxEnum_control.SelectedValue = (Enum)VariableObject;
                    cmbbxEnum_control.SelectionChanged += CmbbxEnum_control_SelectionChanged;

                    grd_control.Children.Add(cmbbxEnum_control);
                }

            }

            grd_control.UpdateLayout();
        }

        private void CmbbxEnum_control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetValue(VariableObjectProperty, (sender as ComboBox).SelectedValue);

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void colorPickerControlValueChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Type var_type = VariableObject.GetType();
            System.Windows.Media.Color ctrlClr = (System.Windows.Media.Color)((ColorPicker)sender).SelectedColor;
            Aurora.Utils.RealColor clr = (Aurora.Utils.RealColor)VariableObject;
            clr.SetMediaColor(ctrlClr);
            VariableObject = clr;
        }

        private void keySequenceControlValueChanged(object sender, EventArgs e)
        {
            SetValue(VariableObjectProperty, ((Aurora.Controls.KeySequence)sender).Sequence);

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void ColorPickerMedia_control_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SetValue(VariableObjectProperty, (sender as ColorPicker).SelectedColor.Value);

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void ColorPickerDrawing_control_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ColorPicker picker = (sender as ColorPicker);
            if (picker.SelectedColor == null)
                picker.SelectedColor = Color.FromArgb(0, 0, 0, 0);

            SetValue(VariableObjectProperty, Utils.ColorUtils.MediaColorToDrawingColor(picker.SelectedColor.Value));

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void DoubleUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetValue(VariableObjectProperty, (double)((sender as DoubleUpDown).Value ?? 0.0));

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void FloatUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetValue(VariableObjectProperty, (float)((sender as DoubleUpDown).Value ?? 0.0f));

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void LongUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetValue(VariableObjectProperty, (sender as LongUpDown).Value ?? 0L);

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void IntUpDown_control_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetValue(VariableObjectProperty, (sender as IntegerUpDown).Value ?? 0);

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void Txtbx_control_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetValue(VariableObjectProperty, (sender as TextBox).Text);

            VariableUpdated?.Invoke(this, VariableObject);
        }

        private void Chkbx_control_VarChanged(object sender, RoutedEventArgs e)
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

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            VariableObject = VariableDefaultObject;

            UpdateControls();
        }
    }
}
