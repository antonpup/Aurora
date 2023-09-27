using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Aurora.Utils;
using Common.Devices;
using JetBrains.Annotations;
using Linearstar.Windows.RawInput.Native;

namespace Aurora.Modules.Inputs;

public class KeyboardKeyEvent : EventArgs
{
    public Keys Key { get; }
    private bool IsE0 { get; }
    private DeviceKeys? _deviceKey;

    public KeyboardKeyEvent(Keys key, RawKeyboardFlags flags)
    {
        Key = key;
        IsE0 = flags.HasFlag(RawKeyboardFlags.KeyE0);
    }

    public DeviceKeys GetDeviceKey()
    {
        return _deviceKey ??= KeyUtils.GetDeviceKey(Key, IsE0);
    }
}

public class MouseKeyEvent : EventArgs
{
    public MouseButtons Key { get; }

    public MouseKeyEvent(MouseButtons key)
    {
        Key = key;
    }
}

public class MouseScrollEvent : EventArgs
{
    public int WheelDelta { get; }

    public MouseScrollEvent(int wheelDelta)
    {
        WheelDelta = wheelDelta;
    }
}

public interface IInputEvents : IDisposable
{
    /// <summary>
    /// Event for a Key pressed Down on a keyboard
    /// </summary>
    event EventHandler<KeyboardKeyEvent> KeyDown;

    /// <summary>
    /// Event for a Key released on a keyboard
    /// </summary>
    event EventHandler<KeyboardKeyEvent> KeyUp;

    /// <summary>
    /// Event that fires when a mouse button is pressed down.
    /// </summary>
    event EventHandler<MouseKeyEvent> MouseButtonDown;

    /// <summary>
    /// Event that fires when a mouse button is released.
    /// </summary>
    event EventHandler<MouseKeyEvent> MouseButtonUp;

    /// <summary>
    /// Event that fires when the mouse scroll wheel is scrolled.
    /// </summary>
    event EventHandler<MouseScrollEvent> Scroll;

    IReadOnlyList<Keys> PressedKeys { get; }
    IReadOnlyList<MouseButtons> PressedButtons { get; }
    bool Shift { get; }
    bool Alt { get; }
    bool Control { get; }
    bool Windows { get; }

    [Pure]
    public TimeSpan GetTimeSinceLastInput();
}