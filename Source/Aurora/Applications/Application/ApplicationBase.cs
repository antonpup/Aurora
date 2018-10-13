using Aurora.EffectsEngine;
using Aurora.Settings;
using Aurora.Utils;

namespace Aurora.Applications.Application
{
    public class ApplicationBase : ObjectSettings<ApplicationSettings>, IInitialize, IEffectRenderer
    {
        public bool Initialized {get; private set;} = false;

        public ApplicationBase(ApplicationConfig config)
        {
            
        }

        public bool Initialize()
        {
            if (this.Initialized)
                return true;
        
            this.LoadSettings();
            
            return (this.Initialized = true);
        }

        public void Render()
        {
            //Get all layers and render them in parallel then combine the results
        }

        public void ProcessData(object data)
        {
            
        }
        
        public void Dispose()
        {

        }
    }
}