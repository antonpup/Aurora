using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Aurora.Devices;
using Aurora.Profiles;
using Aurora.Settings;
using Aurora.Utils;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32;
using NLog;
using RazerSdkHelper;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using MessageBox = System.Windows.MessageBox;

namespace Aurora
{
    /// <summary>
    /// Globally accessible classes and variables
    /// </summary>
    public static class Global
    {
        public static string ScriptDirectory = "Scripts";
        public static ScriptEngine PythonEngine = Python.CreateEngine();

        /// <summary>
        /// A boolean indicating if Aurora was started with Debug parameter
        /// </summary>
        public static bool isDebug;

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
                    _ExecutingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

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

        /// <summary>
        /// Output logger for errors, warnings, and information
        /// </summary>
        public static NLog.Logger logger;
        //public static Logger logger;

        public static void LogLine(this NLog.Logger logger, string text, Logging_Level level = Logging_Level.None)
        {
            switch (level)
            {
                case Logging_Level.Debug:
                    logger.Debug(text);
                    break;
                case Logging_Level.External:
                case Logging_Level.Error:
                    logger.Error(text);
                    break;
                case Logging_Level.None:
                case Logging_Level.Info:
                    logger.Info(text);
                    break;
                case Logging_Level.Warning:
                    logger.Warn(text);
                    break;
            }
        }

        /// <summary>
        /// Input event subscriptions
        /// </summary>
        public static InputEvents InputEvents;

        //public static GameEventHandler geh;
        public static PluginManager PluginManager;
        public static LightingStateManager LightingStateManager;
        public static NetworkListener net_listener;
        public static Configuration Configuration;
        public static DeviceManager dev_manager;
        public static KeyboardLayoutManager kbLayout;
        public static Effects effengine;
        public static KeyRecorder key_recorder;
        public static RzSdkManager razerSdkManager;

        public static object Clipboard { get; set; }

