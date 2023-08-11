using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Aurora.Modules.Inputs;

public sealed class NoopInputEvents : IInputEvents
{
    private static readonly TimeSpan NoopLastInput = new(0);

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

    public TimeSpan GetTimeSinceLastInput() {
        return NoopLastInput;
    }
}