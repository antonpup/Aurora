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
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Aurora.Settings.DeviceLayoutViewer
{

    /// <summary>
    /// Interaction logic for Control_DeviceLayoutPresenter.xaml
    /// </summary>
    public partial class Control_DeviceLayoutPresenter : UserControl
    {
        //List<Control_DeviceLayout> DeviceLayouts = new List<Control_DeviceLayout>();
        private System.Windows.Point _positionInBlock;

        public List<Control_Keycap> Keycaps => DeviceLayouts.SelectMany(dl => dl.KeycapLayouts).ToList();

        public static readonly DependencyProperty IsLayoutMoveEnabledProperty = DependencyProperty.Register("IsLayoutMoveEnabled",
                                                                                                typeof(bool),
                                                                                                typeof(Control_DeviceLayoutPresenter), new PropertyMetadata(false));

        public ObservableCollection<Control_DeviceLayout> DeviceLayouts
        {
            get;
            private set;
        } = new ObservableCollection<Control_DeviceLayout>();
        public bool IsLayoutMoveEnabled
        {
            get { return (bool)GetValue(IsLayoutMoveEnabledProperty); }
            set { SetValue(IsLayoutMoveEnabledProperty, value); }
        }

        public Control_DeviceLayoutPresenter()
        {
            InitializeComponent();
            layouts_viewbox.DataContext = this;

            Global.Configuration.PropertyChanged += Configuration_PropertyChanged;
            this.keyboard_record_message.Visibility = Visibility.Hidden;
            Global.devicesLayout.DevicesConfigChanged += DevicesConfigChanged;

            editor_canvas.Children.Add(new LayerEditor(editor_canvas));
            //DeviceLayoutNumberChanged(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Global.devicesLayout.Load();
        }
        private void DevicesConfigChanged(DeviceConfig changedConf)
        {
            var layoutQuery = DeviceLayouts.Where(l => l.DeviceConfig == changedConf);
            if (Global.devicesLayout.DevicesConfig.Values.Contains(changedConf)){
                if (layoutQuery.Any())
                {
                    Layout_DeviceLayoutUpdated(layoutQuery.First());
                }
                else
                {
                    Control_DeviceLayout layout = new Control_DeviceLayout(changedConf);
                    layout.DeviceLayoutUpdated += Layout_DeviceLayoutUpdated;
                    ///layout.DeviceConfig.ConfigurationChanged += Layout_DeviceConfigUpdated;
                    layout.MouseDoubleClick += DeviceLayout_MouseDoubleClick;
                    layout.MouseDown += DeviceLayout_MouseDown;
                    layout.MouseMove += DeviceLayout_MouseMove;
                    layout.MouseUp += DeviceLayout_MouseUp;
                    DeviceLayouts.Add(layout);
                }
            }
            else
            {
                DeviceLayouts.Remove(layoutQuery.First());
            }
        }


        public void Refresh()
        {
            var keylights = Global.effengine.GetDevicesColor();
            foreach (Control_DeviceLayout layout in DeviceLayouts)
            {
                layout.SetKeyboardColors(keylights);
            }

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
            Global.devicesLayout.AddNewDeviceLayout();
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
            double current_width = 800;
            double current_height = 200;
            double baseline_x = double.MaxValue;
            double baseline_y = double.MaxValue;

            foreach (FrameworkElement layout in DeviceLayouts)
            {
                Point offset = layout.TranslatePoint(new Point(0, 0), layout_container);

                if (offset.X < baseline_x)
                    baseline_x = offset.X;

                if (offset.Y < baseline_y)
                    baseline_y = offset.Y;

                if (offset.X + layout.Width > current_width)
                    current_width = offset.X + layout.Width;

                if (offset.Y + layout.Height > current_height)
                    current_height = offset.Y + layout.Height;

                //layout as Control_DeviceLayout).SaveLayoutPosition(offset);
            }
            foreach (Control_DeviceLayout layout in DeviceLayouts)
            {
                //layout.DeviceConfig.Offset = new Point((int)(layout.DeviceConfig.Offset.X - baseline_x), (int)(layout.DeviceConfig.Offset.Y - baseline_y));
                layout.RenderTransform = new TranslateTransform(layout.DeviceConfig.Offset.X, layout.DeviceConfig.Offset.Y);
            }

            layout_container.Width = current_width;
            layout_container.Height = current_height;
            Effects.grid_baseline_x = 0;
            Effects.grid_baseline_y = 0;
            Effects.grid_width = (float)layout_container.Width;
            Effects.grid_height = (float)layout_container.Height;

            layouts_viewbox.MaxWidth = layout_container.Width;
            layouts_viewbox.MaxHeight = layout_container.Height;
            layouts_viewbox.UpdateLayout();
            this.UpdateLayout();
            CalculateBitmap();

        }
        public void CalculateBitmap()
        {
            Task.Run(() =>
            {
                Dispatcher.Invoke(() => {
                    var bitmap = new Dictionary<DeviceKey, BitmapRectangle>(new DeviceKey.EqualityComparer());

                    foreach (Control_DeviceLayout layout in DeviceLayouts)
                    {
                        foreach (var b in layout.GetBitmap())
                        {
                            if (!bitmap.ContainsKey(b.Key))
                                bitmap.Add(b.Key, b.Value);
                        }
                    }
                    Global.effengine.SetCanvasSize(Control_DeviceLayout.PixelToByte(layout_container.Width) + 1, Control_DeviceLayout.PixelToByte(layout_container.Height) + 1);
                    Global.effengine.SetBitmapping(bitmap);
                });
            });
        }
        private void DeviceLayout_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsLayoutMoveEnabled)
            {
                var layout = sender as Control_DeviceLayout;
                layout.ReleaseMouseCapture();
                Window_DeviceConfig configWindow = new Window_DeviceConfig(layout);
                configWindow.WindowState = WindowState.Normal;
                configWindow.Activate();
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
                    senderLayout.DeviceConfig.Offset = new Point(layoutTranslate.X, layoutTranslate.Y);
                    Global.devicesLayout.SaveConfiguration(senderLayout.DeviceConfig);
                }

            }
        }

    }
 }
