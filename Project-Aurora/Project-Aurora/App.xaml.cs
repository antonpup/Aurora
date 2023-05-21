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
    private static readonly Mutex Mutex = new(true, "{C88D62B0-DE49-418E-835D-CE213D58444C}");

    public static bool IsSilent { get; private set; }
    private bool _isDelayed;
    private int _delayTime = 5000;
    private bool _ignoreUpdate;
    private bool _closing;

    private static readonly PluginsModule PluginsModule = new();
    private static readonly IpcListenerModule IpcListenerModule = new();
    private static readonly HttpListenerModule HttpListenerModule = new();
    private static readonly RazerSdkModule RazerSdkModule = new();
    private static readonly LayoutsModule LayoutsModule = new();

    private readonly List<IAuroraModule> _modules = new()
    {
        new UpdateCleanup(),
        new InputsModule(),
        new PointerUpdateModule(),
        new MediaInfoModule(),
        new AudioCaptureModule(),
        PluginsModule,
        IpcListenerModule,
        HttpListenerModule,
        new LightningStateManagerModule(PluginsModule.PluginManager, IpcListenerModule.IpcListener, HttpListenerModule.HttpListener),
        RazerSdkModule,
        LayoutsModule,
        new HardwareMonitorModule(),
    };

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        if (Mutex.WaitOne(TimeSpan.Zero, true))
        {
#if DEBUG
            Global.isDebug = true;
#endif
            Global.Initialize();
            new UserSettingsBackup().BackupIfNew();
            PrintSystemInfo();
            UseArgs(e);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            if (!Global.isDebug)
                currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (_isDelayed)
                Thread.Sleep(_delayTime);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

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

            Global.Configuration.PropertyChanged += (_, _) =>
            {
                ConfigManager.Save(Global.Configuration);
            };

            Process.GetCurrentProcess().PriorityClass = Global.Configuration.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;

            if (Global.Configuration.UpdatesCheckOnStartUp && !_ignoreUpdate)
            {
                DesktopUtils.CheckUpdate();
            }

            var tasks = _modules.ConvertAll(m => m.InitializeAsync());

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
            MainWindow = new ConfigUI(RazerSdkModule.RzSdkManager, PluginsModule.PluginManager, LayoutsModule.LayoutManager, HttpListenerModule.HttpListener);

            Task.WhenAll(tasks).Wait();

            Global.logger.Info("Loading Device Manager");
            Global.dev_manager.RegisterVariables();
            Task.Run(async () => await Global.dev_manager.InitializeDevices());

            Global.logger.Info("Loading ConfigUI...");

            Global.LightingStateManager.InitUpdate();
            ((ConfigUI)MainWindow).DisplayIfNotSilent();

            //Debug Windows on Startup
            if (Global.Configuration.BitmapWindowOnStartUp)
                Window_BitmapView.Open();
            if (Global.Configuration.HttpWindowOnStartUp)
                Window_GSIHttpDebug.Open(HttpListenerModule.HttpListener);
        }
        else
        {
            try
            {
                NamedPipeClientStream client = new NamedPipeClientStream(".", "aurora\\interface", PipeDirection.Out);
                client.Connect(30);
                if (!client.IsConnected)
                    throw new InvalidOperationException();
                byte[] command = Encoding.ASCII.GetBytes("restore");
                client.Write(command, 0, command.Length);
                client.Close();
            }
            catch
            {
                MessageBox.Show("Aurora is already running.\r\nExiting.", "Aurora - Error");
                ForceShutdownApp(0);
            }
        }
    }

    private void ForceShutdownApp(int exitCode)
    {
        Environment.ExitCode = exitCode;
        Environment.Exit(exitCode);
    }

    private void UseArgs(StartupEventArgs e)
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
                    IsSilent = true;
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
            }
        }
    }

    private static void PrintSystemInfo()
    {
        StringBuilder systeminfoSb = new StringBuilder(string.Empty);
        systeminfoSb.Append("\r\n========================================\r\n");

        try
        {
            var winReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string productName = (string)winReg.GetValue("ProductName");

            systeminfoSb.AppendFormat("Operation System: {0}\r\n", productName);
        }
        catch (Exception exc)
        {
            systeminfoSb.AppendFormat("Operation System: Could not be retrieved. [Exception: {0}]\r\n", exc.Message);
        }

        systeminfoSb.AppendFormat("System Architecture: " + (Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit") +
                                   "\r\n");

        systeminfoSb.AppendFormat("Environment OS Version: {0}\r\n", Environment.OSVersion);

        systeminfoSb.AppendFormat("System Directory: {0}\r\n", Environment.SystemDirectory);
        systeminfoSb.AppendFormat("Executing Directory: {0}\r\n", Global.ExecutingDirectory);
        systeminfoSb.AppendFormat("Launch Directory: {0}\r\n", Directory.GetCurrentDirectory());
        systeminfoSb.AppendFormat("Processor Count: {0}\r\n", Environment.ProcessorCount);

        systeminfoSb.AppendFormat("SystemPageSize: {0}\r\n", Environment.SystemPageSize);
        systeminfoSb.AppendFormat("Environment Version: {0}\r\n", Environment.Version);

        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        systeminfoSb.Append($"Is Elevated: {principal.IsInRole(WindowsBuiltInRole.Administrator)}\r\n");
        systeminfoSb.Append($"Aurora Assembly Version: {Assembly.GetExecutingAssembly().GetName().Version}\r\n");
        systeminfoSb.Append(
            $"Aurora File Version: {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}\r\n");

        systeminfoSb.Append("========================================\r\n");
        Global.logger.Info(systeminfoSb.ToString());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _closing = true;
        base.OnExit(e);

        if (Global.Configuration != null)
            ConfigManager.Save(Global.Configuration);

        var tasks = _modules.ConvertAll(m => m.DisposeAsync());
        var devicesShutdown = Global.dev_manager?.ShutdownDevices().ContinueWith(_ => Global.dev_manager.Dispose());
        tasks.Add(devicesShutdown);
        
        Environment.ExitCode = 0;
        var forceExitTimer = StartForceExitTimer();
        forceExitTimer.DisableComObjectEagerCleanup();

        Task.WhenAll(tasks).Wait();
    }

    private Thread StartForceExitTimer()
    {
        var thread = new Thread(() =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread.Sleep(3000);
            if (stopwatch.ElapsedMilliseconds > 2500)
            {
                ForceShutdownApp(0);
            }
        })
        {
            IsBackground = true,
            Name = "Exit timer"
        };
        thread.Start();

        return thread;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception exc = (Exception)e.ExceptionObject;

        if (exc is COMException { Message: "0x88890004" })
        {
            return;
        }

        Global.logger.Fatal(exc, "Fatal Exception caught : ");

        if (!e.IsTerminating || Current == null || _closing)
        {
            return;
        }

        if (exc is SEHException sehException && sehException.CanResume())
        {
            return;
        }

        Global.logger.Fatal($"Runtime terminating: {e.IsTerminating}");
        LogManager.Flush();

        if (!Global.Configuration.CloseProgramOnException) return;
        MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
        //Perform exit operations
        Current.Shutdown();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var exc = e.Exception;

        if (exc is COMException { Message: "0x88890004" })
        {
            e.Handled = true;
            return;
        }

        Global.logger.Fatal(exc, "Fatal Exception caught : " + exc);
        LogManager.Flush();
        if (!Global.isDebug)
            e.Handled = true;
        else
            throw exc;
        if (!Global.Configuration.CloseProgramOnException) return;
        if (_closing) return;
        MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
        //Perform exit operations
        Current?.Shutdown();
    }
}