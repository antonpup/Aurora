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

using Newtonsoft.Json;

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
        private int curKey = 0;
        private int curItr = 0;

        public VariableRegistry GetRegisteredVariables()
        {
            VariableRegistry registry = new VariableRegistry();
            foreach (var pair in config.Store)
            {
                registry.Register($"{name.ToLower().Replace(" ", "_")}_{pair.Key}", pair.Value, HueVisualNames.GetTitle(pair.Key), HueVisualNames.GetRemark(pair.Key));
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
            return $"~{config.Get<int>("send_interval")}ms";
        }

        public bool Initialize()
        {
            try
            {
                curKey = config.Get<int>("first_key");
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
                            catch
                            {
                                //Just catch the error RegisterAsync throws if the sync button isn't pressed yet
                            }
                            Task.Delay(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult(); //recheck the bridge for pairing every second
                        }
                    });
                }
                conf = client.GetConfigAsync().Result;
                return true;
            }
            catch (Exception e)
            {
                Global.logger.Error(e, "No bridge found");
                return false;
            }
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
            var firstKey = config.Get<int>("first_key");
            var lastKey = config.Get<int>("last_key");
            if (!(firstKey <= lastKey && keyColors.Any(t => t.Key <= (DeviceKeys)lastKey && t.Key >= (DeviceKeys)firstKey)))
                return false;

            while (!keyColors.ContainsKey((DeviceKeys)curKey))
            {
                curKey++;
                if (curKey > lastKey)
                    curKey = firstKey;
                if (curKey > 338)
                {
                    curKey = 0;
                }
            }
            if (!initialized || DateTime.Now - lastCall <= TimeSpan.FromMilliseconds(config.Get<int>("send_interval")))
                return false;

            var defaultColor = config.GetColor("default_color").GetHueDeviceColor();
            lights = client.GetLightsAsync().GetAwaiter().GetResult().ToList();
            foreach (var light in lights)
            {
                var lightConfigName = $"send_{light.Name.ToLower().Replace(" ", "_")}";
                if (!config.Store.ContainsKey(lightConfigName))
                {
                    config.Set(lightConfigName, false);
                }
                else
                {
                    try
                    {
                        config.Get<bool>(lightConfigName);
                    }
                    catch
                    {
                        config.Set(lightConfigName, Convert.ToBoolean(config.Get<string>(lightConfigName)));
                    }
                }
            }
            var brightness = config.Get<int>("brightness");
            var command = new LightCommand();
            var defaultCommand = new LightCommand().SetColor(defaultColor).TurnOn();
            defaultCommand.Brightness = (byte)(defaultCommand.Brightness / (double)byte.MaxValue * (brightness / (double)byte.MaxValue) * byte.MaxValue);
            command.SetColor(config.Get<bool>("blend_keys") ? keyColors.Where(t => t.Key <= (DeviceKeys)lastKey && t.Key >= (DeviceKeys)firstKey).Select(t => t.Value).GetHueDeviceColorMergedColor() : keyColors.Single(t => t.Key == (DeviceKeys)curKey).Value.GetHueDeviceColor());
            if (brightness == 0)
            {
                command.TurnOff();
                defaultCommand.TurnOff();
            }
            else
            {
                if (command.IsColorBlack() && DateTime.Now - lastCall >= TimeSpan.FromSeconds(config.Get<int>("default_delay")) || config.Get<bool>("force_default"))
                {
                    command.SetColor(defaultColor);
                    lastFrameDefault = true;
                }
                command.Brightness = (byte)(command.Brightness / (double)byte.MaxValue * (brightness / (double)byte.MaxValue) * byte.MaxValue);
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
            foreach (var light in lights)
            {
                var commandToSend = config.Get<bool>($"send_{light.Name.ToLower().Replace(" ", "_")}") ? command : defaultCommand;
                if (commandToSend.Brightness != light.State.Brightness || commandToSend.Saturation != light.State.Saturation || commandToSend.Hue != light.State.Hue || commandToSend.On != light.State.On)
                    client.SendCommandAsync(commandToSend, new[] {light.Id}).GetAwaiter().GetResult();
            }
            lastCall = DateTime.Now;
            lastFrame = command;
            curItr++;
            if (curItr > config.Get<int>("key_iteration_count") - 1)
            {
                curKey++;
                curItr = 0;
            }
            if (curKey > lastKey)
                curKey = firstKey;
            if (!command.IsColorSame(defaultColor))
                lastFrameDefault = false;
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

    static class HueVisualNames
    {
        private static Dictionary<string, HueIdentifiers> _identifiers = new Dictionary<string, HueIdentifiers>
        {
            {"default_delay", new HueIdentifiers("Delay", "The delay before sending default color in seconds")},
            {"default_color", new HueIdentifiers("Default Color", "The default color to use to send to hue if not active")},
            {"blend_keys", new HueIdentifiers("Blend key selection", "")},
            {"force_default", new HueIdentifiers("Force default color", "")},
            {"brightness", new HueIdentifiers("Brightness", "Brightness on bulbs (0-255)")},
            {"use_default", new HueIdentifiers("Use default delay", "")},
            {"key_iteration_count", new HueIdentifiers("Iteration count", "How many times to iterate on one key before jumping to the next one")},
            {"first_key", new HueIdentifiers("First key selection", "The first key to iterate on (must be lower or equal than last key)")},
            {"last_key", new HueIdentifiers("Last key selection", "The last key to iterate on (must be higher or equal than first key)")},
            {"send_interval", new HueIdentifiers("Delay", "The delay to use to send to hue (min 100ms)")}
        };

        public static string GetTitle(string v)
        {
            return !_identifiers.ContainsKey(v) ? "" : _identifiers[v].Title;
        }
        public static string GetRemark(string v)
        {
            return !_identifiers.ContainsKey(v) ? "" : _identifiers[v].Remark;
        }

        struct HueIdentifiers
        {
            public string Title;
            public string Remark;

            public HueIdentifiers(string title, string remark)
            {
                Title = title;
                Remark = remark;
            }
        }
    }

    class HueConfig : JsonConfigManager
    {
        public HueConfig() : base(Path.Combine(Global.AppDataDirectory, "hue.json"), null) { }

        protected override void InitialiseDefaults()
        {
            Set("default_delay", 1, 1, int.MaxValue);
            Set("default_color", new RealColor(Color.White));
            Set("blend_keys", false);
            Set("force_default", false);
            Set("brightness", 255, 0, 255);
            Set("use_default", true);
            Set("key_iteration_count", 5, 1, int.MaxValue);
            Set("first_key", (int)DeviceKeys.Peripheral_Logo, (int)DeviceKeys.Peripheral, (int)DeviceKeys.MONITORLIGHT103);
            Set("last_key", (int)DeviceKeys.Peripheral_Logo, (int)DeviceKeys.Peripheral, (int)DeviceKeys.MONITORLIGHT103);
            Set("send_interval", 100, 100, int.MaxValue);
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
            return new RGBColor(color.R / (double)byte.MaxValue * (color.A / (double)byte.MaxValue), color.G / (double)byte.MaxValue * (color.A / (double)byte.MaxValue), color.B / (double)byte.MaxValue * (color.A / (double)byte.MaxValue));
        }

        public static RGBColor GetHueDeviceColorMergedColor(this IEnumerable<Color> colors)
        {
            if (!colors.Any())
                return new RGBColor(0, 0, 0);

            var f = colors.Select(t => new
            {
                A = t.A / (double)byte.MaxValue,
                R = t.R / (double)byte.MaxValue,
                G = t.G / (double)byte.MaxValue,
                B = t.B / (double)byte.MaxValue
            }).ToList();
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