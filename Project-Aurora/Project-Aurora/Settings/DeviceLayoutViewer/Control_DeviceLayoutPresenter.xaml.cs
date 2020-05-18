using Aurora.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Settings.DeviceLayoutViewer
{
    /// <summary>
    /// Interaction logic for Control_DeviceLayoutPresenter.xaml
    /// </summary>
    public partial class Control_DeviceLayoutPresenter : UserControl
    {
        List<Control_DeviceLayout> DeviceLayouts = new List<Control_DeviceLayout>();
        private System.Windows.Point _positionInBlock;

        public List<Control_Keycap> Keycaps => DeviceLayouts.SelectMany(dl => dl.KeyboardMap.Values).ToList();
        public static readonly DependencyProperty IsLayoutMoveEnabledProperty = DependencyProperty.Register("IsLayoutMoveEnabled",
                                                                                                typeof(bool),
                                                                                                typeof(Control_DeviceLayoutPresenter));

        public bool IsLayoutMoveEnabled
        {
            get { return (bool)GetValue(IsLayoutMoveEnabledProperty); }
            set { SetValue(IsLayoutMoveEnabledProperty, value); }
        }
        
        private void enableEditLayout_Click(object sender, RoutedEventArgs e)
        {
            IsLayoutMoveEnabled = !IsLayoutMoveEnabled;
        }

        public Control_DeviceLayoutPresenter()
        {
            InitializeComponent();
            DataContext = this;
            Global.devicesLayout.DeviceLayoutNumberChanged += DeviceLayoutNumberChanged;
            Global.Configuration.PropertyChanged += Configuration_PropertyChanged;
            this.keyboard_record_message.Visibility = Visibility.Hidden;
            
            //DeviceLayoutNumberChanged(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.devicesLayout.Load();
            //DeviceLayoutNumberChanged(this);
        }

        public void Refresh()
        {
            var keylights = Global.effengine.GetDevicesColor();
            DeviceLayouts.ForEach(dp => dp.SetKeyboardColors(keylights));

            if (Global.key_recorder.IsRecording())
                this.keyboard_record_message.Visibility = Visibility.Visible;
            else
                this.keyboard_record_message.Visibility = Visibility.Hidden;
        }
        private void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Configuration.BitmapAccuracy)))
            {
                CalculateBitmap();
            }
        }
        private void AddNewDeviceLayout(object sender, RoutedEventArgs e)
        {
            Global.devicesLayout.AddNewDevice();
            //DeviceLayoutNumberChanged(this);
        }
        
        private void OpenEnableMenu(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("enableMenu") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }
        private void Layout_DeviceLayoutUpdated(object sender)
        {

            double baseline_x = double.MaxValue;
            double baseline_y = double.MaxValue;
            double current_width = double.MinValue;
            double current_height = double.MinValue;
            foreach (FrameworkElement layout in DeviceLayouts)
            {
                Point offset = layout.TranslatePoint(new Point(0, 0), layouts_grid);

                if (offset.X + layout.Width > current_width)
                    current_width = offset.X + layout.Width;
                if (offset.X < baseline_x)
                    baseline_x = offset.X;

                if (offset.Y + layout.Height > current_height)
                    current_height = offset.Y + layout.Height;
                if (offset.Y < baseline_y)
                    baseline_y = offset.Y;
               //layout as Control_DeviceLayout).SaveLayoutPosition(offset);
            }
            /*foreach (UIElement layout in layouts_grid.Children)
            {
                Point offset = layout.TranslatePoint(new Point(0, 0), layouts_grid);
                /*if (offset.X + layout.Width - baseline_x > current_width)
                    current_width = offset.X + layout.Width- baseline_x;

                if (offset.Y + layout.Height - baseline_y > current_height)
                    current_height = offset.Y + layout.Height - baseline_y;*/
            //layout.RenderTransform = new TranslateTransform(offset
            //layout.Margin = new Thickness(offset.X - baseline_x, offset.Y - baseline_y, 0, 0);
            /* layout.RenderTransform = new TranslateTransform(offset.X - baseline_x, offset.Y - baseline_y);
             if (layout is Control_DeviceLayout)
                 (layout as Control_DeviceLayout).SaveLayoutPosition(offset);
         }*/

            layouts_grid.Width = current_width - baseline_x;
            //this.Width = width + (keyboard_grid.Width - virtual_keyboard_width);

            layouts_grid.Height = current_height - baseline_y;
            foreach (UIElement layout in layouts_grid.Children)
            {
                Point offset = layout.TranslatePoint(new Point(0, 0), layouts_grid);
                layout.RenderTransform = new TranslateTransform(offset.X - baseline_x, offset.Y - baseline_y);
                layout.UpdateLayout();
            }
             //layouts_grid.RenderTransform = new TranslateTransform(-baseline_x, -baseline_y);
             //keyboard_grid.Clip = new RectangleGeometry(new Rect(baseline_x, baseline_x, current_width - baseline_x, current_height - baseline_y));
             //this.Height = height + (keyboard_grid.Height - virtual_keyboard_height);
             Effects.grid_baseline_x = 0;// (float)-baseline_x;
             Effects.grid_baseline_y = 0;// (float)-baseline_y;
             Effects.grid_width = (float)layouts_grid.Width;
             Effects.grid_height = (float)layouts_grid.Height;


             //keyboard_grid.Margin = new Thickness(-baseline_x, -baseline_y, 0, 0);
             //layout2.LayoutTransform = new TranslateTransform(layout2.Region.X, layout2.Region.Y);
             layouts_viewbox.MaxWidth = layouts_grid.Width;
             layouts_viewbox.MaxHeight = layouts_grid.Height;
             layouts_grid.UpdateLayout();
             layouts_viewbox.UpdateLayout();
             this.UpdateLayout();
             CalculateBitmap();

        }
        public void CalculateBitmap()
        {
            if (IsLayoutMoveEnabled)
            {
                Global.effengine.SetCanvasSize(Control_DeviceLayout.PixelToByte(layouts_grid.Width) + 1, Control_DeviceLayout.PixelToByte(layouts_grid.Height) + 1);
                var bitmap = new Dictionary<DeviceKey, BitmapRectangle>(new DeviceKey.EqualityComparer());
                DeviceLayouts.ForEach(item => item.GetBitmap().ToList().ForEach(x => {
                    if (!bitmap.ContainsKey(x.Key))
                        bitmap.Add(x.Key, x.Value);
                }));
                Global.effengine.SetBitmapping(bitmap);
            }
        }
        private void DeviceLayoutNumberChanged(object sender)
        {
            DeviceLayouts = Global.devicesLayout.GetDeviceLayouts();
            //keyboard_grid.ClipToBounds = true;

            layouts_grid.Children.Clear();
            foreach (var layout in DeviceLayouts)
            {
                layouts_grid.Children.Add(layout);
                layout.DeviceLayoutUpdated += Layout_DeviceLayoutUpdated;
                layout.MouseDoubleClick += DeviceLayout_MouseDoubleClick;
                layout.MouseDown += DeviceLayout_MouseDown;
                layout.MouseMove += DeviceLayout_MouseMove;
                layout.MouseUp += DeviceLayout_MouseUp;
            }
            layouts_grid.Children.Add(new LayerEditor(layouts_grid));
        }
        private void DeviceLayout_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsLayoutMoveEnabled)
            {
                var layout = sender as Control_DeviceLayout;
                layout.ReleaseMouseCapture();
                Window_DeviceConfig configWindow = new Window_DeviceConfig(layout);
                configWindow.Show();
            }

        }
        private void DeviceLayout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var layout = sender as Control_DeviceLayout;
            if (e.ClickCount < 2 && IsLayoutMoveEnabled)
            {
                // when the mouse is down, get the position within the current control. (so the control top/left doesn't move to the mouse position)
                _positionInBlock = Mouse.GetPosition(layout);

                // capture the mouse (so the mouse move events are still triggered (even when the mouse is not above the control)
                layout.CaptureMouse();
            }

        }

        private void DeviceLayout_MouseMove(object sender, MouseEventArgs e)
        {
            var layout = sender as Control_DeviceLayout;
            // if the mouse is captured. you are moving it. (there is your 'real' boolean)
            if (layout.IsMouseCaptured)
            {
                // get the parent container
                var container = VisualTreeHelper.GetParent(layout) as UIElement;

                // get the position within the container
                var mousePosition = e.GetPosition(container);

                // move the usercontrol.
                layout.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, mousePosition.Y - _positionInBlock.Y);

            }
        }

        private void DeviceLayout_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var layout = sender as Control_DeviceLayout;
            if (layout.IsMouseCaptured)
            {
                // release this control.
                layout.ReleaseMouseCapture();

                if (layout.RenderTransform is TranslateTransform)
                {
                    layout.DeviceConfig.Offset.X = (layout.RenderTransform as TranslateTransform).X;
                    layout.DeviceConfig.Offset.Y = (layout.RenderTransform as TranslateTransform).Y;
                    layout.DeviceConfig.Save();
                }
            }
        }

    }
 }
