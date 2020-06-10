using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public class ApplicationSettings : INotifyPropertyChanged
    {
        public bool IsEnabled { get; set; } = true;
        public bool IsOverlayEnabled { get; set; } = true;
        public bool Hidden { get; set; } = false;
        public string SelectedProfile { get; set; } = "default";

        public event PropertyChangedEventHandler PropertyChanged;

        public ApplicationSettings() { }
    }

    public class FirstTimeApplicationSettings : ApplicationSettings
    {
        public bool IsFirstTimeInstalled { get; set; }

        public FirstTimeApplicationSettings() { }
    }
}
