using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings
{
    public interface IManager : IInit
    {
        void Save();
        void AcceptPlugins(List<Type> plugin);
    }

    public abstract class ManagerSettings<T> : ObjectSettings<T>, IManager where T : SettingsBase
    {
        public ManagerSettings() : base()
        {

        }
        public ManagerSettings(string prefix) : base(prefix)
        {

        }

        public bool Initialized { get; protected set; } = false;

        public virtual bool Initialize()
        {
            this.LoadSettings();
            return true;
        }
        public virtual void Save()
        {
            this.SaveSettings();
        }

        public abstract void Dispose();
        public abstract void AcceptPlugins(List<Type> plugin);
    }
}
