using Aurora.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Aurora.Core.Utils;

namespace Aurora.Core.Plugins
{
    public class Plugin
    {
        public const string PluginMetadataFile = "metadata.json";

        public string PluginDirectory { get; }

        public bool ProcessingSuccessful { get; set; } = true;

        public string ID { get; }

        public string Name { get; }

        public string Author { get; }

        public string ProjectSite { get; }

        public string Version { get; }

        public string AuroraVersion { get; }

        public Dictionary<string, JsonElement> Components { get; }

        public Assembly Assembly { get; set; }

        public Plugin(string pluginDirectory)
        {
            PluginDirectory = pluginDirectory;

            string metadataJson = File.ReadAllText(Path.Combine(pluginDirectory, PluginMetadataFile));
            JsonDocument json = JsonDocument.Parse(metadataJson);
            var root = json.RootElement;
            ID = root.GetProperty("id").GetString();
            Name = root.GetProperty("name").GetString();
            Author = root.GetProperty("author").GetString();
            ProjectSite = root.GetProperty("project-site").GetString();
            Version = root.GetProperty("version").GetString();
            AuroraVersion = root.GetProperty("aurora-version").GetString();
            if (root.TryGetProperty("components", out JsonElement elem))
                Components = elem.EnumerateObject().ToDictionary((prop) => prop.Name, (prop) => prop.Value);
            else
                Components = new Dictionary<string, JsonElement>();
        }
    }

    public interface IPluginComponent
    {
        /// <summary>
        /// The ID of this plugin, used the specify the additional properties for this Component in the Plugin's metadata file
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Process the given plugin
        /// </summary>
        /// <param name="plugin">The plugin to process</param>
        /// <param name="properties">The properties specified for this component in the Plugin's metadata json</param>
        /// <returns></returns>
        bool Process(Plugin plugin, JsonElement? properties);
    }

    public class AssemblyPluginComponent : IPluginComponent
    {
        public const string DefaultDLLName = "plugin.dll";

        public string ID => "assembly";

        public bool Process(Plugin plugin, JsonElement? properties)
        {
            string assemblyName = properties?.TryGetString() ?? DefaultDLLName;
            string assemblyPath = Path.Combine(plugin.PluginDirectory, assemblyName);

            if (File.Exists(assemblyPath))
            {
                plugin.Assembly = Assembly.LoadFrom(assemblyPath);
                //foreach (AssemblyName name in dllPlugin.GetReferencedAssemblies())
                //  AppDomain.CurrentDomain.Load(name);
                return true;
            }
            else if (!assemblyName.Equals(DefaultDLLName))
            {
                return false;
            }

            return true;

        }
    }

    public class PluginManagerSettings : SettingsBase
    {
        //TODO: Convert this to correct format for Settings
        public Dictionary<string, bool> PluginManagement { get; private set; } = new Dictionary<string, bool>();


        public PluginManagerSettings()
        {

        }

        public bool PluginActive(string name)
        {
            return PluginManagement.ContainsKey(name) && PluginManagement[name];
        }

    }

    public delegate void PluginManagerEventHandler(PluginManager sender);

    public class PluginManager : ObjectSettings<PluginManagerSettings>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Dictionary<string, Plugin> Plugins { get; protected set; } = new Dictionary<string, Plugin>();

        public IEnumerable<Plugin> ActivePlugins
        {
            get
            {
                foreach (Plugin plugin in Plugins.Values)
                {
                    if (this.Settings.PluginActive(plugin.ID))
                        continue;
                    yield return plugin;
                }
            }
        }

        public IEnumerable<Plugin> NewInactivePlugins
        {
            get
            {
                foreach (Plugin plugin in Plugins.Values)
                {
                    //If the plugin ID is not present in the Plugin list then it hasn't yet been enabled/disabled so is new
                    if (!this.Settings.PluginManagement.ContainsKey(plugin.ID))
                        yield return plugin;
                }
            }
        }

        public List<IPluginComponent> PluginComponents { get; protected set; } = new List<IPluginComponent>();

        public bool Initialized { get; protected set; } = false;
        public const string PluginDirectory = "Plugins";

