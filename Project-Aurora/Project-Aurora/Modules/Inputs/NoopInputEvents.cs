using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Aurora.Modules.Inputs;

public sealed class NoopInputEvents : IInputEvents
{
    public void Dispose()
    {
        //noop
    }

    public event EventHandler<KeyboardKeyEvent>? KeyDown;
    public event EventHandler<KeyboardKeyEvent>? KeyUp;
    public event EventHandler<MouseKeyEvent>? MouseButtonDown;
    public event EventHandler<MouseKeyEvent>? MouseButtonUp;
    public event EventHandler<MouseScrollEvent>? Scroll;
    public IReadOnlyList<Keys> PressedKeys { get; } = Array.Empty<Keys>();
    public IReadOnlyList<MouseButtons> PressedButtons { get; } = Array.Empty<MouseButtons>();
    public bool Shift => false;
    public bool Alt => false;
    public bool Control => false;
    public bool Windows => false;
}