using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Aurora.Utils;
using Microsoft.Win32;
using SharpDX.Multimedia;
using SharpDX.RawInput;

namespace Aurora
{
    /// <summary>
    /// Class for subscribing to various HID input events
    /// </summary>
    public sealed class InputEvents : IDisposable
    {
        private readonly MessagePumpThread thread = new MessagePumpThread();

        /// <summary>
        /// Event for a Key pressed Down on a keyboard
        /// </summary>
        public event EventHandler<KeyboardInputEventArgs> KeyDown;

        /// <summary>
        /// Event for a Key released on a keyboard
        /// </summary>
        public event EventHandler<KeyboardInputEventArgs> KeyUp;

        /// <summary>
        /// Event that fires when a mouse button is pressed down.
        /// </summary>
        public event EventHandler<MouseInputEventArgs> MouseButtonDown;

        /// <summary>
        /// Event that fires when a mouse button is released.
        /// </summary>
        public event EventHandler<MouseInputEventArgs> MouseButtonUp;

        /// <summary>
        /// Event that fires when the mouse scroll wheel is scrolled.
        /// </summary>
        public event EventHandler<MouseInputEventArgs> Scroll;

        private readonly List<Keys> pressedKeySequence = new List<Keys>();

        private readonly List<MouseButtons> pressedMouseButtons = new List<MouseButtons>();

        public Keys[] PressedKeys
        {
            get { return pressedKeySequence.ToArray(); }
        }

        public MouseButtons[] PressedButtons {
            get { return pressedMouseButtons.ToArray(); }
        }

        public bool Shift => new[] {Keys.ShiftKey, Keys.RShiftKey, Keys.LShiftKey}
            .Any(PressedKeys.Contains);

        public bool Alt => new[] {Keys.Menu, Keys.RMenu, Keys.LMenu}
            .Any(PressedKeys.Contains);

        public bool Control => new[] {Keys.ControlKey, Keys.RControlKey, Keys.LControlKey}
            .Any(PressedKeys.Contains);

        public bool Windows => new[] { Keys.LWin, Keys.RWin }
            .Any(PressedKeys.Contains);

        public InputEvents()
        {
            try
            {
                thread.Start(MessagePumpInit);
                SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock || e.Reason == SessionSwitchReason.SessionUnlock)
                this.pressedKeySequence.Clear();
        }

        private void MessagePumpInit()
        {
            using (var dummyForm = new Form())
            using (new DisposableRawInputHook(UsagePage.Generic, UsageId.GenericKeyboard, UsageId.GenericMouse,
                DeviceFlags.InputSink, dummyForm.Handle, DeviceOnKeyboardInput, DeviceOnMouseInput))
            {
                thread.EnterMessageLoop();
            }
        }

        private void DeviceOnKeyboardInput(object sender, KeyboardInputEventArgs e)
        {
            if ((int)e.Key == 255)
            {
                // discard "fake keys" which are part of an escaped sequence
                return;
            }

            KeyUtils.CorrectRawInputData(e);

			//Debug.WriteLine($"RawInput {e.Key} {e.MakeCode} {e.ScanCodeFlags} {e.GetDeviceKey()}", "InputEvents");

			if (e.ScanCodeFlags.HasFlag(ScanCodeFlags.Break))
            {
                pressedKeySequence.RemoveAll(k => k == e.Key);
                KeyUp?.Invoke(sender, e);
            }
            else
            {
                if (!pressedKeySequence.Contains(e.Key))
                    pressedKeySequence.Add(e.Key);
                KeyDown?.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Handles a SharpDX MouseInput event and fires the relevant InputEvents event (Scroll, MouseButtonDown or MouseButtonUp).
        /// </summary>
        private void DeviceOnMouseInput(object sender, MouseInputEventArgs e) {
            if (e.WheelDelta != 0)
                Scroll?.Invoke(sender, e);

            if (e.IsMouseDownEvent()) {
                if (!pressedMouseButtons.Contains(e.GetMouseButton()))
                    pressedMouseButtons.Add(e.GetMouseButton());
                MouseButtonDown?.Invoke(sender, e);

            } else if (e.IsMouseUpEvent()) {
                pressedMouseButtons.Remove(e.GetMouseButton());
                MouseButtonUp?.Invoke(sender, e);
            }
        }

        private sealed class DisposableRawInputHook : IDisposable
        {
            private readonly UsagePage usagePage;
            private readonly UsageId usageIdKeyboard;
            private readonly UsageId usageIdMouse;
            private readonly EventHandler<KeyboardInputEventArgs> keyHandler;
            private readonly EventHandler<MouseInputEventArgs> mouseHandler;

            public DisposableRawInputHook(UsagePage usagePage, UsageId usageIdKeyboard, UsageId usageIdMouse, DeviceFlags flags, IntPtr target,
                EventHandler<KeyboardInputEventArgs> keyHandler, EventHandler<MouseInputEventArgs> mouseHandler)
            {
                this.usagePage = usagePage;
                this.usageIdKeyboard = usageIdKeyboard;
                this.usageIdMouse = usageIdMouse;
                this.keyHandler = keyHandler;
                this.mouseHandler = mouseHandler;

                Device.RegisterDevice(usagePage, usageIdKeyboard, flags, target);
                Device.KeyboardInput += keyHandler;

                Device.RegisterDevice(usagePage, usageIdMouse, flags, target);
                Device.MouseInput += mouseHandler;
            }

            public void Dispose()
            {
                Device.KeyboardInput -= keyHandler;
                Device.RegisterDevice(usagePage, usageIdKeyboard, DeviceFlags.Remove);

                Device.MouseInput -= mouseHandler;
                Device.RegisterDevice(usagePage, usageIdMouse, DeviceFlags.Remove);
            }
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                thread.Dispose();
            }
        }
    }
}