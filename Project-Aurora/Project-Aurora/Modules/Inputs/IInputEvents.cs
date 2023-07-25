using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Aurora.Modules.Inputs;

public class KeyEvent : EventArgs
{
    public Keys Key { get; }

    public KeyEvent(Keys key)
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
    event EventHandler<KeyEvent> KeyDown;

    /// <summary>
    /// Event for a Key released on a keyboard
    /// </summary>
    event EventHandler<KeyEvent> KeyUp;

    /// <summary>
    /// Event that fires when a mouse button is pressed down.
    /// </summary>
    event EventHandler<KeyEvent> MouseButtonDown;

    /// <summary>
    /// Event that fires when a mouse button is released.
    /// </summary>
    event EventHandler<KeyEvent> MouseButtonUp;

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
}