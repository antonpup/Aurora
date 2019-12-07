using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;

using Aurora.Settings;
using Aurora.Utils;

using Q42.HueApi.Models.Bridge;

using Color = System.Drawing.Color;

namespace Aurora.Devices.Hue
{
    class HueDevice : Device
    {
        private VariableRegistry registry;
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
            if (registry == null)
            {
                registry = new VariableRegistry();
                foreach (var identifier in HueVisualNames.Identifiers)
                {
                    registry.Register($"{name.ToLower().Replace(" ", "_")}_{identifier.Key}", identifier.Value.GetItem());
                }
            }
            return registry;
        }

        public T GetVariable<T>(string name)
        {
            return registry.GetVariable<T>($"{this.name.ToLower().Replace(" ", "_")}_{name}");
        }

        public string GetDeviceName()
        {
            return $"{name} Bridge";
        }

        public string GetDeviceDetails()
        {
            return $"{GetDeviceName()} {(initialized ? "Connected to:" + conf.Name : "Connecting to:" + (bridge != null ? bridge.BridgeId : "") + " press the sync button to finalize setup.")} {(initialized ? ", controlling: " + (lights?.Count ?? 0) + " lights" : "")}";
        }

        public string GetDeviceUpdatePerformance()
        {
            return $"~{GetVariable<int>("send_interval")}ms";
        }

        public bool Initialize()
        {
            try
            {
                curKey = GetVariable<int>("first_key");
                var _ip = GetVariable<string>("ip");
                if (_ip == "0.0.0.0" || !new Regex(@"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9])\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9])\b").IsMatch(_ip))
                {
                    IBridgeLocator bridgeLocator = new HttpBridgeLocator();
                    var bridges = bridgeLocator.LocateBridgesAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                    bridge = bridges.First();
                    client = new LocalHueClient(bridge.IpAddress);
                    conf = client.GetConfigAsync().Result;
                }
                else
                {
                    client = new LocalHueClient(_ip);
                    conf = client.GetConfigAsync().Result;
                }
                if (File.Exists(Path.Combine(Global.AppDataDirectory, conf.BridgeId)))
                {
                    client.Initialize(File.ReadAllText(Path.Combine(Global.AppDataDirectory, conf.BridgeId)));
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
                                File.WriteAllText(Path.Combine(Global.AppDataDirectory, conf.BridgeId), key);
                            }
                            catch
                            {
                                //Just catch the error RegisterAsync throws if the sync button isn't pressed yet
                            }
                            Task.Delay(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult(); //recheck the bridge for pairing every second
                        }
                    });
                }
                return true;
            }
            catch (Exception e)
            {
                Global.logger.Error($"Error initializing Hue connection{Environment.NewLine}{e.ToString()}");
                return false;
            }
        }

        public void Shutdown()
        {
        }

        public void Reset()
        {
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
            var firstKey = GetVariable<int>("first_key");
            var lastKey = GetVariable<int>("last_key");
            lights = client.GetLightsAsync().GetAwaiter().GetResult().ToList();
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
            if (!initialized || DateTime.Now - lastCall <= TimeSpan.FromMilliseconds(GetVariable<int>("send_interval")))
                return false;

            var defaultColor = GetVariable<RealColor>("default_color").GetHueDeviceColor();
            foreach (var light in lights)
            {
                var lightConfigName = $"{name.ToLower().Replace(" ", "_")}_send_{light.Name.ToLower().Replace(" ", "_")}";
                if (!registry.HasVariable(lightConfigName))
                {
                    registry.Register(lightConfigName, false, $"Control if {light.Name} should be synced");
                }
            }
            var brightness = GetVariable<int>("brightness");
            var command = new LightCommand();
            var defaultCommand = new LightCommand().SetColor(defaultColor).TurnOn();
            defaultCommand.Brightness = (byte)(defaultCommand.Brightness / (double)byte.MaxValue * (brightness / (double)byte.MaxValue) * byte.MaxValue);
            command.SetColor(GetVariable<bool>("blend_keys") ? keyColors.Where(t => t.Key <= (DeviceKeys)lastKey && t.Key >= (DeviceKeys)firstKey).Select(t => t.Value).GetHueDeviceColorMergedColor() : keyColors.Single(t => t.Key == (DeviceKeys)curKey).Value.GetHueDeviceColor());
            if (brightness == 0)
            {
                command.TurnOff();
                defaultCommand.TurnOff();
            }
            else
            {
                if (command.IsColorBlack() && DateTime.Now - lastCall >= TimeSpan.FromSeconds(GetVariable<int>("default_delay")) || GetVariable<bool>("force_default"))
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
                var commandToSend = GetVariable<bool>($"send_{light.Name.ToLower().Replace(" ", "_")}") ? command : defaultCommand;
                if (commandToSend.Brightness != light.State.Brightness || commandToSend.Saturation != light.State.Saturation || commandToSend.Hue != light.State.Hue || commandToSend.On != light.State.On)
                    client.SendCommandAsync(commandToSend, new[] { light.Id }).GetAwaiter().GetResult();
            }
            lastCall = DateTime.Now;
            lastFrame = command;
            curItr++;
            if (curItr > GetVariable<int>("key_iteration_count") - 1)
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
        public static Dictionary<string, HueIdentifiers> Identifiers = new Dictionary<string, HueIdentifiers>
        {
            {"ip", new HueIdentifiers("0.0.0.0","Bridge IP", "The IP to try to connect to (default 0.0.0.0 witch will scan network for bridges)") },
            {"send_interval", new HueIdentifiers(100,"Delay", "The delay to use to send to hue (min 100ms)",100, int.MaxValue)},
            {"use_default", new HueIdentifiers(true,"Use default color delay")},
            {"default_delay", new HueIdentifiers(5,"Default color delay", "The delay before sending default color in seconds", 0, int.MaxValue)},
            {"force_default", new HueIdentifiers(false, "Force default color")},
            {"default_color", new HueIdentifiers(new RealColor(Color.White), "Default Color", "The default color to use to send to hue if not active")},
            {"brightness", new HueIdentifiers(192,"Brightness", "Brightness on bulbs (0-255)",0,255)},
            {"blend_keys", new HueIdentifiers(false, "Blend key selection", "Blends all keys from the iteration of first key to last key")},
            {"key_iteration_count", new HueIdentifiers(10,"Iteration count", "How many times to iterate on one key before jumping to the next one",1, int.MaxValue)},
            {"first_key", new HueIdentifiers((int)DeviceKeys.Peripheral_Logo, "First key selection", "The first key to iterate on (must be lower or equal than last key)", 0, int.MaxValue)},
            {"last_key", new HueIdentifiers((int)DeviceKeys.Peripheral_Logo, "Last key selection", "The last key to iterate on (must be higher or equal than first key)", 0, int.MaxValue)}
        };

        public struct HueIdentifiers
        {
            public object Value;
            public string Title;
            public string Remark;
            public object Max;
            public object Min;

            public HueIdentifiers(object value, string title, string remark = "", object min = null, object max = null)
            {
                Title = title;
                Remark = remark;
                Value = value;
                Min = min;
                Max = max;
            }

            public VariableRegistryItem GetItem()
            {
                return new VariableRegistryItem(Value, Max, Min, Title, Remark);
            }
        }
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
