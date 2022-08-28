using System;
using System.Collections.Generic;
using System.Drawing;
using YeeLightAPI.YeeLightConstants;

namespace Aurora.Devices.YeeLight
{
    public interface IYeeLightState
    {
        IYeeLightState Update(Color color);
    }

    public static class YeeLightStateBuilder
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

            return colorState;
        }
    }
    
    public class YeeLightStateOff : IYeeLightState
    {
        public IYeeLightState ColorState;

        private readonly List<YeeLightAPI.YeeLightDevice> _lights;
        
        private bool _isOff;

        public YeeLightStateOff(List<YeeLightAPI.YeeLightDevice> lights)
        {
            _lights = lights;
        }

        public IYeeLightState Update(Color color)
        {
            if (!_isOff)
            {
                _isOff = true;
                TurnOff();
            }
            
            if (Utils.IsBlack(color))
            {
                return this;
            }

            var newState = ColorState.Update(color);
            TurnOn();
            _isOff = false;
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
    
    public class YeeLightStateColor : IYeeLightState
    {
        public IYeeLightState WhiteState;
        public IYeeLightState OffState;

        private readonly List<YeeLightAPI.YeeLightDevice> _lights;
        private readonly int _whiteCounterStart;

        private Color _previousColor;
        private int _whiteCounter;

        public YeeLightStateColor(List<YeeLightAPI.YeeLightDevice> devices, int whiteCounterStart)
        {
            _lights = devices;
            _whiteCounterStart = whiteCounterStart;
        }

        public IYeeLightState Update(Color color)
        {
            if (Utils.IsWhiteTone(color)) // && Global.LightingStateManager.GetCurrentProfile() == Global.LightingStateManager.DesktopProfile
            {
                if (Utils.IsBlack(color))
                {
                    return OffState.Update(color);
                }
                if (_whiteCounter-- <= 0)
                {
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
                x.SetBrightness(Math.Max(color.R, Math.Max(color.G, Math.Max(color.B, (short) 1))) * 100 / 255);
            });
        }
    }
    
    public class YeeLightStateWhite : IYeeLightState
    {
        public IYeeLightState ColorState;
        public IYeeLightState OffState;

        private readonly List<YeeLightAPI.YeeLightDevice> _lights;

        private Color _previousColor;

        public YeeLightStateWhite(List<YeeLightAPI.YeeLightDevice> lights)
        {
            _lights = lights;
        }

        public IYeeLightState Update(Color color)
        {
            if (!Utils.IsWhiteTone(color))
            {
                return ColorState.Update(color);
            }
            
            if (Utils.IsBlack(color))
            {
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
                x.SetTemperature(6500);
                x.SetBrightness(color.R * 100 / 255);
            });
        }
    }

    internal static class Utils
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