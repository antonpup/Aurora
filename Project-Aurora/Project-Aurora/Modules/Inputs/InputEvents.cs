using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Aurora.Utils;
using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;
using User32 = Aurora.Utils.User32;

namespace Aurora.Modules.Inputs;

/// <summary>
/// Class for subscribing to various HID input events
/// </summary>
public sealed class InputEvents : IInputEvents
{
    /// <summary>
    /// Event for a Key pressed Down on a keyboard
    /// </summary>
    public event EventHandler<KeyboardKeyEvent>? KeyDown;

    /// <summary>
    /// Event for a Key released on a keyboard
    /// </summary>
    public event EventHandler<KeyboardKeyEvent>? KeyUp;

    /// <summary>
    /// Event that fires when a mouse button is pressed down.
    /// </summary>
    public event EventHandler<MouseKeyEvent>? MouseButtonDown;

    /// <summary>
    /// Event that fires when a mouse button is released.
    /// </summary>
    public event EventHandler<MouseKeyEvent>? MouseButtonUp;

    /// <summary>
    /// Event that fires when the mouse scroll wheel is scrolled.
    /// </summary>
    public event EventHandler<MouseScrollEvent>? Scroll;

    private readonly List<Keys> _pressedKeySequence = new();

    private readonly List<MouseButtons> _pressedMouseButtons = new();

    private bool _disposed;

    public IReadOnlyList<Keys> PressedKeys => new ReadOnlyCollection<Keys>(_pressedKeySequence.ToArray());

    public IReadOnlyList<MouseButtons> PressedButtons => new ReadOnlyCollection<MouseButtons>(_pressedMouseButtons.ToArray());

    private static readonly Keys[] ShiftKeys = {Keys.ShiftKey, Keys.RShiftKey, Keys.LShiftKey};
    public bool Shift => ShiftKeys.Any(PressedKeys.Contains);

    private static readonly Keys[] AltKeys = {Keys.Menu, Keys.RMenu, Keys.LMenu};
    public bool Alt => AltKeys.Any(PressedKeys.Contains);

    private static readonly Keys[] CtrlKeys = {Keys.ControlKey, Keys.RControlKey, Keys.LControlKey};
    public bool Control => CtrlKeys.Any(PressedKeys.Contains);

    private static readonly Keys[] WinKeys = { Keys.LWin, Keys.RWin };
    public bool Windows => WinKeys.Any(PressedKeys.Contains);

    delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private readonly WndProc? _fnWndProcHook;
    private readonly nint _hWndProcHook;
    private readonly IntPtr _hWnd;

    public InputEvents()
    {
        _hWnd = User32.CreateWindowEx(0, "STATIC", "", 0x80000000, 0, 0,
            0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        _hWndProcHook = User32.GetWindowLongPtr(_hWnd, -4);

        // register the keyboard device and you can register device which you need like mouse
        RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink, _hWnd);
        RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, _hWnd);

        _fnWndProcHook = Hook;
        nint newLong = Marshal.GetFunctionPointerForDelegate(_fnWndProcHook);
        User32.SetWindowLongPtr(_hWnd, -4, newLong);
    }
    
    private IntPtr Hook(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam)
    {
        const int wmInput = 0x00FF;

        // You can read inputs by processing the WM_INPUT message.
        if (msg != wmInput) return User32.CallWindowProc(_hWndProcHook, _hWnd, msg, wparam, lparam);
        // Create an RawInputData from the handle stored in lParam.
        var data = RawInputData.FromHandle(lparam);

        // The data will be an instance of either RawInputMouseData, RawInputKeyboardData, or RawInputHidData.
        // They contain the raw input data in their properties.
        switch (data)
        {
            case RawInputMouseData mouse:
                DeviceOnMouseInput(mouse.Mouse);
                break;
            case RawInputKeyboardData keyboard:
                DeviceOnKeyboardInput(keyboard.Keyboard);
                break;
        }

        //return User32.CallWindowProc(_hWndProcHook, _hWnd, msg, wparam, lparam);
        return IntPtr.Zero;
    }

    private void DeviceOnKeyboardInput(RawKeyboard keyboardData)
    {
        try
        {
            var key = KeyUtils.CorrectRawInputData(keyboardData.VirutalKey, keyboardData.ScanCode, keyboardData.Flags);
            if ((int)key == 255)
            {
                // discard "fake keys" which are part of an escaped sequence
                return;
            }

            if ((keyboardData.Flags & RawKeyboardFlags.Up) != 0)
            {
                _pressedKeySequence.RemoveAll(k => k == key);
                KeyUp?.Invoke(this, new KeyboardKeyEvent(key, keyboardData.Flags));
            }
            else
            {
                if (!_pressedKeySequence.Contains(key))
                    _pressedKeySequence.Add(key);
                KeyDown?.Invoke(this, new KeyboardKeyEvent(key, keyboardData.Flags));
            }
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception while handling keyboard input");
        }
    }

    /// <summary>
    /// Handles a SharpDX MouseInput event and fires the relevant InputEvents event (Scroll, MouseButtonDown or MouseButtonUp).
    /// </summary>
    private void DeviceOnMouseInput(RawMouse mouseData)
    {
        // Scrolling
        if (mouseData.ButtonData != 0)
        {
            if (mouseData.Buttons == RawMouseButtonFlags.MouseWheel)
                Scroll?.Invoke(this, new MouseScrollEvent(mouseData.ButtonData));
            return;
        }

        var (button, isDown) = mouseData.Buttons switch
        {
            RawMouseButtonFlags.LeftButtonDown => (MouseButtons.Left, true),
            RawMouseButtonFlags.LeftButtonUp => (MouseButtons.Left, false),
            RawMouseButtonFlags.MiddleButtonDown => (MouseButtons.Middle, true),
            RawMouseButtonFlags.MiddleButtonUp => (MouseButtons.Middle, false),
            RawMouseButtonFlags.RightButtonDown => (MouseButtons.Right, true),
            RawMouseButtonFlags.RightButtonUp => (MouseButtons.Right, false),
            _ => (MouseButtons.Left, false)
        };

        if (isDown)
        {
            if (!_pressedMouseButtons.Contains(button))
                _pressedMouseButtons.Add(button);
            MouseButtonDown?.Invoke(this, new MouseKeyEvent(button));
        }
        else
        {
            _pressedMouseButtons.Remove(button);
            MouseButtonUp?.Invoke(this, new MouseKeyEvent(button));
        }
    }

    public TimeSpan GetTimeSinceLastInput() {
        var inf = new User32.tagLASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<User32.tagLASTINPUTINFO>() };
        return !User32.GetLastInputInfo(ref inf) ?
            new TimeSpan(0) :
            new TimeSpan(0, 0, 0, 0, Environment.TickCount - inf.dwTime);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}