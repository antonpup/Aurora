using System;
using System.Windows.Forms;
using SharpDX.RawInput;

namespace Aurora;

public interface IInputEvents : IDisposable
{
    /// <summary>
    /// Event for a Key pressed Down on a keyboard
    /// </summary>
    event EventHandler<KeyboardInputEventArgs> KeyDown;

    /// <summary>
    /// Event for a Key released on a keyboard
    /// </summary>
    event EventHandler<KeyboardInputEventArgs> KeyUp;

    /// <summary>
    /// Event that fires when a mouse button is pressed down.
    /// </summary>
    event EventHandler<MouseInputEventArgs> MouseButtonDown;

    /// <summary>
    /// Event that fires when a mouse button is released.
    /// </summary>
    event EventHandler<MouseInputEventArgs> MouseButtonUp;

    /// <summary>
    /// Event that fires when the mouse scroll wheel is scrolled.
    /// </summary>
    event EventHandler<MouseInputEventArgs> Scroll;

    Keys[] PressedKeys { get; }
    MouseButtons[] PressedButtons { get; }
    bool Shift { get; }
    bool Alt { get; }
    bool Control { get; }
    bool Windows { get; }
}