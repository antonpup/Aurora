using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Aurora.Utils;
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

        private readonly List<Keys> pressedKeySequence = new List<Keys>();

        public Keys[] PressedKeys
        {
            get { return pressedKeySequence.ToArray(); }
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
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private void MessagePumpInit()
        {
            using (var dummyForm = new Form())
            using (new DisposableRawInputHook(UsagePage.Generic, UsageId.GenericKeyboard,
                DeviceFlags.InputSink, dummyForm.Handle, DeviceOnKeyboardInput))
            {
                thread.EnterMessageLoop();
            }
        }

        private void DeviceOnKeyboardInput(object sender, KeyboardInputEventArgs e)
        {
            //Debug.WriteLine($"RawInput {e.Key} {e.MakeCode} {e.ScanCodeFlags} {e.GetDeviceKey()}", "InputEvents");

            if ((int)e.Key == 255)
            {
                // discard "fake keys" which are part of an escaped sequence
                return;
            }

            KeyUtils.CorrectRawInputData(e);

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

        private sealed class DisposableRawInputHook : IDisposable
        {
            private readonly UsagePage usagePage;
            private readonly UsageId usageId;
            private readonly EventHandler<KeyboardInputEventArgs> handler;

            public DisposableRawInputHook(UsagePage usagePage, UsageId usageId, DeviceFlags flags, IntPtr target,
                EventHandler<KeyboardInputEventArgs> handler)
            {
                this.usagePage = usagePage;
                this.usageId = usageId;
                this.handler = handler;

                Device.RegisterDevice(usagePage, usageId, flags, target);
                Device.KeyboardInput += handler;
            }

            public void Dispose()
            {
                Device.KeyboardInput -= handler;
                Device.RegisterDevice(usagePage, usageId, DeviceFlags.Remove);
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