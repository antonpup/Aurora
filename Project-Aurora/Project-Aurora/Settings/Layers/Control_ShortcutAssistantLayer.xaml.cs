using Aurora.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aurora.Settings.Layers
{
    /// <summary>
    /// Interaction logic for Control_ShortcutAssistantLayer.xaml
    /// </summary>
    public partial class Control_ShortcutAssistantLayer : UserControl
    {
        private Button buttonAddNewShortcut = new Button() {
            Content = "New shortcut",
            HorizontalAlignment = HorizontalAlignment.Center
        };

        protected Control_ShortcutAssistantLayer()
        {
            InitializeComponent();

            this.PopulatePresets();

            buttonAddNewShortcut.Click += ButtonAddNewShortcut_Click;

            this.Loaded += (obj, e) => { this.SetSettings(); };
        }

        public Control_ShortcutAssistantLayer(ShortcutAssistantLayerHandler datacontext) : this()
        {
            this.DataContext = datacontext;
        }

        public void SetSettings()
        {
            this.comboboxPresentationType.SelectedItem = ((ShortcutAssistantLayerHandler)this.DataContext).Properties._PresentationType;

            this.stackPanelShortcuts.Children.Clear();
            foreach(Keybind keyb in ((ShortcutAssistantLayerHandler)this.DataContext).Properties._ShortcutKeys)
            {
                AddKeybind(keyb);
            }
            AddStackPanelButton();
        }
        private void PopulatePresets(MenuItem item = null, ShortcutNode node = null)
        {
            MenuItem currentItem = item ?? menuPresets;
            foreach (IShortcut shortcut in node?.Children ?? Global.PluginManager.PredefinedShortcuts)
            {
                MenuItem newItem = new MenuItem { Header = shortcut.Title, Tag = shortcut };
                newItem.Click += ShortcutPresetClick;
                if (shortcut is ShortcutNode)
                    PopulatePresets(newItem, (ShortcutNode)shortcut);
                currentItem.Items.Add(newItem);
            }
        }

        private void ShortcutPresetClick(object sender, RoutedEventArgs e)
        {
            IShortcut shortcut;
            if ((shortcut = (e.OriginalSource as MenuItem)?.Tag as IShortcut) != null)
            {
                ShortcutAssistantLayerHandler layer = (ShortcutAssistantLayerHandler)this.DataContext;
                if (shortcut is ShortcutNode)
                    layer.Properties._ShortcutKeys = ((ShortcutNode)shortcut).GetShortcuts();
                else if (shortcut is ShortcutGroup)
                    layer.Properties._ShortcutKeys = ((ShortcutGroup)shortcut).Shortcuts;
                this.SetSettings();
                e.Handled = true;
            }
        }

        private void ButtonRemoveKeybind_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button && (sender as Button).Tag is DockPanel)
            {
                this.stackPanelShortcuts.Children.Remove((sender as Button).Tag as DockPanel);
                ApplySettings();
            }
        }

        private void ButtonAddNewShortcut_Click(object sender, RoutedEventArgs e)
        {
            this.stackPanelShortcuts.Children.Remove(buttonAddNewShortcut);

            AddKeybind(new Keybind());

            AddStackPanelButton();
        }

        private void AddKeybind(Keybind keyb)
        {
            Control_Keybind keybindEditor = new Control_Keybind();
            keybindEditor.ContextKeybind = keyb;
            keybindEditor.VerticalAlignment = VerticalAlignment.Stretch;
            keybindEditor.KeybindUpdated += KeybindEditor_KeybindUpdated;

            DockPanel dp = new DockPanel()
            {
                LastChildFill = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Tag = keybindEditor
            };

            Button ButtonRemoveKeybind = new Button()
            {
                Content = "X",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = dp
            };
            DockPanel.SetDock(ButtonRemoveKeybind, Dock.Right);
            ButtonRemoveKeybind.Click += ButtonRemoveKeybind_Click;


            dp.Children.Add(ButtonRemoveKeybind);
            dp.Children.Add(keybindEditor);

            this.stackPanelShortcuts.Children.Add(dp);
        }

        public void AddStackPanelButton()
        {
            this.stackPanelShortcuts.Children.Add(buttonAddNewShortcut);
        }

        private void KeybindEditor_KeybindUpdated(object sender, Keybind newKeybind)
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            List<Keybind> newShortcuts = new List<Keybind>();

            foreach (var child in this.stackPanelShortcuts.Children)
            {
                if (child is DockPanel && (child as DockPanel).Tag is Control_Keybind)
                    newShortcuts.Add(((child as DockPanel).Tag as Control_Keybind).ContextKeybind);
            }

            ((ShortcutAssistantLayerHandler)this.DataContext).Properties._ShortcutKeys = newShortcuts.ToArray();
        }

        private void keysMain_SequenceUpdated(object sender, EventArgs e)
        {
            ((ShortcutAssistantLayerHandler)this.DataContext).Properties._Sequence = ((Controls.KeySequence)sender).Sequence;
        }

        private void cmbModifier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0) {
                this.SetSettings();
            }
        }

        private void comboboxPresentationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && this.DataContext is ShortcutAssistantLayerHandler && sender is ComboBox)
                ((ShortcutAssistantLayerHandler)this.DataContext).Properties._PresentationType = (ShortcutAssistantPresentationType)Enum.Parse(typeof(ShortcutAssistantPresentationType), (sender as ComboBox).SelectedIndex.ToString());
        }
    }
}