        public static void Initialize()
        {
            logger = LogManager.GetLogger("global");
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly Mutex mutex = new(true, "{C88D62B0-DE49-418E-835D-CE213D58444C}");
        private static InputInterceptor InputInterceptor;

        public static bool isSilent;
        private static bool _isDelayed;
        private static int _delayTime = 5000;
        private static bool _ignoreUpdate;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
#if DEBUG
                Global.isDebug = true;
#endif
                Global.Initialize();
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.SetCurrentDirectory(path);
                string logiDll = Path.Combine(path, "LogitechLed.dll");
                if (File.Exists(logiDll))
                    File.Delete(logiDll);
                StringBuilder systeminfo_sb = new StringBuilder(string.Empty);
                systeminfo_sb.Append("\r\n========================================\r\n");

                try
                {
                    var win_reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                    string productName = (string)win_reg.GetValue("ProductName");

                    systeminfo_sb.AppendFormat("Operation System: {0}\r\n", productName);
                }
                catch (Exception exc)
                {
                    systeminfo_sb.AppendFormat("Operation System: Could not be retrieved. [Exception: {0}]\r\n", exc.Message);
                }

                systeminfo_sb.AppendFormat("System Architecture: " + (Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit") + "\r\n");

                systeminfo_sb.AppendFormat("Environment OS Version: {0}\r\n", Environment.OSVersion);

                systeminfo_sb.AppendFormat("System Directory: {0}\r\n", Environment.SystemDirectory);
                systeminfo_sb.AppendFormat("Executing Directory: {0}\r\n", Global.ExecutingDirectory);
                systeminfo_sb.AppendFormat("Launch Directory: {0}\r\n", Directory.GetCurrentDirectory());
                systeminfo_sb.AppendFormat("Processor Count: {0}\r\n", Environment.ProcessorCount);

                systeminfo_sb.AppendFormat("SystemPageSize: {0}\r\n", Environment.SystemPageSize);
                systeminfo_sb.AppendFormat("Environment Version: {0}\r\n", Environment.Version);

                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                systeminfo_sb.Append($"Is Elevated: {principal.IsInRole(WindowsBuiltInRole.Administrator)}\r\n");
                systeminfo_sb.Append($"Aurora Assembly Version: {Assembly.GetExecutingAssembly().GetName().Version}\r\n");
                systeminfo_sb.Append($"Aurora File Version: {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}\r\n");

                systeminfo_sb.Append("========================================\r\n");
                Global.logger.Info(systeminfo_sb.ToString());

                string arg;

                for (int arg_i = 0; arg_i < e.Args.Length; arg_i++)
                {
                    arg = e.Args[arg_i];

                    switch (arg)
                    {
                        case "-debug":
                            Global.isDebug = true;
                            Global.logger.Info("Program started in debug mode.");
                            break;
                        case "-silent":
                            isSilent = true;
                            Global.logger.Info("Program started with '-silent' parameter");
                            break;
                        case "-ignore_update":
                            _ignoreUpdate = true;
                            Global.logger.Info("Program started with '-ignore_update' parameter");
                            break;
                        case "-delay":
                            _isDelayed = true;

                            if (arg_i + 1 < e.Args.Length && int.TryParse(e.Args[arg_i + 1], out _delayTime))
                                arg_i++;
                            else
                                _delayTime = 5000;

                            Global.logger.Info("Program started with '-delay' parameter with delay of " + _delayTime + " ms");

                            break;
                        case "-install_logitech":
                            Global.logger.Info("Program started with '-install_logitech' parameter");

                            try
                            {
                                InstallLogitech();
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show("Could not patch Logitech LED SDK. Error: \r\n\r\n" + exc, "Aurora Error");
                            }

                            Environment.Exit(0);
                            break;
                    }
                }

                AppDomain currentDomain = AppDomain.CurrentDomain;
                if (!Global.isDebug)
                    currentDomain.UnhandledException += CurrentDomain_UnhandledException;

                if (_isDelayed)
                    Thread.Sleep(_delayTime);

                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                //AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

                Global.dev_manager = new DeviceManager();
                Global.effengine = new Effects();

                //Load config
                Global.logger.Info("Loading Configuration");
                try
                {
                    Global.Configuration = ConfigManager.Load();
                }
                catch (Exception exc)
                {
                    Global.logger.Error("Exception during ConfigManager.Load(). Error: " + exc);
                    MessageBox.Show("Exception during ConfigManager.Load().Error: " + exc.Message + "\r\n\r\n Default configuration loaded.", "Aurora - Error");

                    Global.Configuration = new Configuration();
                }

                Global.Configuration.PropertyChanged += (sender, eventArgs) => {
                    ConfigManager.Save(Global.Configuration);
                };

                Process.GetCurrentProcess().PriorityClass = Global.Configuration.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;

                if (Global.Configuration.UpdatesCheckOnStartUp && !_ignoreUpdate)
                {
                    string updaterPath = Path.Combine(Global.ExecutingDirectory, "Aurora-Updater.exe");

                    if (File.Exists(updaterPath))
                    {
                        try
                        {
                            ProcessStartInfo updaterProc = new ProcessStartInfo();
                            updaterProc.FileName = updaterPath;
                            updaterProc.Arguments = "-silent";
                            Process.Start(updaterProc);
                        }
                        catch (Exception exc)
                        {
                            Global.logger.Error("Could not start Aurora Updater. Error: " + exc);
                        }
                    }
                }

                Global.logger.Info("Loading Plugins");
                (Global.PluginManager = new PluginManager()).Initialize();

                Global.logger.Info("Loading KB Layouts");
                Global.kbLayout = new KeyboardLayoutManager();
                Global.kbLayout.LoadBrandDefault();

                Global.logger.Info("Loading Input Hooking");
                Global.InputEvents = new InputEvents();
                Global.Configuration.PropertyChanged += SetupVolumeAsBrightness;
                SetupVolumeAsBrightness(Global.Configuration,
                    new PropertyChangedEventArgs(nameof(Global.Configuration.UseVolumeAsBrightness)));
                DesktopUtils.StartSessionWatch();

                Global.key_recorder = new KeyRecorder(Global.InputEvents);

                Global.logger.Info("Loading RazerSdkManager");
                if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
                {
                    try
                    {
                        Global.razerSdkManager = new RzSdkManager
                        {
                            KeyboardEnabled = true,
                            MouseEnabled = true,
                            MousepadEnabled = true,
                            AppListEnabled = true,
                        };

                        Global.razerSdkManager.DataUpdated += RzHelper.OnDataUpdated;

                        var appList = Global.razerSdkManager.GetDataProvider<RzAppListDataProvider>();
                        appList.Update();
                        RzHelper.CurrentAppExecutable = appList.CurrentAppExecutable;

                        Global.logger.Info("RazerSdkManager loaded successfully!");
                    }
                    catch (Exception exc)
                    {
                        Global.logger.Fatal("RazerSdkManager failed to load!");
                        Global.logger.Fatal(exc.ToString());
                    }
                }
                else
                {
                    Global.logger.Warn("Currently installed razer sdk version \"{0}\" is not supported by the RazerSdkManager!", RzHelper.GetSdkVersion());
                }

                Global.logger.Info("Loading Applications");
                (Global.LightingStateManager = new LightingStateManager()).Initialize();

                if (Global.Configuration.GetPointerUpdates)
                {
                    Global.logger.Info("Fetching latest pointers");
                    Task.Run(() => PointerUpdateUtils.FetchDevPointers("master"));
                }

                Global.logger.Info("Loading Device Manager");
                Global.dev_manager.RegisterVariables();
                Global.dev_manager.InitializeDevices();

                /*Global.logger.LogLine("Starting GameEventHandler", Logging_Level.Info);
                Global.geh = new GameEventHandler();
                if (!Global.geh.Init())
                {
                    Global.logger.LogLine("GameEventHander could not initialize", Logging_Level.Error);
                    return;
                }*/

                Global.logger.Info("Starting GameStateListener");
                try
                {
                    Global.net_listener = new NetworkListener(9088);
                    Global.net_listener.NewGameState += Global.LightingStateManager.GameStateUpdate;
                    Global.net_listener.WrapperConnectionClosed += Global.LightingStateManager.ResetGameState;
                }
                catch (Exception exc)
                {
                    Global.logger.Error("GameStateListener Exception, " + exc);
                    MessageBox.Show("GameStateListener Exception.\r\n" + exc);
                    Environment.Exit(0);
                }

                if (!Global.net_listener.Start())
                {
                    Global.logger.Error("GameStateListener could not start");
                    MessageBox.Show("GameStateListener could not start. Try running this program as Administrator.\r\nExiting.");
                    Environment.Exit(0);
                }

                Global.logger.Info("Listening for game integration calls...");

                Global.logger.Info("Loading ResourceDictionaries...");
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/MetroDark/MetroDark.MSControls.Core.Implicit.xaml", UriKind.Relative) });
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Themes/MetroDark/MetroDark.MSControls.Toolkit.Implicit.xaml", UriKind.Relative) });
                Global.logger.Info("Loaded ResourceDictionaries");


                Global.logger.Info("Loading ConfigUI...");

                MainWindow = new ConfigUI();
                Global.LightingStateManager.InitUpdate();
                ((ConfigUI)MainWindow).Display();

                //Debug Windows on Startup
                if (Global.Configuration.BitmapWindowOnStartUp)
                    Window_BitmapView.Open();
                if (Global.Configuration.HttpWindowOnStartUp)
                    Window_GSIHttpDebug.Open();
            }
            else
            {
                try
                {
                    NamedPipeClientStream client = new NamedPipeClientStream(".", "aurora\\interface", PipeDirection.Out);
                    client.Connect(30);
                    if (!client.IsConnected)
                        throw new Exception();
                    byte[] command = Encoding.ASCII.GetBytes("restore");
                    client.Write(command, 0, command.Length);
                    client.Close();
                }
                catch
                {
                    //Global.logger.LogLine("Aurora is already running.", Logging_Level.Error);
                    MessageBox.Show("Aurora is already running.\r\nExiting.", "Aurora - Error");
                }
            }
        }

        private static void SetupVolumeAsBrightness(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(Global.Configuration.UseVolumeAsBrightness))
            {
                if (Global.Configuration.UseVolumeAsBrightness)
                {
                    InputInterceptor = new InputInterceptor();
                    InputInterceptor.Input += InterceptVolumeAsBrightness;
                }
                else if (InputInterceptor != null)
                {
                    InputInterceptor.Input -= InterceptVolumeAsBrightness;
                    InputInterceptor.Dispose();
                }
            }
        }

