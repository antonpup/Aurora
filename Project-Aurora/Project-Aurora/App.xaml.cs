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
using Aurora.Modules.GameStateListen;
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
    public static bool Closing { get; private set; }
    private static readonly Mutex Mutex = new(false, "{C88D62B0-DE49-418E-835D-CE213D58444C}");

    public static bool IsSilent { get; private set; }
    private bool _ignoreUpdate;

    private static readonly PluginsModule PluginsModule = new();
    private static readonly IpcListenerModule IpcListenerModule = new();
    private static readonly HttpListenerModule HttpListenerModule = new();
    private static readonly LightningStateManagerModule LightningStateManagerModule = new(
        PluginsModule.PluginManager, IpcListenerModule.IpcListener, HttpListenerModule.HttpListener);
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
        LightningStateManagerModule,
        RazerSdkModule,
        LayoutsModule,
        new HardwareMonitorModule(),
    };

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Global.Initialize();
        UseArgs(e);

        try
        {
            if (!Mutex.WaitOne(TimeSpan.FromMilliseconds(0), true))
            {
                try
                {
                    NamedPipeClientStream client = new NamedPipeClientStream(
                        ".", IpcListener.AuroraInterfacePipe, PipeDirection.Out, PipeOptions.Asynchronous);
                    client.Connect(2);
                    if (!client.IsConnected)
                        throw new InvalidOperationException();
                    var command = Encoding.ASCII.GetBytes("restore");
                    client.Write(command, 0, command.Length);
                    client.Close();
                }
                catch
                {
                    MessageBox.Show("Aurora is already running.\r\nExiting.", "Aurora - Error");
                    ForceShutdownApp(0);
                }

                Closing = true;
                Environment.Exit(0);
                return;
            }
        }
        catch(AbandonedMutexException) { /* Means previous instance closed anyway */ }

        new UserSettingsBackup().BackupIfNew();
        PrintSystemInfo();

        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.AppendPrivatePath("x64");
        if (!Global.isDebug)
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

        Global.dev_manager = new DeviceManager();
        Global.effengine = new Effects();

        //Load config
        Global.logger.Info("Loading Configuration");
        Global.Configuration = ConfigManager.Load();

        if (Global.Configuration.HighPriority)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        }
        if (Global.Configuration.UpdatesCheckOnStartUp && !_ignoreUpdate)
        {
            DesktopUtils.CheckUpdate();
        }

        _modules.ForEach(m => m.InitializeAsync());

        Global.dev_manager.RegisterVariables();
        RazerSdkModule.RzSdkManager.ContinueWith(_ => //Razer device needs this
        {
            Global.logger.Info("Loading Device Manager");
            return Global.dev_manager.InitializeDevices();
        });

        MainWindow = new ConfigUI(RazerSdkModule.RzSdkManager, PluginsModule.PluginManager, LayoutsModule.LayoutManager,
            HttpListenerModule.HttpListener, IpcListenerModule.IpcListener);

        Global.logger.Info("Loading ConfigUI...");
        ((ConfigUI)MainWindow).DisplayIfNotSilent();

        //Debug Windows on Startup
        if (Global.Configuration.BitmapWindowOnStartUp)
            Window_BitmapView.Open();
        if (Global.Configuration.HttpWindowOnStartUp)
            Window_GSIHttpDebug.Open(HttpListenerModule.HttpListener);
    }

    private void ForceShutdownApp(int exitCode)
    {
        Mutex.ReleaseMutex();
        Mutex.Dispose();
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
                case "-restart":
                    var pid = int.Parse(e.Args[++i]);
                    try
                    {
                        var previousAuroraProcess = Process.GetProcessById(pid);
                        previousAuroraProcess.WaitForExit();
                    }
                    catch (ArgumentException exception) { /* process doesn't exist */ }
                    break;
                case "-minimized":
                case "-silent":
                    IsSilent = true;
                    Global.logger.Info("Program started with '-silent' parameter");
                    break;
                case "-ignore_update":
                    _ignoreUpdate = true;
                    Global.logger.Info("Program started with '-ignore_update' parameter");
                    break;
                case "-delay":
                    if (i + 1 >= e.Args.Length || !int.TryParse(e.Args[i++], out var delayTime))
                        delayTime = 5000;
                    Global.logger.Info("Program started with '-delay' parameter with delay of " + delayTime + " ms");
                    Thread.Sleep(delayTime);
                    break;
            }
        }
    }

    private static void PrintSystemInfo()
    {
        var systemInfoSb = new StringBuilder(string.Empty);
        systemInfoSb.Append("\r\n========================================\r\n");

        try
        {
            var winReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var productName = (string)winReg.GetValue("ProductName");

            systemInfoSb.Append($"Operation System: {productName}\r\n");
        }
        catch (Exception exc)
        {
            systemInfoSb.Append($"Operation System: Could not be retrieved. [Exception: {exc.Message}]\r\n");
        }

        systemInfoSb.AppendFormat("System Architecture: " + (Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit") +
                                   "\r\n");

        systemInfoSb.Append($"Environment OS Version: {Environment.OSVersion}\r\n");

        systemInfoSb.Append($"System Directory: {Environment.SystemDirectory}\r\n");
        systemInfoSb.Append($"Executing Directory: {Global.ExecutingDirectory}\r\n");
        systemInfoSb.Append($"Launch Directory: {Directory.GetCurrentDirectory()}\r\n");
        systemInfoSb.Append($"Processor Count: {Environment.ProcessorCount}\r\n");

        systemInfoSb.Append($"SystemPageSize: {Environment.SystemPageSize}\r\n");
        systemInfoSb.Append($"Environment Version: {Environment.Version}\r\n");

        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        systemInfoSb.Append($"Is Elevated: {principal.IsInRole(WindowsBuiltInRole.Administrator)}\r\n");
        systemInfoSb.Append($"Aurora Assembly Version: {Assembly.GetExecutingAssembly().GetName().Version}\r\n");
        systemInfoSb.Append(
            $"Aurora File Version: {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}\r\n");

        systemInfoSb.Append("========================================\r\n");
        Global.logger.Info(systemInfoSb.ToString());
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (Closing)
        {
            return;
        }
        Closing = true;
        base.OnExit(e);

        if (Global.Configuration != null)
            ConfigManager.Save(Global.Configuration);

        var tasks = _modules.ConvertAll(m => m.DisposeAsync());
        var devicesShutdown = Global.dev_manager?.ShutdownDevices().ContinueWith(_ => Global.dev_manager.Dispose());
        tasks.Add(devicesShutdown);
        
        var forceExitTimer = StartForceExitTimer();

        Task.WhenAll(tasks).Wait();
        forceExitTimer.GetApartmentState(); //statement just to keep referenced
        LogManager.Flush();
        Mutex.ReleaseMutex();
        Mutex.Dispose();
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

        if (!e.IsTerminating || Current == null || Closing)
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
        if (!Global.Configuration?.CloseProgramOnException ?? false) return;
        if (Closing) return;
        MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc, "Aurora has stopped working");
        //Perform exit operations
        Current?.Shutdown();
    }
}