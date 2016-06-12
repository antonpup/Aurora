using Aurora.Devices;
using Aurora.Settings;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Forms;

namespace Aurora
{
    public static class Global
    {
        public static bool isDebug = false;
        public static Logger logger = new Logger();
        public static GameEventHandler geh;
        public static NetworkListener net_listener;
        public static Configuration Configuration { get; set; }
        public static DeviceManager dev_manager = new DeviceManager();
        public static KeyboardLayoutManager kbLayout;
        public static Effects effengine = new Effects();
        public static KeyRecorder key_recorder = new KeyRecorder();
        public static IKeyboardMouseEvents input_hook = Hook.GlobalEvents();
        public static Keys held_modified = Keys.None;
    }

    static class Program
    {
        public static System.Windows.Application WinApp { get; private set; }
        public static Window MainWindow;

        public static bool isSilent = false;
        private static bool isDelayed = false;
        private static int delayTime = 5000;
        private static bool ignore_update = false;

        [STAThread]
        static void Main(string[] args)
        {
            string arg = "";

            for (int arg_i = 0; arg_i < args.Length; arg_i++)
            {
                arg = args[arg_i];

                switch (arg)
                {
                    case ("-debug"):
                        Global.isDebug = true;
                        Global.logger.LogLine("Program started in debug mode.", Logging_Level.Info);
                        break;
                    case ("-silent"):
                        isSilent = true;
                        Global.logger.LogLine("Program started with '-silent' parameter", Logging_Level.Info);
                        break;
                    case ("-ignore_update"):
                        ignore_update = true;
                        Global.logger.LogLine("Program started with '-ignore_update' parameter", Logging_Level.Info);
                        break;
                    case ("-delay"):
                        isDelayed = true;

                        if (arg_i + 1 < args.Length && int.TryParse(args[arg_i + 1], out delayTime))
                            arg_i++;
                        else
                            delayTime = 5000;

                        Global.logger.LogLine("Program started with '-delay' parameter with delay of " + delayTime + " ms", Logging_Level.Info);

                        break;
                    case ("-install_logitech"):
                        Global.logger.LogLine("Program started with '-install_logitech' parameter", Logging_Level.Info);

                        try
                        {
                            InstallLogitech();
                        }
                        catch (Exception exc)
                        {
                            System.Windows.MessageBox.Show("Could not patch Logitech LED SDK. Error: \r\n\r\n" + exc, "Aurora Error");
                        }

                        Environment.Exit(0);
                        break;
                }
            }

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            /*
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Global.logger.LogLine("Aurora is already running.", Logging_Level.Error);
                System.Windows.MessageBox.Show("Aurora is already running.\r\nExiting.", "Aurora - Error");
                Environment.Exit(0);
            }
            */

            if (isDelayed)
                System.Threading.Thread.Sleep((int)delayTime);

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            //Load config
            try
            {
                Global.Configuration = ConfigManager.Load();
            }
            catch (Exception e)
            {
                Global.logger.LogLine("Exception during ConfigManager.Load(). Error: " + e, Logging_Level.Error);
                System.Windows.MessageBox.Show("Exception during ConfigManager.Load().Error: " + e.Message, "Aurora - Error");

                Global.Configuration = new Configuration();
            }

            if (Global.Configuration.updates_check_on_start_up && !ignore_update)
            {
                string updater_path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "Aurora-Updater.exe");

                if (File.Exists(updater_path))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = updater_path;
                    startInfo.Arguments = Global.Configuration.updates_allow_silent_minor ? "-silent_minor -silent" : "-silent";
                    Process.Start(startInfo);
                }
            }

            Global.dev_manager.Initialize();
            Devices.Device[] active_devices = Global.dev_manager.GetInitializedDevices();

            if (active_devices.Length == 0)
            {
                Global.logger.LogLine("No devices were initialized", Logging_Level.Error);
                System.Windows.MessageBox.Show("No compatible devices detected.\r\nExiting.", "Aurora - Error");
                Environment.Exit(0);
            }

