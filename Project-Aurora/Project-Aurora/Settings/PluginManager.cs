using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Data;

namespace Aurora.Settings
{
    public interface IShortcut
    {
        string Title { get; set; }
    }

    public class ShortcutNode : IShortcut
    {
        public string Title { get; set; }

        public List<IShortcut> Children { get; set; } = null;

        public ShortcutNode(string title)
        {
            this.Title = title;
        }

        public Keybind[] GetShortcuts()
        {
            if (Children == null)
                return new Keybind[0];

            List<Keybind> binds = new List<Keybind>();

            foreach (IShortcut shortcut in Children)
            {
                if (shortcut is ShortcutGroup)
                    binds.AddRange(((ShortcutGroup)shortcut).Shortcuts);
                else if (shortcut is ShortcutNode)
                    binds.AddRange(((ShortcutNode)shortcut).GetShortcuts());
            }

            return binds.ToArray();
        }
    }

    public class ShortcutGroup : IShortcut
    {
        public string Title { get; set; }

        public Keybind[] Shortcuts { get; set; } = null;

        public ShortcutGroup(string title)
        {
            this.Title = title;
        }
    }

    public interface IPluginHost
    {
        Dictionary<string, IPlugin> Plugins { get; }
        void SetPluginEnabled(string id, bool enabled);
    }

    public interface IPlugin
    {
        string ID { get; }
        string Title { get; }
        string Author { get; }
        Version Version { get; }
        IPluginHost PluginHost { get; set; }
        void ProcessManager(object manager);
    }

    public static class PluginUtils
    {
        public static bool Enabled(this IPlugin self)
        {
            return self.PluginHost != null;
        }
    }

    public class PluginEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var plugin = (IPlugin)value;
            return plugin.Enabled();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PluginManagerSettings
    {
        public Dictionary<string, bool> PluginManagement { get; private set; } = new Dictionary<string, bool>();

        public PluginManagerSettings()
        {

        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {

        }
    }

    public class PluginManager : ObjectSettings<PluginManagerSettings>, IInit, IPluginHost
    {
        public const string PluginDirectory = "Plugins";

        public Dictionary<string, IPlugin> Plugins { get; set; } = new Dictionary<string, IPlugin>();

        public PluginManager()
        {
            SettingsSavePath = Path.Combine(Global.AppDataDirectory, "PluginSettings.json");
        }

        public bool Initialized { get; protected set; }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            LoadSettings();
            LoadPlugins();

            return Initialized = true;
        }

        public void ProcessManager(object manager)
        {
            foreach (var plugin in this.Plugins)
            {
                try
                {
                    plugin.Value.ProcessManager(manager);
                }
                catch(Exception e)
                {
                    Global.logger.Error(e, "Failed to load plugin {PluginKey}", plugin.Key);
                }
            }
        }

        private void LoadPlugins()
        {
            string installationDir = Path.Combine(Global.ExecutingDirectory, PluginDirectory);
            LoadPlugins(installationDir);
            string userDir = Path.Combine(Global.AppDataDirectory, PluginDirectory);
            LoadPlugins(userDir);
        }

        private void LoadPlugins(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);

                //No need to search the directory if we just created it
                return;
            }

            foreach (string pathPlugin in Directory.EnumerateFiles(dir, "*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    Global.logger.Information("Loading plugin: {PathPlugin}", pathPlugin);
                    Assembly dllPlugin = Assembly.LoadFrom(pathPlugin);

                    foreach (AssemblyName name in dllPlugin.GetReferencedAssemblies())
                        AppDomain.CurrentDomain.Load(name);

                    foreach (Type typ in dllPlugin.GetExportedTypes())
                    {
                        if (!typeof(IPlugin).IsAssignableFrom(typ)) continue;
                        //Create an instance of the plugin type
                        IPlugin objPlugin = (IPlugin)Activator.CreateInstance(typ);

                        //Get the ID of the plugin
                        string id = objPlugin.ID;

                        if (!Settings.PluginManagement.ContainsKey(id) || Settings.PluginManagement[id])
                            objPlugin.PluginHost = this;

                        Plugins.Add(id, objPlugin);
                    }
                }
                catch (Exception exc)
                {
                    Global.logger.Error(exc, "Failed loading plugin {PluginPath}", pathPlugin);
                    if (Global.isDebug)
                        throw;
                }
            }
        }

        public void SetPluginEnabled(string id, bool enabled)
        {
            if (!this.Settings.PluginManagement.ContainsKey(id))
                this.Settings.PluginManagement.Add(id, enabled);
            else
                this.Settings.PluginManagement[id] = enabled;

            this.SaveSettings();
        }

        public void Dispose()
        {

        }
    }
}
