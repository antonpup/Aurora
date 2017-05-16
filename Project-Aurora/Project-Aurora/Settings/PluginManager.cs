using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class PluginManager : IInit
    {
        public List<IShortcut> PredefinedShortcuts { get; protected set; } = new List<IShortcut>();

        public bool Initialized { get; protected set; }

        public bool Initialize()
        {
            if (Initialized)
                return true;

            this.CreateDefaults();

            return Initialized = true;
        }

        protected void CreateDefaults()
        {
            this.PredefinedShortcuts.Add(
                new ShortcutNode("Windows") {
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
