using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Aurora.Core.Plugins;
using System.Text.Json;
using Aurora.Core.Utils;

namespace Aurora.Core
{
    public class Const
    {
        public const string ScriptDirectory = "Scripts";

        /// <summary>
        /// A boolean indicating if Aurora was started with Debug parameter
        /// </summary>
        public static bool isDebug = false;

        private static string _ExecutingDirectory = "";
        private static string _AppDataDirectory = "";
        private static string _LogsDirectory = "";

        /// <summary>
        /// The path to the application executing directory
        /// </summary>
        public static string ExecutingDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_ExecutingDirectory))
                    _ExecutingDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                return _ExecutingDirectory;
            }
        }

        public static string AppDataDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_AppDataDirectory))
                    _AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora");

                return _AppDataDirectory;
            }
        }

        public static string LogsDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_LogsDirectory))
                    _LogsDirectory = Path.Combine(AppDataDirectory, "Logs");

                return _LogsDirectory;
            }
        }
    }
    public delegate void ManagerUpdateEventHandler(IManager sender, AuroraCore core);
    public delegate void AuroraCoreEventHandler(AuroraCore sender);

    public interface IManager : IDisposable {

        bool Initialized { get; }

        //We may want to consider doing this sequentially

        /// <summary>
        /// Perform operations so the manager is ready to be consumed by others, which shouldn't require any context
        /// </summary>
        /// <returns></returns>
        Task<bool> PreInit(AuroraCore core);

        /// <summary>
        /// Perform Initialisation, using whatever manager context is necessary, this could be called on managers in any arbitrary order, so if any operations require something to be fully initialized, then register to its post init event or the AuroraCore post init event
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        Task<bool> Init(AuroraCore core);

        public event ManagerUpdateEventHandler PostInit;

        event ManagerUpdateEventHandler PreUpdate;

        /// <summary>
        /// Update the manager, called repeatedly, only perform operations in this which are independent from other managers, if they are dependent, then you should bind to the appropriate events on them
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        Task Update(AuroraCore core);

        event ManagerUpdateEventHandler PostUpdate;

        void Save();
    }

    public class AuroraCore
    {
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public PluginManager PluginManager { get; } = new PluginManager();

        protected List<IManager> Managers { get; set; } = new List<IManager>();

        public event AuroraCoreEventHandler PreInit;

        public event AuroraCoreEventHandler PostInit;

        public event AuroraCoreEventHandler PreUpdate;

        public event AuroraCoreEventHandler PostUpdate;

        private static AuroraCore instance;
        public static AuroraCore Instance { get => instance ?? (instance = new AuroraCore()); }

        protected bool initialized = false;
        public bool Initialized { get => initialized;  }

        private AuroraCore()
        {
            //Add base managers
        }

        private class ManagersPluginComponent : IPluginComponent
        {
            private AuroraCore Parent;

            public ManagersPluginComponent(AuroraCore parent)
            {
                this.Parent = parent;
            }

            public string ID => "managers";

            public bool Process(Plugin plugin, JsonElement? properties)
            {
                if (properties == null)
                    return true;

                if (properties?.ValueKind != JsonValueKind.Array)
                {
                    logger.Warn($"Plugin: '{plugin}', does not have a valid properties entry for {this.ID}, it should be an array of Manager Types");
                    return false;
                }

                if (plugin.Assembly == null)
                {
                    logger.Error($"Plugin: '{plugin}' is attempting to load managers but there is no loaded assembly!");
                    return false;
                }
                try
                {
                    foreach (JsonElement managerElem in properties?.EnumerateArray())
                    {
                        if (managerElem.ValueKind == JsonValueKind.String)
                        {
                            string manager = managerElem.GetString();
                            Type managerType = plugin.Assembly.GetTypeFromShortName(manager);
                            if (managerType == null)
                            {
                                logger.Error($"Couldn't find manager '{manager}' in Plugin {plugin}");
                                return false;
                            }

                            if (typeof(IManager).IsAssignableFrom(managerType))
                            {
                                IManager managerInst = (IManager)Activator.CreateInstance(managerType);
                                this.Parent.AddManager(managerInst);
                            }
                            else
                            {
                                logger.Error($"Plugin '{plugin}' has requested to add manager type '{managerType}' but it does not implement {nameof(IManager)}!");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, $"Component '{this.ID}' failed on Plugin '{plugin}'");
#if DEBUG
                    throw exc;
#endif
                    return false;
                }

                return true;
            }
        }

        public async Task<bool> Initialize()
        {
            if (Initialized)
                return true;

            if (!await this.PluginManager.LoadPlugins())
            {
                logger.Error("Failed to initialize core. Plugin Manager failed to load plugins");
                return false;
            }

            this.PluginManager.RunPluginComponent(new ManagersPluginComponent(this));

            logger.Info("AuroraCore: PreInit phase start");
            bool[] preInitResults = await Task.WhenAll(this.Managers.ConvertAll((manager) => manager.PreInit(this)));
            if (!preInitResults.Aggregate((cur, next) => cur && next))
            {
                logger.Error("Failed to initialize core. One or more Managers failed to PreInitialize");
                return false;
            }
            logger.Info("AuroraCore: PreInit phase complete");

            if (!await this.PluginManager.InitPlugins(this))
            {
                logger.Error("Failed to initialize core. Plugin Manager failed to initialise plugins");
                return false;
            }

            PreInit?.Invoke(this);

            logger.Info("AuroraCore: Init phase start");
            bool[] initResults = await Task.WhenAll(this.Managers.ConvertAll((manager) => manager.Init(this)));
            if (!preInitResults.Aggregate((cur, next) => cur && next))
            {
                logger.Error("Failed to initialize core. One or more Managers failed to Initialize");
                return false;
            }
            PostInit?.Invoke(this);
            logger.Info("AuroraCore: Init phase complete");

            return initialized = true;
        }


        public T GetManager<T>() where T : IManager
        {
            return (T)Managers.Find((m) => m is T);
        }

        public void AddManager(IManager manager)
        {
            this.Managers.Add(manager);
            //TODO: PreInit/Init if applicable
        }

        public void AddManagers(IEnumerable<IManager> managers)
        {
            foreach (IManager manager in managers)
            {
                AddManager(manager);
            }
        }
    }
}
