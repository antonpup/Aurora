using System.ComponentModel;
using Aurora.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Aurora.Settings;

namespace Aurora.Applications.Application
{
    
    //TODO: Setup enum descriptions
    public enum ApplicationRenderType
    {
        BelowForeground,
        Foreground,
        AboveForeground
    }
    
    public class ApplicationSettings : SettingsProfile
    {
        //protected ApplicationRenderType?
        //public ApplicationRenderType? RenderType { get; set; }
        protected bool enabled;
        public bool Enabled
        {
            get => enabled;
            set => UpdateVar(ref enabled, value);
        }
       
        public ApplicationSettings()
        {
                
        }

        public override void Default()
        {
            //RenderType = ApplicationRenderType.Foreground;
            Enabled = true;
        }
    }
}