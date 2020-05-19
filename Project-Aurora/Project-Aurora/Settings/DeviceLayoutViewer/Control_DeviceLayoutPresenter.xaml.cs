using Aurora.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
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
                                                                                                typeof(Control_DeviceLayoutPresenter), new PropertyMetadata(false));

        public bool IsLayoutMoveEnabled
        {
            get { return (bool)GetValue(IsLayoutMoveEnabledProperty); }
            set { SetValue(IsLayoutMoveEnabledProperty, value); }
        }

        public Control_DeviceLayoutPresenter()
        {
            InitializeComponent();
            layouts_viewbox.DataContext = this;

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
            MenuItem menuItem = new MenuItem() { Header = "Edit Layout Enabled", IsCheckable = true };

            Binding binding = new Binding("IsLayoutMoveEnabled") {Source = this, Mode = BindingMode.TwoWay};
            menuItem.SetBinding(MenuItem.IsCheckedProperty, binding);

            ContextMenu cm = new ContextMenu() { PlacementTarget = sender as Button, IsOpen = true };
            cm.Items.Add(menuItem);
        }
        private void Layout_DeviceLayoutUpdated(object sender)
        {
            double current_width = double.MinValue;
            double current_height = double.MinValue;
            foreach (FrameworkElement layout in DeviceLayouts)
            {
                Point offset = layout.TranslatePoint(new Point(0, 0), layouts_grid);

                if (offset.X + layout.Width > current_width)
                    current_width = offset.X + layout.Width;

                if (offset.Y + layout.Height > current_height)
                    current_height = offset.Y + layout.Height;

                //layout as Control_DeviceLayout).SaveLayoutPosition(offset);
            }

            layouts_grid.Width = current_width;
            layouts_grid.Height = current_height;
            Effects.grid_baseline_x = 0;
            Effects.grid_baseline_y = 0;
            Effects.grid_width = (float)layouts_grid.Width;
            Effects.grid_height = (float)layouts_grid.Height;

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
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => {
                        Global.effengine.SetCanvasSize(Control_DeviceLayout.PixelToByte(layouts_grid.Width) + 1, Control_DeviceLayout.PixelToByte(layouts_grid.Height) + 1);
                        var bitmap = new Dictionary<DeviceKey, BitmapRectangle>(new DeviceKey.EqualityComparer());
                        DeviceLayouts.ForEach(item => item.GetBitmap().ToList().ForEach(x =>
                        {
                            if (!bitmap.ContainsKey(x.Key))
                                bitmap.Add(x.Key, x.Value);
                        }));
                        Global.effengine.SetBitmapping(bitmap);
                    });
                });
            }
        }
        private void DeviceLayoutNumberChanged(object sender)
        {
            DeviceLayouts = Global.devicesLayout.GetDeviceLayouts();
            //keyboard_grid.ClipToBounds = true;

            layouts_grid.Children.Clear();
            if (DeviceLayouts.Count != 0)
            {
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
            else {
                Label error_message = new Label();

                /*DockPanel info_panel = new DockPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };*/

                TextBlock info_message = new TextBlock()
                {
                    Text = "To enable/disable layout editor right click on this box",
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)),
                };

                DockPanel.SetDock(info_message, Dock.Top);
                //info_panel.Children.Add(info_message);

                error_message.Content = info_message;

                error_message.FontSize = 16.0;
                error_message.FontWeight = FontWeights.Bold;
                error_message.HorizontalContentAlignment = HorizontalAlignment.Center;
                error_message.VerticalContentAlignment = VerticalAlignment.Center;

                layouts_grid.Children.Add(error_message);
                //Update size
                layouts_grid.Width = 450;
                layouts_grid.Height = 200;
                layouts_viewbox.MaxWidth = 800;
                layouts_viewbox.MaxHeight = 300;
            }
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
            var senderLayout = sender as Control_DeviceLayout;
            if (senderLayout.IsMouseCaptured)
            {
                // release this control.
                senderLayout.ReleaseMouseCapture();

                if (senderLayout.RenderTransform is TranslateTransform layoutTranslate)
                {
                    senderLayout.DeviceConfig.Offset.X = layoutTranslate.X;
                    senderLayout.DeviceConfig.Offset.Y = layoutTranslate.Y;
                    Global.devicesLayout.SaveConfiguration(senderLayout.DeviceConfig);
                }
            }
        }

    }
 }
