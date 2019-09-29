using Aurora.Core;
using Aurora.Core.Plugins;
using System;
using System.Threading.Tasks;

namespace TestPlugin
{
    public class TestManager : IManager
    {
        public bool Initialized { get; protected set; } = false;

        public event ManagerUpdateEventHandler PostInit;
        public event ManagerUpdateEventHandler PreUpdate;
        public event ManagerUpdateEventHandler PostUpdate;

        public void Dispose()
        {
        }

        public async Task<bool> Init(AuroraCore core)
        {
            Console.WriteLine("TestManager Init called");
            return (Initialized = true);
        }

        public async Task<bool> PreInit(AuroraCore core)
        {
            Console.WriteLine("TestManager PreInit called");
            return true;
        }

        public void Save()
        {
        }

        public async Task Update(AuroraCore core)
        {
        }
    }

    public static class PluginEntry
    {
        public static bool OnLoad(AuroraCore core)
        {
            Console.WriteLine("TestPlugin OnLoad called");
            
            return true;
        }

        public static bool Init(AuroraCore core)
        {
            Console.WriteLine("TestPlugin Init called");

            return true;
        }
    }
}