            if (Global.Configuration.keyboard_brand == PreferredKeyboard.Logitech)
                Global.kbLayout = new KeyboardLayoutManager(KeyboardBrand.Logitech);
            else if (Global.Configuration.keyboard_brand == PreferredKeyboard.Corsair)
                Global.kbLayout = new KeyboardLayoutManager(KeyboardBrand.Corsair);
            else if (Global.Configuration.keyboard_brand == PreferredKeyboard.Razer)
                Global.kbLayout = new KeyboardLayoutManager(KeyboardBrand.Razer);
            else
            {
                Global.kbLayout = new KeyboardLayoutManager(KeyboardBrand.Logitech);

                foreach (var device in active_devices)
                {
                    if (!device.IsKeyboardConnected())
                        continue;

                    switch (device.GetDeviceName())
                    {
                        case ("Corsair"):
                            Global.kbLayout = new KeyboardLayoutManager(KeyboardBrand.Corsair);
                            break;

                        /*
                        case ("Razer"):
                            Global.kbLayout = new KeyboardLayoutManager(KeyboardBrand.Razer);
                            break;
                        */
                        default:
                            continue;
                    }
                }
            }

            Global.input_hook.KeyDown += InputHookKeyDown;
            Global.input_hook.KeyUp += InputHookKeyUp;

            Global.geh = new GameEventHandler();
            if (!Global.geh.Init())
            {
                Global.logger.LogLine("GameEventHander could not initialize", Logging_Level.Error);
                return;
            }

            Global.net_listener = new NetworkListener(9088);
            Global.net_listener.NewGameState += new NewGameStateHandler(Global.geh.GameStateUpdate);

            if (!Global.net_listener.Start())
            {
                Global.logger.LogLine("GameStateListener could not start", Logging_Level.Error);
                System.Windows.MessageBox.Show("GameStateListener could not start. Try running this program as Administrator.\r\nExiting.");
                Environment.Exit(0);
            }

            Global.logger.LogLine("Listening for game integration calls...", Logging_Level.None);

            WinApp = new System.Windows.Application();
            ResourceDictionary resourceDictionaryCore = new ResourceDictionary();
            resourceDictionaryCore.Source = new Uri("Themes/MetroDark/MetroDark.MSControls.Core.Implicit.xaml", UriKind.Relative);
            ResourceDictionary resourceDictionaryToolkit = new ResourceDictionary();
            resourceDictionaryToolkit.Source = new Uri("Themes/MetroDark/MetroDark.MSControls.Toolkit.Implicit.xaml", UriKind.Relative);

            WinApp.Resources.MergedDictionaries.Add(resourceDictionaryCore);
            WinApp.Resources.MergedDictionaries.Add(resourceDictionaryToolkit);

            WinApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            MainWindow = new ConfigUI();
            WinApp.MainWindow = MainWindow;
            WinApp.Run(MainWindow);

            ConfigManager.Save(Global.Configuration);

            Global.geh.Destroy();
            Global.net_listener.Stop();

