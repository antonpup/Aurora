using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings;
using Aurora.Utils;
using Bloody.NET;

namespace Aurora.Devices.Bloody
{
    public class BloodyDevice : DefaultDevice
    {
        public override string DeviceName => "Bloody";
        protected override string DeviceInfo => IsInitialized ? GetDeviceNames() : base.DeviceInfo;
        private readonly Stopwatch _updateDelayStopWatch = new();

        private BloodyKeyboard _keyboard;
        private List<BloodyPeripheral> _peripherals;

        private event EventHandler<Dictionary<DeviceKeys, Color>> DeviceUpdated;

        protected override Task<bool> DoInitialize()
        {
            _keyboard = BloodyKeyboard.Initialize();
            if(_keyboard != null)
            {
                DeviceUpdated += UpdateKeyboard;
            }

            _peripherals = BloodyPeripheral.GetDevices();
            DeviceUpdated += UpdatePeripherals;
            _updateDelayStopWatch.Start();

            IsInitialized = (_keyboard != null) || (_peripherals.Any());
            return Task.FromResult(IsInitialized);
        }

        public override Task Shutdown()
        {
            _keyboard?.Disconnect();
            DeviceUpdated -= UpdateKeyboard;

            _peripherals.ForEach(p => p.Disconnect());
            DeviceUpdated -= UpdatePeripherals;
            _updateDelayStopWatch.Stop();

            IsInitialized = false;
            return Task.CompletedTask;
        }

        protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, DoWorkEventArgs e, bool forced = false)
        {
            var sendDelay = Global.Configuration.VarRegistry.GetVariable<int>($"{DeviceName}_send_delay");
            if (_updateDelayStopWatch.ElapsedMilliseconds <= sendDelay)
                return Task.FromResult(false);

            DeviceUpdated?.Invoke(this, keyColors);
            _updateDelayStopWatch.Restart();
            return Task.FromResult(true);
        }
        protected override void RegisterVariables(VariableRegistry variableRegistry)
        {
            variableRegistry.Register($"{DeviceName}_send_delay", 32, "Send delay (ms)");
        }

        private void UpdateKeyboard(object sender, Dictionary<DeviceKeys, Color> keyColors)
        {
            foreach (var key in keyColors)
            {
                if (BloodyKeyMap.KeyMap.TryGetValue(key.Key, out var bloodyKey))
                    _keyboard.SetKeyColor(bloodyKey, ColorUtils.CorrectWithAlpha(key.Value));
            }

            _keyboard.Update();
        }

        private void UpdatePeripherals(object sender, Dictionary<DeviceKeys, Color> keyColors)
        {
            foreach(var dev in _peripherals)
            {
                Dictionary<BloodyPeripheralLed, DeviceKeys> keyMap = null;
                switch (dev.PeripheralType)
                {
                    case PeripheralType.MOUSE:
                        keyMap = BloodyKeyMap.MouseLightMap;
                        break;
                    case PeripheralType.MOUSEPAD:
                        keyMap = BloodyKeyMap.MousePadLightMap;
                        break;
                }
                foreach (KeyValuePair<BloodyPeripheralLed, DeviceKeys> ledAndKey in keyMap)
                {
                    Color color;
                    keyColors.TryGetValue(ledAndKey.Value, out color);
                    dev.SetKeyColor(ledAndKey.Key, ColorUtils.CorrectWithAlpha(color));
                }
                dev.Update();
            }
        }

        private string GetDeviceNames()
        {
            return (_keyboard != null ? " Keyboard" : "") + String.Join(" ", _peripherals);
        }
    }
}