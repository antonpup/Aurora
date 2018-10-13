using Aurora.Utils;

namespace Aurora.Devices
{
    public class DeviceManager : IInitialize
    {
        

        public bool Initialized { get; private set; }
        
        public bool Initialize()
        {
            if (this.Initialized)
                return true;
            
            
            return (this.Initialized = true);
        }
        
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}