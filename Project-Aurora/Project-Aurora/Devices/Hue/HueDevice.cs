using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aurora.Settings;
using Aurora.Settings.Bindables;
using Aurora.Utils;

using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;
using Q42.HueApi.Models.Bridge;

using Color = System.Drawing.Color;
using IBindable = Aurora.Settings.Bindables.IBindable;

namespace Aurora.Devices.Hue
{
    class HueDevice : Device
    {
        private HueConfig config = new HueConfig();

        private string name = "Philips Hue";

        private bool initialized => client?.IsInitialized ?? false;

        private LocatedBridge bridge;

        private LocalHueClient client;

        private BridgeConfig conf;

        private List<Light> lights;

        public VariableRegistry GetRegisteredVariables()
        {
            VariableRegistry registry = new VariableRegistry();
            foreach (var pair in config.Store)
            {
                registry.Register($"{name.ToLower().Replace(" ", "_")}_{pair.Key}", pair.Value);
            }
            return registry;
        }

        public string GetDeviceName()
        {
            return $"{name} Bridge";
        }

        public string GetDeviceDetails()
        {
            return $"{GetDeviceName()} {(initialized ? "Connected to:" + conf.Name : "Connecting to:" + (bridge != null ? bridge.BridgeId : "") + " press the sync button to finalize setup.")} {(initialized ? ", controlling: " + lights.Count + " lights" : "")}";
        }

        public string GetDeviceUpdatePerformance()
        {
            return "~100ms";
        }

        public bool Initialize()
        {
            IBridgeLocator bridgeLocator = new HttpBridgeLocator();
            var bridges = bridgeLocator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).Result;
            bridge = bridges.First();
            client = new LocalHueClient(bridge.IpAddress);
            if (File.Exists(Path.Combine(Global.AppDataDirectory, bridge.BridgeId)))
            {
                client.Initialize(File.ReadAllText(Path.Combine(Global.AppDataDirectory, bridge.BridgeId)));
            }
            else
            {
                Task.Run(() =>
                {
                    while (!initialized)
                    {
                        try
                        {
                            var key = client.RegisterAsync("Aurora", Environment.MachineName).Result;
                            client.Initialize(key);
                            File.WriteAllText(Path.Combine(Global.AppDataDirectory, bridge.BridgeId), key);
                        }
                        catch { }
                        Task.Delay(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult(); //recheck the bridge for pairing every second
                    }
                });
            }
            conf = client.GetConfigAsync().Result;
            return true;
        }

        public void Shutdown()
        {
            config.Save();
        }

        public void Reset()
        {
            config.Load();
        }

        public bool Reconnect()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized()
        {
            return initialized;
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsKeyboardConnected()
        {
            throw new NotImplementedException();
        }

        public bool IsPeripheralConnected()
        {
            throw new NotImplementedException();
        }

        public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            if (!initialized || DateTime.Now - lastCall <= TimeSpan.FromMilliseconds(105)) return false;
            var defaultColor = config.GetColor("default_color").GetHueDeviceColor();
            lights = client.GetLightsAsync().GetAwaiter().GetResult().ToList();
            var lightIds = lights.Select(t => t.Id);
            var brightness = config.Get<int>("brightness");
            var command = new LightCommand();
            command.SetColor(keyColors.Single(t => t.Key == DeviceKeys.Peripheral_Logo).Value.GetHueDeviceColor());
            if (brightness == 0) command.TurnOff();
            else
            {
                if (command.IsColorBlack() && DateTime.Now - lastCall >= TimeSpan.FromSeconds(config.Get<int>("default_delay")) || config.Get<bool>("force_default"))
                {
                    command.SetColor(defaultColor);
                    lastFrameDefault = true;
                }
                command.Brightness = (byte) (command.Brightness / (double) byte.MaxValue * (brightness / (double) byte.MaxValue) * byte.MaxValue);
                if (command.Brightness == 0 && !lastFrameDefault)
                {
                    command.Hue = lastFrame.Hue;
                    command.Saturation = lastFrame.Saturation;
                    command.TurnOff();
                }
                else
                {
                    if (lights.Any(t => !t.State.On))
                    {
                        command.On = true;
                    }
                }
            }
            client.SendCommandAsync(command, lightIds).GetAwaiter().GetResult();
            lastCall = DateTime.Now;
            lastFrame = command;
            if (!command.IsColorSame(defaultColor)) lastFrameDefault = false;
            return true;
        }

        public LightCommand lastFrame;

        public bool lastFrameDefault;

        public DateTime lastCall;

        public bool UpdateDevice(DeviceColorComposition colorComposition, DoWorkEventArgs e, bool forced = false)
        {
            return UpdateDevice(colorComposition.keyColors, e, forced);
        }
    }

    class HueConfig : JsonConfigManager
    {
        public HueConfig() : base(Path.Combine(Global.AppDataDirectory, "hue.json"), null) { }

        protected override void InitialiseDefaults()
        {
            Set("default_delay", 1, 1, int.MaxValue);
            Set("default_color", new RealColor(Color.White));
            Set("force_default", false);
            Set("brightness", 255, 0, 255);
            Set("use_default", true);
        }

        public IDictionary<string, IBindable> Store => ConfigStore;
    }

    public static class Extension
    {
        public static RGBColor GetHueDeviceColor(this RealColor color)
        {
            return GetHueDeviceColor(color.GetDrawingColor());
        }

        public static RGBColor GetHueDeviceColor(this Color color)
        {
            return new RGBColor(color.R / (double) byte.MaxValue * (color.A / (double) byte.MaxValue), color.G / (double) byte.MaxValue * (color.A / (double) byte.MaxValue), color.B / (double) byte.MaxValue * (color.A / (double) byte.MaxValue));
        }

        public static RGBColor GetHueDeviceColorMergedColor(this IEnumerable<Color> colors)
        {
            if (!colors.Any()) return new RGBColor(0, 0, 0);
            var f = colors.Select(t => new {A = t.A / (double) byte.MaxValue, R = t.R / (double) byte.MaxValue, G = t.G / (double) byte.MaxValue, B = t.B / (double) byte.MaxValue}).ToList();
            return new RGBColor(f.Sum(t => t.R) / f.Count, f.Sum(t => t.G) / f.Count, f.Sum(t => t.B) / f.Count);
        }

        public static bool IsColorBlack(this LightCommand command) => command.Hue == 0 && command.Brightness == 0 && command.Saturation == 0;

        public static bool IsColorSame(this LightCommand command, RGBColor color)
        {
            var hsb = color.GetHSB();
            return command.Hue == hsb.Hue && command.Brightness == hsb.Brightness && command.Saturation == hsb.Saturation;
        }
    }
}