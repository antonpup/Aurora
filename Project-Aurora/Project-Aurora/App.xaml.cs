using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using Aurora.Devices;
using Aurora.Modules;
using Aurora.Profiles;
using Aurora.Settings;
using Aurora.Utils;
using Microsoft.Win32;
using NLog;
using MessageBox = System.Windows.MessageBox;

namespace Aurora;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static readonly Mutex mutex = new(true, "{C88D62B0-DE49-418E-835D-CE213D58444C}");

    public static bool isSilent;
    private static bool _isDelayed;
    private static int _delayTime = 5000;
    private static bool _ignoreUpdate;

    private readonly List<IAuroraModule> _modules = new()
    {
        new UpdateCleanup(),
        new PluginsModule(),
        new LayoutsModule(),
        new InputsModule(),
        new RazerSdkModule(),
        new PointerUpdateModule(),
    };

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (mutex.WaitOne(TimeSpan.Zero, true))
        {
#if DEBUG
                Global.isDebug = true;
#endif
            Global.Initialize();

            PrintSystemInfo();

            UseArgs(e);

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

            Global.Configuration.PropertyChanged += (_, _) => {
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

            var tasks = _modules.ConvertAll(m => m.InitializeAsync());
            Task.WhenAll(tasks).Wait();

            Global.logger.Info("Loading Applications");
            (Global.LightingStateManager = new LightingStateManager()).Initialize();

            Global.logger.Info("Loading Device Manager");
            Global.dev_manager.RegisterVariables();
            Global.dev_manager.InitializeDevices();

            Global.logger.Info("Starting GameStateListener");
            try
            {
                Global.HttpListener = new AuroraHttpListener(9088);
                Global.HttpListener.NewGameState += Global.LightingStateManager.GameStateUpdate;
                
                Global.IpcListener = new IpcListener();
                Global.IpcListener.NewGameState += Global.LightingStateManager.GameStateUpdate;
                Global.IpcListener.WrapperConnectionClosed += Global.LightingStateManager.ResetGameState;
            }
            catch (Exception exc)
            {
                Global.logger.Error("GameStateListener Exception, " + exc);
                MessageBox.Show("GameStateListener Exception.\r\n" + exc);
                Environment.Exit(0);
            }

            if (!Global.HttpListener.Start())
            {
                Global.logger.Error("GameStateListener could not start");
                MessageBox.Show("GameStateListener could not start. Try running this program as Administrator.\r\nExiting.");
                Environment.Exit(0);
            }

            if (!Global.IpcListener.Start())
            {
                Global.logger.Error("GameStateListener could not start");
                MessageBox.Show("GameStateListener could not start. Try running this program as Administrator.\r\nExiting.");
                Environment.Exit(0);
            }

            Global.logger.Info("Listening for game integration calls...");

            Global.logger.Info("Loading ResourceDictionaries...");
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("Themes/MetroDark/MetroDark.MSControls.Core.Implicit.xaml", UriKind.Relative)
            });
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("Themes/MetroDark/MetroDark.MSControls.Toolkit.Implicit.xaml", UriKind.Relative)
            });
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
                MessageBox.Show("Aurora is already running.\r\nExiting.", "Aurora - Error");
                Environment.Exit(0);
            }
        }
    }

    private static void UseArgs(StartupEventArgs e)
    {
        for (var i = 0; i < e.Args.Length; i++)
        {
            var arg = e.Args[i];

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

                    if (i + 1 < e.Args.Length && int.TryParse(e.Args[i + 1], out _delayTime))
                        i++;
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
    }

    private static void PrintSystemInfo()
    {
        StringBuilder systeminfo_sb = new StringBuilder(string.Empty);
        systeminfo_sb.Append("\r\n========================================\r\n");

        try
        {
            var win_reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string productName = (string) win_reg.GetValue("ProductName");

            systeminfo_sb.AppendFormat("Operation System: {0}\r\n", productName);
        }
        catch (Exception exc)
        {
            systeminfo_sb.AppendFormat("Operation System: Could not be retrieved. [Exception: {0}]\r\n", exc.Message);
        }

        systeminfo_sb.AppendFormat("System Architecture: " + (Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit") +
                                   "\r\n");

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
        systeminfo_sb.Append(
            $"Aurora File Version: {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}\r\n");

        systeminfo_sb.Append("========================================\r\n");
        Global.logger.Info(systeminfo_sb.ToString());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        Global.LightingStateManager.SaveAll();

        if (Global.Configuration != null)
            ConfigManager.Save(Global.Configuration);

        var tasks = _modules.ConvertAll(m => m.DisposeAsync());
        Task.WhenAll(tasks).Wait();

        Global.LightingStateManager?.Dispose();
        Global.IpcListener?.Stop().Wait();
        Global.HttpListener?.Stop().Wait();
        Global.dev_manager?.ShutdownDevices();

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
            
        Global.logger.Fatal($"Runtime terminating: {e.IsTerminating}");
        LogManager.Flush();

        if (!Global.Configuration.CloseProgramOnException) return;
        MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
        //Perform exit operations
        Current?.Shutdown();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var exc = e.Exception;
        Global.logger.Fatal(exc, "Fatal Exception caught : " + exc);
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
        string logitech_path = (string)Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary",
            null, null);//null gets the default value
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
        var logitechPath64 = (string)Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary",
            null, null);
        if (logitechPath64 == null || logitechPath64 == @"C:\Program Files\LGHUB\sdk_legacy_led_x64.dll")
        {
            logitechPath64 = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll";

            if (!Directory.Exists(Path.GetDirectoryName(logitechPath64)))
                Directory.CreateDirectory(Path.GetDirectoryName(logitechPath64));

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);

            key.CreateSubKey("Classes");
            key = key.OpenSubKey("Classes", true);

            key.CreateSubKey("CLSID");
            key = key.OpenSubKey("CLSID", true);

            key.CreateSubKey("{a6519e67-7632-4375-afdf-caa889744403}");
            key = key.OpenSubKey("{a6519e67-7632-4375-afdf-caa889744403}", true);

            key.CreateSubKey("ServerBinary");
            key = key.OpenSubKey("ServerBinary", true);

            key.SetValue(null, logitechPath64);
        }

        if (File.Exists(logitechPath64) && !File.Exists(logitechPath64 + ".aurora_backup"))
            File.Move(logitechPath64, logitechPath64 + ".aurora_backup");

        using (BinaryWriter logitechWrapper64 = new BinaryWriter(new FileStream(logitechPath64, FileMode.Create, FileAccess.Write)))
        {
            logitechWrapper64.Write(Aurora.Properties.Resources.Aurora_LogiLEDWrapper64);
        }

        Global.logger.Info("Logitech LED SDK patched successfully");
        MessageBox.Show("Logitech LED SDK patched successfully");
    }
}