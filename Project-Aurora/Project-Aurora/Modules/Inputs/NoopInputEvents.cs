using System;
using System.Windows.Forms;
using SharpDX.RawInput;

namespace Aurora.Modules.Inputs;

public class NoopInputEvents : IInputEvents
{
    public void Dispose()
    {
        //noop
    }

    public event EventHandler<KeyboardInputEventArgs> KeyDown;
    public event EventHandler<KeyboardInputEventArgs> KeyUp;
    public event EventHandler<MouseInputEventArgs> MouseButtonDown;
    public event EventHandler<MouseInputEventArgs> MouseButtonUp;
    public event EventHandler<MouseInputEventArgs> Scroll;
    public Keys[] PressedKeys { get; } = Array.Empty<Keys>();
    public MouseButtons[] PressedButtons { get; } = Array.Empty<MouseButtons>();
    public bool Shift => false;
    public bool Alt => false;
    public bool Control => false;
    public bool Windows => false;
}