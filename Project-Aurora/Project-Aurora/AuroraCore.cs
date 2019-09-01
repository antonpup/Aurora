using Aurora.Devices;
using Aurora.Devices.Layout;
using Aurora.Profiles;
using Aurora.Settings;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using RazerSdkWrapper;
using RazerSdkWrapper.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace Aurora
{
    public class AuroraCore : ManagerSettings<AuroraCoreSettings>
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static InputInterceptor InputInterceptor;

        public static string SavePath = App.AppDataDirectory;

        public static string ScriptDirectory = "Scripts";
        public static ScriptEngine PythonEngine = Python.CreateEngine();

        

        /// <summary>
        /// Input event subscriptions
        /// </summary>
        public static InputEvents InputEvents;

        //public static GameEventHandler geh;


        /// <summary>
        /// Currently held down modifer key
        /// </summary>
        public static Keys held_modified = Keys.None;

        public static object Clipboard { get; set; }

        public static long StartTime;


        public PluginManager PluginManager;
        public LightingStateManager LightingStateManager;
        public NetworkListener net_listener;
        public DeviceManager dev_manager;
        public Effects effengine;
        public KeyRecorder key_recorder;
        public RzManager razerManager;

        public AuroraCore() : base("CoreSettings.json")
        {

        }


        public override bool Initialize()
        {
            return Initialize(false);
        }

        public bool Initialize(bool ignoreUpdate = false)
        {
            if (Initialized)
                return true;

            if (!base.Initialize())
                throw new Exception("could not load settings");


            StartTime = Utils.TimeUtils.GetMillisecondsSinceEpoch();

            logger.Info("Loading Plugins");
            (PluginManager = new PluginManager()).Initialize(this);

            dev_manager = new DeviceManager();
            effengine = new Effects();

            Process.GetCurrentProcess().PriorityClass = this.Settings.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;

            if (this.Settings.CheckUpdatesOnStart && !ignoreUpdate)
            {
                string updater_path = Path.Combine(App.ExecutingDirectory, "Aurora-Updater.exe");

                if (File.Exists(updater_path))
                {
                    try
                    {
                        ProcessStartInfo updaterProc = new ProcessStartInfo();
                        updaterProc.FileName = updater_path;
                        updaterProc.Arguments = "-silent";
                        Process.Start(updaterProc);
                    }
                    catch (Exception exc)
                    {
                        logger.Error("Could not start Aurora Updater. Error: " + exc);
                    }
                }
            }

            

            logger.Info("Loading Global Device Layout");
            GlobalDeviceLayout.Instance.Initialize();

            logger.Info("Loading Input Hooking");
            InputEvents = new InputEvents();
            this.Settings.PropertyChanged += SetupVolumeAsBrightness;

            SetupVolumeAsBrightness(this.Settings,
                new PropertyChangedExEventArgs(nameof(this.Settings.UseVolumeAsBrightness), null, null));
            Utils.DesktopUtils.StartSessionWatch();

            key_recorder = new KeyRecorder(InputEvents);

            logger.Info("Loading RazerManager");
            if (RzHelper.IsSdkVersionSupported(RzHelper.GetSdkVersion()))
            {
                try
                {
                    razerManager = new RzManager()
                    {
                        KeyboardEnabled = true,
                        MouseEnabled = true,
                        MousepadEnabled = true,
                        AppListEnabled = true,
                    };

                    logger.Info("RazerManager loaded successfully!");
                }
                catch (Exception exc)
                {
                    logger.Fatal("RazerManager failed to load!");
                    logger.Fatal(exc.ToString());
                }
            }
            else
            {
                logger.Warn("Currently installed razer sdk version \"{0}\" is not supported!", RzHelper.GetSdkVersion());
            }

            logger.Info("Loading Applications");
            (LightingStateManager = new LightingStateManager()).Initialize();

            if (this.Settings.GetPointerUpdates)
            {
                logger.Info("Fetching latest pointers");
                Task.Run(() => Utils.PointerUpdateUtils.FetchDevPointers("master"));
            }

            logger.Info("Loading Device Manager");
            //dev_manager.RegisterVariables();
            dev_manager.Initialize();

            /*Global.logger.LogLine("Starting GameEventHandler", Logging_Level.Info);
            Global.geh = new GameEventHandler();
            if (!Global.geh.Init())
            {
                Global.logger.LogLine("GameEventHander could not initialize", Logging_Level.Error);
                return;
            }*/

            logger.Info("Starting GameStateListener");
            try
            {
                net_listener = new NetworkListener(9088);
                net_listener.NewGameState += new NewGameStateHandler(LightingStateManager.GameStateUpdate);
                net_listener.WrapperConnectionClosed += new WrapperConnectionClosedHandler(LightingStateManager.ResetGameState);
            }
            catch (Exception exc)
            {
                logger.Error("GameStateListener Exception, " + exc);
                System.Windows.MessageBox.Show("GameStateListener Exception.\r\n" + exc);
                return false;
            }

            if (!net_listener.Start())
            {
                logger.Error("GameStateListener could not start");
                System.Windows.MessageBox.Show("GameStateListener could not start. Try running this program as Administrator.\r\nExiting.");
                return false;
            }

            logger.Info("Listening for game integration calls...");

            InitUpdate();

            return Initialized = true;
        }

        private static void SetupVolumeAsBrightness(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (sender is AuroraCoreSettings settings)
            {

                if (eventArgs.PropertyName == nameof(AuroraCoreSettings.UseVolumeAsBrightness))
                {
                    if (settings.UseVolumeAsBrightness)
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
            else
            {
                throw new NotSupportedException("Why didn't I get AuroraCoreSettings?");
            }
        }

        private static void InterceptVolumeAsBrightness(object sender, InputInterceptor.InputEventData e)
        {
            var keys = (Keys)e.Data.VirtualKeyCode;
            if ((keys.Equals(Keys.VolumeDown) || keys.Equals(Keys.VolumeUp))
                && InputEvents.Alt)
            {
                e.Intercepted = true;
                Task.Factory.StartNew(() =>
                {
                    if (e.KeyDown)
                    {
                        float brightness = App.Core.Settings.GlobalBrightness;
                        brightness += keys == Keys.VolumeUp ? 0.05f : -0.05f;
                        App.Core.Settings.GlobalBrightness = Math.Max(0f, Math.Min(1f, brightness));
                    }
                }
                );
            }
        }

        private Timer updateTimer;

        private const int defaultTimerInterval = 33;
        private int timerInterval = defaultTimerInterval;
        private long currentTick;

        private void InitUpdate()
        {
            updateTimer = new Timer(g => {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                if (App.isDebug)
                    Update();
                else
                {
                    try
                    {
                        Update();
                    }
                    catch (Exception exc)
                    {
                        logger.Error("ProfilesManager.Update() Exception, " + exc);
                    }
                }

                watch.Stop();
                currentTick += timerInterval + watch.ElapsedMilliseconds;
                updateTimer?.Change(Math.Max(timerInterval, 0), Timeout.Infinite);
            }, null, 0, Timeout.Infinite);
            GC.KeepAlive(updateTimer);
        }

        private void Update()
        {
            LightingStateManager.Update(currentTick, dev_manager, effengine);
        }


        public override void Save()
        {
            base.Save();
            GlobalDeviceLayout.Instance.SaveSettings();
            LightingStateManager.Save();
            PluginManager.SaveSettings();
        }

        public override void Dispose()
        {
            updateTimer.Dispose();
            updateTimer = null;

            key_recorder?.Dispose();
            InputEvents?.Dispose();
            LightingStateManager?.Dispose();
            net_listener?.Stop();
            dev_manager?.Shutdown();
            dev_manager?.Dispose();

            try
            {
                razerManager?.Dispose();
            }
            catch (Exception exc)
            {
                logger.Fatal("RazerManager failed to dispose!");
                logger.Fatal(exc.ToString());
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
                logger.Error("Exception closing \"Aurora-SkypeIntegration\", Exception: " + exc);
            }
        }

        public override void AcceptPlugins(List<Type> plugin)
        {
            LightingStateManager.AcceptPlugins(plugin);
        }
    }

    public class AuroraCoreSettings : SettingsBase
    {
        private bool useVolumeAsBrightness = false;
        public bool UseVolumeAsBrightness { get { return useVolumeAsBrightness; } set { UpdateVar(ref useVolumeAsBrightness, value); } }

        private float globalBrightness = 1.0f;
        public float GlobalBrightness { get { return globalBrightness; } set { UpdateVar(ref globalBrightness, value); } }

        private float keyboardBrightness = 1.0f;
        public float KeyboardBrightness { get { return keyboardBrightness; } set { UpdateVar(ref keyboardBrightness, value); } }

        private float peripheralBrightness = 1.0f;
        public float PeripheralBrightness { get { return peripheralBrightness; } set { UpdateVar(ref peripheralBrightness, value); } }

        private bool getDevReleases = false;
        public bool GetDevReleases { get { return getDevReleases; } set { UpdateVar(ref getDevReleases, value); } }

        private bool getPointerUpdates = true;
        public bool GetPointerUpdates { get { return getPointerUpdates; } set { UpdateVar(ref getPointerUpdates, value); } }

        private bool highPriority = false;
        public bool HighPriority { get { return highPriority; } set { UpdateVar(ref highPriority, value); } }

        private bool enableAudioCapture = false;
        public bool EnableAudioCapture { get => enableAudioCapture; set { UpdateVar(ref enableAudioCapture, value); } }

        private bool checkUpdatesOnStart = true;
        public bool CheckUpdatesOnStart { get => checkUpdatesOnStart; set { UpdateVar(ref checkUpdatesOnStart, value); } }

        private bool startSilently = false;
        public bool StartSilently { get => startSilently; set { UpdateVar(ref startSilently, value); } }

        private AppExitMode closeMode = AppExitMode.Ask;
        public AppExitMode CloseMode { get => closeMode; set { UpdateVar(ref closeMode, value); } }

        //Debug Settings
        private bool bitmapDebugTopMost = false;
        public bool BitmapDebugTopMost { get { return bitmapDebugTopMost; } set { UpdateVar(ref bitmapDebugTopMost, value); } }

        private bool httpDebugTopMost = false;
        public bool HttpDebugTopMost { get { return httpDebugTopMost; } set { UpdateVar(ref httpDebugTopMost, value); } }
    }
}
