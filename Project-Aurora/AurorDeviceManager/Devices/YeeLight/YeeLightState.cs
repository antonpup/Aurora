using System.Drawing;
using YeeLightAPI.YeeLightConstants;

namespace AurorDeviceManager.Devices.YeeLight;

public interface IYeeLightState
{
    void InitState();
        
    IYeeLightState Update(Color color);
}

static class YeeLightStateBuilder
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

internal class YeeLightStateOff : IYeeLightState
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
    
internal class YeeLightStateColor : IYeeLightState
{
    public IYeeLightState WhiteState;
    public IYeeLightState OffState;

    private readonly List<YeeLightAPI.YeeLightDevice> _lights;
    private readonly int _whiteCounterStart;

    private int _previousBrightness;
    private Color _previousColor;
    private int _whiteCounter;

    public YeeLightStateColor(List<YeeLightAPI.YeeLightDevice> lights, int whiteCounterStart)
    {
        _lights = lights;
        _whiteCounterStart = whiteCounterStart;
    }

    public void InitState()
    {
        //noop
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

        if (Utils.ShouldSendKeepAlive())
        {
            _lights.ForEach(device => device.SetPower(Constants.PowerStateParamValues.ON));
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
        if (!Utils.ShouldSendKeepAlive()) return this;
        _lights.ForEach(device => device.SetPower(Constants.PowerStateParamValues.ON));
        UpdateLights(targetColor);
        return this;
    }

    private List<Task> _tasks = new();
    private void UpdateLights(Color color)
    {
        _lights.ForEach(x =>
        {
            var colorAsync = x.SetColorAsync(color.R, color.G, color.B);
            _tasks.Add(colorAsync);
            var brightness = Math.Max(color.R, Math.Max(color.G, Math.Max(color.B, (short) 1))) * 100 / 255;
            if (_previousBrightness == brightness) return;
            _previousBrightness = brightness;
            var brightnessAsync = x.SetBrightnessAsync(brightness);
            _tasks.Add(brightnessAsync);
        });
        Task.WhenAll(_tasks).Wait();
        _tasks.Clear();
    }
}
    
internal partial class YeeLightStateWhite : IYeeLightState
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
        var tasks = _lights.ConvertAll(x => x.SetColorTemperatureAsync(6500));
        Task.WhenAll(tasks).Wait();
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
                InitState();
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
        var tasks = _lights.ConvertAll(x => x.SetBrightnessAsync(color.R * 100 / 255));
        Task.WhenAll(tasks).Wait();
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
        return color is { R: 0, G: 0, B: 0 };
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