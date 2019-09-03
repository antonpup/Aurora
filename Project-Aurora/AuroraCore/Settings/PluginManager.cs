using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;

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
    public class PluginManagerSettings : SettingsBase
    {
        //TODO: Convert this to correct format for Settings
        public Dictionary<string, bool> PluginManagement { get; private set; } = new Dictionary<string, bool>();

        public PluginManagerSettings()
        {

        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {

        }
    }

    public class PluginManager : ObjectSettings<PluginManagerSettings>, IPluginHost //IInit
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public List<IShortcut> PredefinedShortcuts { get; protected set; } = new List<IShortcut>();

        public const string PluginDirectory = "Plugins";

        public Dictionary<string, IPlugin> Plugins { get; set; } = new Dictionary<string, IPlugin>();

        public PluginManager() : base()
        {
            this.CreateDefaults();
        }

        public bool Initialized { get; protected set; }

        public bool Initialize(IManager manager)
        {
            if (Initialized)
                return true;

            this.LoadSettings();
            this.LoadPlugins(manager);

            return Initialized = true;
        }

        public void ProcessManager(object manager)
        {
            foreach (var plugin in this.Plugins)
            {
                plugin.Value.ProcessManager(manager);
            }
        }

        private void LoadPlugins(IManager manager)
        {
            string dir = Path.Combine(Const.ExecutingDirectory, PluginDirectory);
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
                    Assembly dllPlugin = Assembly.LoadFrom(pathPlugin);

                    foreach (AssemblyName name in dllPlugin.GetReferencedAssemblies())
                        AppDomain.CurrentDomain.Load(name);

                    manager.AcceptPlugins(dllPlugin.GetExportedTypes().ToList());

                    /*foreach (Type typ in dllPlugin.GetExportedTypes())
                    {
                        if (typeof(IPlugin).IsAssignableFrom(typ))
                        {
                            //Create an instance of the plugin type
                            IPlugin objPlugin = (IPlugin)Activator.CreateInstance(typ);

                            //Get the ID of the plugin
                            string id = objPlugin.ID;

                            if (id != null && (!Settings.PluginManagement.ContainsKey(id) || Settings.PluginManagement[id]))
                                objPlugin.PluginHost = this;

                            this.Plugins.Add(id, objPlugin);
                        }
                    }*/
                }
                catch (Exception exc)
                {
                    logger.Error(exc.ToString());
                    if (Const.isDebug)
                        throw exc;
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

        protected void CreateDefaults()
        {
            this.PredefinedShortcuts.Add(
                new ShortcutNode("Windows")
                {
                    Children = new List<IShortcut> {
                        new ShortcutGroup("Ctrl"){
                            Shortcuts = new Keybind[]
                            {
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.X }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.C }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.V }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.Z }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.F4 }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.A }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.D }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.R }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.Y }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.Right }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.Left }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.Down }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.Up }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.LMenu, Keys.Tab }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.Up }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.Down }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.Left }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.Right }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.Escape }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.LShiftKey, Keys.Escape }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.Escape }),
                                new Keybind( new Keys[] { Keys.LControlKey, Keys.F })
                            }
                        },
                        new ShortcutGroup("Win"){
                            Shortcuts = new Keybind[]
                            {
                                new Keybind( new Keys[] { Keys.LWin, Keys.L }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.D }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.B }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.A }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.LMenu, Keys.D }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.E }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.G }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.I }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.M }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.P }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.R }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.S }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.Up }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.Down }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.Left }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.Right }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.Home }),
                                new Keybind( new Keys[] { Keys.LWin, Keys.D })
                            }
                        },
                        new ShortcutGroup("Alt"){
                            Shortcuts = new Keybind[]
                            {
                                new Keybind( new Keys[] { Keys.LMenu, Keys.Tab }),
                                new Keybind( new Keys[] { Keys.LMenu, Keys.F4 }),
                                new Keybind( new Keys[] { Keys.LMenu, Keys.Space }),
                                new Keybind( new Keys[] { Keys.LMenu, Keys.Left }),
                                new Keybind( new Keys[] { Keys.LMenu, Keys.Right }),
                                new Keybind( new Keys[] { Keys.LMenu, Keys.PageUp }),
                                new Keybind( new Keys[] { Keys.LMenu, Keys.PageDown }),
                                new Keybind( new Keys[] { Keys.LMenu, Keys.Tab })
                            }
                        }
                    }
                }
            );
        }

        public void Dispose()
        {

        }
    }
}