        private static void InterceptVolumeAsBrightness(object sender, InputInterceptor.InputEventData e)
        {
            var keys = (Keys)e.Data.VirtualKeyCode;
            if ((keys.Equals(Keys.VolumeDown) || keys.Equals(Keys.VolumeUp))
                && Global.InputEvents.Alt)
            {
                e.Intercepted = true;
                Task.Factory.StartNew(() =>
                {
                    if (e.KeyDown)
                    {
                        float brightness = Global.Configuration.GlobalBrightness;
                        brightness += keys == Keys.VolumeUp ? 0.05f : -0.05f;
                        Global.Configuration.GlobalBrightness = Math.Max(0f, Math.Min(1f, brightness));

                        ConfigManager.Save(Global.Configuration);
                    }
                }
                );
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Global.LightingStateManager.SaveAll();
            Global.PluginManager.SaveSettings();

            if (Global.Configuration != null)
                ConfigManager.Save(Global.Configuration);

            Global.key_recorder?.Dispose();
            Global.InputEvents?.Dispose();
            Global.LightingStateManager?.Dispose();
            Global.net_listener?.Stop().Wait();
            Global.dev_manager?.ShutdownDevices();

            try
            {
                Global.razerSdkManager?.Dispose();
                Global.razerSdkManager = null;
            }
            catch (Exception exc)
            {
                Global.logger.Fatal("RazerManager failed to dispose!");
                Global.logger.Fatal(exc.ToString());
            }

            InputInterceptor?.Dispose();

            try
            {
                foreach (Process proc in Process.GetProcessesByName("Aurora-SkypeIntegration"))
                {
                    proc.Kill();
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error("Exception closing \"Aurora-SkypeIntegration\", Exception: " + exc);
            }

            //LogManager.Shutdown();
            Environment.Exit(0);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception)e.ExceptionObject;
            Global.logger.Fatal(exc, "Fatal Exception caught : ");

            if (Current == null)
            {
                return;
            }

            if (exc is SEHException sehException)
            {
                if (sehException.CanResume())
                {
                    return;
                }
            }
            
            Global.logger.Fatal(String.Format("Runtime terminating: {0}", e.IsTerminating));
            LogManager.Flush();

            if (!Global.Configuration.CloseProgramOnException) return;
            MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
            //Perform exit operations
            Current?.Shutdown();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception exc = e.Exception;
            Global.logger.Fatal("Fatal Exception caught : " + exc, exc);
            LogManager.Flush();
            if (!Global.isDebug)
                e.Handled = true;
            else
                throw exc;
            if (Global.Configuration.CloseProgramOnException)
            {
                MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
                //Perform exit operations
                Current?.Shutdown();
            }
        }

        public static void InstallLogitech()
        {
            //Check for Admin
            bool isElevated;
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!isElevated)
            {
                Global.logger.Error("Program does not have admin rights");
                MessageBox.Show("Program does not have admin rights");
                Environment.Exit(1);
            }

            //Patch 32-bit
            string logitech_path = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", null, null);//null gets the default value
            if (logitech_path == null || logitech_path == @"C:\Program Files\LGHUB\sdk_legacy_led_x86.dll")
            {
                logitech_path = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x86\LogitechLed.dll";

                if (!Directory.Exists(Path.GetDirectoryName(logitech_path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(logitech_path));

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

                key.SetValue(null, logitech_path);//null to set the default value
            }

            if (File.Exists(logitech_path) && !File.Exists(logitech_path + ".aurora_backup"))
                File.Move(logitech_path, logitech_path + ".aurora_backup");

            using (BinaryWriter logitech_wrapper_86 = new BinaryWriter(new FileStream(logitech_path, FileMode.Create, FileAccess.Write)))
            {
                logitech_wrapper_86.Write(Aurora.Properties.Resources.Aurora_LogiLEDWrapper86);
            }

            //Patch 64-bit
            string logitech_path_64 = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", null, null);
            if (logitech_path_64 == null || logitech_path_64 == @"C:\Program Files\LGHUB\sdk_legacy_led_x64.dll")
            {
                logitech_path_64 = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll";

                if (!Directory.Exists(Path.GetDirectoryName(logitech_path_64)))
                    Directory.CreateDirectory(Path.GetDirectoryName(logitech_path_64));

                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);

                key.CreateSubKey("Classes");
                key = key.OpenSubKey("Classes", true);

                key.CreateSubKey("CLSID");
                key = key.OpenSubKey("CLSID", true);

                key.CreateSubKey("{a6519e67-7632-4375-afdf-caa889744403}");
                key = key.OpenSubKey("{a6519e67-7632-4375-afdf-caa889744403}", true);

                key.CreateSubKey("ServerBinary");
                key = key.OpenSubKey("ServerBinary", true);

                key.SetValue(null, logitech_path_64);
            }

            if (File.Exists(logitech_path_64) && !File.Exists(logitech_path_64 + ".aurora_backup"))
                File.Move(logitech_path_64, logitech_path_64 + ".aurora_backup");

            using (BinaryWriter logitech_wrapper_64 = new BinaryWriter(new FileStream(logitech_path_64, FileMode.Create, FileAccess.Write)))
            {
                logitech_wrapper_64.Write(Aurora.Properties.Resources.Aurora_LogiLEDWrapper64);
            }

            Global.logger.Info("Logitech LED SDK patched successfully");
            MessageBox.Show("Logitech LED SDK patched successfully");

            //Environment.Exit(0);
        }
    }
}