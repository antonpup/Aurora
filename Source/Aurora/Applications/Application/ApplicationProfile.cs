using System.Collections.ObjectModel;
using Aurora.Applications.Layers;
using Aurora.Settings;

namespace Aurora.Applications.Application
{
    public class ApplicationProfile : SettingsProfile
    {
        private ObservableCollection<ILayer> _layers;
        public ObservableCollection<ILayer> Layers
        {
            get => _layers;
            set => UpdateVar(ref _layers, value);
        }
        
        public override void Default()
        {
            
        }
    }
}