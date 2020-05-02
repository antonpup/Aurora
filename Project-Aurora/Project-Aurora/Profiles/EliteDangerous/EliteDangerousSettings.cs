using Aurora.Settings;

namespace Aurora.Profiles.EliteDangerous
{
    public class EliteDangerousSettings : FirstTimeApplicationSettings
    {
        private string gamePath = "";
        
        public string GamePath { get { return gamePath; } set { gamePath = value; InvokePropertyChanged(); } }
    }
}