            Environment.Exit(0);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception)e.ExceptionObject;
            Global.logger.LogLine("Fatal Exception caught : " + exc, Logging_Level.Error);
            Global.logger.LogLine(String.Format("Runtime terminating: {0}", e.IsTerminating), Logging_Level.Error);

            System.Windows.MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
        }

        private static void InstallLogitech()
        {
            //Check for Admin
            bool isElevated;
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!isElevated)
            {
                Global.logger.LogLine("Program does not have admin rights", Logging_Level.Error);
                System.Windows.MessageBox.Show("Program does not have admin rights");
                Environment.Exit(1);
            }

            //Patch 32-bit
            string logitech_path = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", "(Default)", null);
            if (logitech_path == null)
            {
                logitech_path = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x86\LogitechLed.dll";

                if (!Directory.Exists(Path.GetDirectoryName(logitech_path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logitech_path));
                }

                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);

                key.CreateSubKey("Classes");
                key = key.OpenSubKey("Classes", true);

                key.CreateSubKey("WOW6432Node");
                key = key.OpenSubKey("WOW6432Node", true);

                key.CreateSubKey("CLSID");
                key = key.OpenSubKey("CLSID", true);

                key.CreateSubKey("{a6519e67-7632-4375-afdf-caa889744403}");
                key = key.OpenSubKey("{a6519e67-7632-4375-afdf-caa889744403}", true);

                key.CreateSubKey("ServerBinary");
                key = key.OpenSubKey("ServerBinary", true);

                key.SetValue("(Default)", logitech_path);
            }

            if (!File.Exists(logitech_path + ".aurora_backup"))
                File.Move(logitech_path, logitech_path + ".aurora_backup");

            using (BinaryWriter logitech_wrapper_86 = new BinaryWriter(new FileStream(logitech_path, FileMode.Create, FileAccess.Write)))
            {
                logitech_wrapper_86.Write(Properties.Resources.Aurora_LogiLEDWrapper86);
            }

            //Patch 64-bit
            string logitech_path_64 = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", "(Default)", null);
            if (logitech_path_64 == null)
            {
                logitech_path_64 = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll";

                if (!Directory.Exists(Path.GetDirectoryName(logitech_path_64)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logitech_path_64));
                }

                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);

                key.CreateSubKey("Classes");
                key = key.OpenSubKey("Classes", true);

                key.CreateSubKey("CLSID");
                key = key.OpenSubKey("CLSID", true);

                key.CreateSubKey("{a6519e67-7632-4375-afdf-caa889744403}");
                key = key.OpenSubKey("{a6519e67-7632-4375-afdf-caa889744403}", true);

                key.CreateSubKey("ServerBinary");
                key = key.OpenSubKey("ServerBinary", true);

                key.SetValue("(Default)", logitech_path_64);
            }

            if (!File.Exists(logitech_path_64 + ".aurora_backup"))
                File.Move(logitech_path_64, logitech_path_64 + ".aurora_backup");

            using (BinaryWriter logitech_wrapper_64 = new BinaryWriter(new FileStream(logitech_path_64, FileMode.Create, FileAccess.Write)))
            {
                logitech_wrapper_64.Write(Properties.Resources.Aurora_LogiLEDWrapper64);
            }

            Global.logger.LogLine("Logitech LED SDK patched successfully", Logging_Level.Info);
            System.Windows.MessageBox.Show("Logitech LED SDK patched successfully");

            Environment.Exit(0);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                if (Global.net_listener != null)
                    Global.net_listener.Stop();

                if (Global.dev_manager != null)
                    Global.dev_manager.Shutdown();

                if (Global.Configuration != null)
                    ConfigManager.Save(Global.Configuration);
            }
            catch(Exception exc)
            {
                Global.logger.LogLine("Exception during OnProcessExit(). Error: " + exc, Logging_Level.Error);
            }
        }

        private static void InputHookKeyDown(object sender, KeyEventArgs e)
        {
            //Handle Assistant
            if ((e.KeyCode == Keys.LMenu || e.KeyCode == Keys.RMenu || e.KeyCode == Keys.LControlKey || e.KeyCode == Keys.RControlKey || e.KeyCode == Keys.RWin || e.KeyCode == Keys.LWin) && Global.held_modified == Keys.None)
                Global.held_modified = e.KeyCode;

            //Handle Volume Overlay
            if ((e.KeyCode == Keys.VolumeUp || e.KeyCode == Keys.VolumeDown) && e.Modifiers == Keys.Alt && Global.Configuration.use_volume_as_brightness)
            {
                e.Handled = true;

                if (e.KeyCode == Keys.VolumeUp)
                    Global.Configuration.global_brightness = Global.Configuration.global_brightness + 0.05f > 1.0f ? 1.0f : Global.Configuration.global_brightness + 0.05f;
                else if (e.KeyCode == Keys.VolumeDown)
                    Global.Configuration.global_brightness = Global.Configuration.global_brightness - 0.05f < 0.0f ? 0.0f : Global.Configuration.global_brightness - 0.05f;

                ConfigManager.Save(Global.Configuration);

                return;
            }
            else if (e.KeyCode == Keys.VolumeUp || e.KeyCode == Keys.VolumeDown)
            {
                Global.geh.AddOverlayForDuration(new Profiles.Overlays.Event_VolumeOverlay(), Global.Configuration.volume_overlay_settings.delay * 1000);
            }
        }

        private static void InputHookKeyUp(object sender, KeyEventArgs e)
        {
            //Handle Assistant
            if (Global.held_modified == e.KeyCode)
                Global.held_modified = Keys.None;
        }
    }
}
