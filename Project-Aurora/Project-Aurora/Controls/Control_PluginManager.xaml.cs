using Aurora.Settings;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Aurora.Controls
{
    /// <summary>
    /// Interaction logic for Control_DeviceManager.xaml
    /// </summary>
    public partial class Control_PluginManager : UserControl
    {
        private IPluginHost host;

        public IPluginHost Host { get { return host; } set { host = value; this.DataContext = value; } }

        public Control_PluginManager()
        {
            InitializeComponent();
        }

        private void chkEnabled_Checked(object? sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            var plugin = (KeyValuePair<string, IPlugin>)chk.DataContext;
            Host.SetPluginEnabled(plugin.Key, (bool)chk.IsChecked);
        }
    }
}
