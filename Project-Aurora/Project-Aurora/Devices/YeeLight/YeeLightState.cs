using System;
using System.Collections.Generic;
using System.Drawing;
using YeeLightAPI.YeeLightConstants;

namespace Aurora.Devices.YeeLight
{
    public interface IYeeLightState
    {
        void InitState();
        
        IYeeLightState Update(Color color);
    }

    internal static class YeeLightStateBuilder
    {
        public static IYeeLightState Build(List<YeeLightAPI.YeeLightDevice> devices, int whiteCounter)
        {
            var colorState = new YeeLightStateColor(devices, whiteCounter);
            var whiteState = new YeeLightStateWhite(devices);
            var offState = new YeeLightStateOff(devices);
            
            colorState.WhiteState = whiteState;
            colorState.OffState = offState;
            whiteState.ColorState = colorState;
            whiteState.OffState = offState;
            offState.ColorState = colorState;

            return offState;
        }
    }
    
    class YeeLightStateOff : IYeeLightState
    {
        public IYeeLightState ColorState;

        private readonly List<YeeLightAPI.YeeLightDevice> _lights;

        public YeeLightStateOff(List<YeeLightAPI.YeeLightDevice> lights)
        {
            _lights = lights;
        }

        public void InitState()
        {
            TurnOff();
        }

        public IYeeLightState Update(Color color)
        {
            if (Utils.IsBlack(color))
            {
                return this;
            }

            TurnOn();
            ColorState.InitState();
            var newState = ColorState.Update(color);
            return newState;
        }
        
        private void TurnOff()
        {
            _lights.ForEach(device => device.SetPower(Constants.PowerStateParamValues.OFF));
        }
        
        private void TurnOn()
        {
            _lights.ForEach(device => device.SetPower(Constants.PowerStateParamValues.ON));
        }
    }
    
    class YeeLightStateColor : IYeeLightState
    {
        public IYeeLightState WhiteState;
        public IYeeLightState OffState;

        private readonly List<YeeLightAPI.YeeLightDevice> _lights;
        private readonly int _whiteCounterStart;

        private int _previousBrightness;
        private Color _previousColor;
        private int _whiteCounter;

        public YeeLightStateColor(List<YeeLightAPI.YeeLightDevice> devices, int whiteCounterStart)
        {
            _lights = devices;
            _whiteCounterStart = whiteCounterStart;
        }

        public void InitState()
        {
        }

        public IYeeLightState Update(Color color)
        {
            if (Utils.IsWhiteTone(color)) // && Global.LightingStateManager.GetCurrentProfile() == Global.LightingStateManager.DesktopProfile
            {
                if (Utils.IsBlack(color))
                {
                    OffState.InitState();
                    return OffState.Update(color);
                }
                if (_whiteCounter-- <= 0)
                {
                    WhiteState.InitState();
                    return WhiteState.Update(color);
                }
            }
            else
            {
                _whiteCounter = _whiteCounterStart;
            }
            
            if (_previousColor == color)
                return ProceedSameColor(color);
            _previousColor = color;
            
            //color changed
            UpdateLights(color);
            return this;
        }

        private IYeeLightState ProceedSameColor(Color targetColor)
        {
            if (Utils.ShouldSendKeepAlive())
            {
                UpdateLights(targetColor);
            }
            return this;
        }

        private void UpdateLights(Color color)
        {
            _lights.ForEach(x =>
            {
                x.SetColor(color.R, color.G, color.B);
                var brightness = Math.Max(color.R, Math.Max(color.G, Math.Max(color.B, (short) 1))) * 100 / 255;
                if (_previousBrightness == brightness) return;
                _previousBrightness = brightness;
                x.SetBrightness(brightness);
            });
        }
    }
    
    class YeeLightStateWhite : IYeeLightState
    {
        public IYeeLightState ColorState;
        public IYeeLightState OffState;

        private readonly List<YeeLightAPI.YeeLightDevice> _lights;

        private Color _previousColor;

        public YeeLightStateWhite(List<YeeLightAPI.YeeLightDevice> lights)
        {
            _lights = lights;
        }

        public void InitState()
        {
            _lights.ForEach(x =>
            {
                x.SetTemperature(6500);
            });
        }

        public IYeeLightState Update(Color color)
        {
            if (!Utils.IsWhiteTone(color))
            {
                ColorState.InitState();
                return ColorState.Update(color);
            }
            
            if (Utils.IsBlack(color))
            {
                OffState.InitState();
                return OffState.Update(color);
            }
            
            if (_previousColor == color)
            {
                if (Utils.ShouldSendKeepAlive())
                {
                    UpdateLights(color);
                }
                return this;
            }

            _previousColor = color;
            
            //color changed
            UpdateLights(color);
            return this;
        }

        private void UpdateLights(Color color)
        {
            _lights.ForEach(x =>
            {
                x.SetBrightness(color.R * 100 / 255);
            });
        }
    }

    static class Utils
    {
        internal static bool IsWhiteTone(Color color)
        {
            return color.R == color.G && color.G == color.B;
        }
        internal static bool IsBlack(Color color)
        {
            return color.R == 0 && color.G == 0 && color.B  == 0;
        }

        private const int KeepAliveCounter = 500;
        private static int _keepAlive = KeepAliveCounter;
        internal static bool ShouldSendKeepAlive()
        {
            if (_keepAlive-- != 0) return false;
            _keepAlive = KeepAliveCounter;
            return true;
        }
    }
}