        public event PluginManagerEventHandler PreLoadPlugins;
        public event PluginManagerEventHandler PostLoadPlugins;
        public event PluginManagerEventHandler PreInit;
        public event PluginManagerEventHandler PostInit;

        public PluginManager() : base()
        {

        }

        public async Task<bool> LoadPlugins()
        {
            logger.Info("Loading Plugins");

            this.LoadSettings();

            PreLoadPlugins?.Invoke(this);

            string dir = Path.Combine(Const.ExecutingDirectory, PluginDirectory);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);

                //No need to search the directory if we just created it
                return (Initialized = true);
            }

            foreach (string pluginPath in Directory.EnumerateDirectories(dir))
            {
                if (File.Exists(Path.Combine(pluginPath, Plugin.PluginMetadataFile)))
                {
                    try
                    {
                        Plugin plugin = new Plugin(pluginPath);
                        if (!this.Plugins.ContainsKey(plugin.ID))
                        {
                            this.Plugins.Add(plugin.ID, plugin);
                        }
                        else
                        {
                            throw new Exception($"Plugin with ID {plugin.ID} already exists!");
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, $"Failed to load plugin {pluginPath}");
#if DEBUG
                        throw exc;
#endif
                    }
                }
                else
                {
                    logger.Warn($"Plugin directory '{pluginPath}' does not have a {Plugin.PluginMetadataFile} file");
                }
            }

            this.RunPluginComponent(new AssemblyPluginComponent());

            this.RunPluginComponent(new EntryPointPluginComponent("OnLoad"));
            this.AddPluginComponent(new EntryPointPluginComponent("Init"));

            PostLoadPlugins?.Invoke(this);
            logger.Info("Finished Loading Plugins");
            return true;
        }

        public void AddPluginComponent(IPluginComponent component)
        {
            this.PluginComponents.Add(component);
        }

        public bool RunPluginComponent(params IPluginComponent[] components)
        {
            //TODO: Handle bool return error

            foreach (Plugin plugin in this.ActivePlugins)
            {
                if (!plugin.ProcessingSuccessful)
                    continue;

                foreach (IPluginComponent component in components)
                {
                    JsonElement? properties = null;
                    //Check if the component should be skipped because the plugin has disabled it
                    if (plugin.Components.TryGetValue(component.ID, out JsonElement elem))
                    {
                        if (elem.IsFalse())
                            continue;

                        properties = elem;
                    }

                    if (!component.Process(plugin, properties))
                    {
                        logger.Error($"Plugin: '{plugin}' failed loading at component {component}");
                        plugin.ProcessingSuccessful = false;
                    }
                }
            }


            return true;
        }

        public async Task<bool> InitPlugins(AuroraCore core)
        {
            PreInit?.Invoke(this);

            logger.Info("Initialising Plugins");
            bool ret = (Initialized = this.RunPluginComponent(this.PluginComponents.ToArray()));
            PostInit?.Invoke(this);
            return ret;
        }

        public void Dispose()
        {

        }

        public void Save()
        {
            this.SaveSettings();
        }

        
        internal class EntryPointPluginComponent : IPluginComponent
        {
            public const string EntryClassName = "PluginEntry";

            public string ID => "entry-point";

            public string MethodCall { get; protected set; }

            public EntryPointPluginComponent(string methodCall)
            {
                MethodCall = methodCall;
            }

            public bool Process(Plugin plugin, JsonElement? properties)
            {
                if (plugin.Assembly == null)
                    return true;

                try
                {
                    Type entry = plugin.Assembly.GetTypeFromShortName(EntryClassName);
                    if (entry == null)
                    {
                        logger.Error($"Could not find Entry class '{EntryClassName}' on Plugin: '{plugin}'");
                        return true;
                    }
                    MethodInfo method = entry.GetMethod(MethodCall);
                    if (method.IsStatic)
                    {
                        method.Invoke(null, new object[] { AuroraCore.Instance });
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, $"Plugin '{plugin}' failed to execute method '{MethodCall}'");
#if DEBUG
                    throw exc;
#endif
                }

                return true;
            }
        }
    }
}
