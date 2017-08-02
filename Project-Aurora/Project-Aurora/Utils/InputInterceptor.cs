using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Aurora.Utils;
using Gma.System.MouseKeyHook.WinApi;
using Microsoft.Win32.SafeHandles;
using SharpDX.RawInput;

namespace Aurora
{
    /// <summary>
    /// Class for intercepting input events
    /// </summary>
    internal sealed class InputInterceptor : IDisposable
    {
        private readonly MessagePumpThread thread = new MessagePumpThread();
        private readonly HookProcedure hookProcedure;

        public event EventHandler<InputEventData> Input;

        public InputInterceptor()
        {
            try
            {
                // Store delegate to prevent it's been garbage collected
                hookProcedure = LowLevelHookProc;
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
            using (new ThreadPriorityChanger(ThreadPriority.AboveNormal,
                ThreadPriorityChanger.ThreadPriority.THREAD_PRIORITY_TIME_CRITICAL))
            using (var lowLevelHookHandle = HookNativeMethods.SetWindowsHookEx(HookIds.WH_KEYBOARD_LL, hookProcedure,
                Process.GetCurrentProcess().MainModule.BaseAddress, 0))
            {
                if (lowLevelHookHandle.IsInvalid)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                thread.EnterMessageLoop();
            }
        }

        private IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var passThrough = nCode != 0;
            if (passThrough)
            {
                return HookNativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }

            var callbackData = new InputEventData
            {
                Message = (int)wParam,
                Data = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct))
            };

            //Debug.WriteLine(
            //    $@"LowLevelInput {(Keys)callbackData.Data.VirtualKeyCode} {callbackData.Data.ScanCode} {callbackData.Data.Flags} {
            //            KeyUtils.GetDeviceKey(
            //                (Keys)callbackData.Data.VirtualKeyCode, callbackData.Data.ScanCode,
            //                ((ScanCodeFlags)callbackData.Data.Flags).HasFlag(ScanCodeFlags.E0))
            //        }", "InputEvents");

            Input?.Invoke(this, callbackData);

            if (callbackData.Intercepted)
            {
                return new IntPtr(-1);
            }
            return HookNativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public sealed class InputEventData
        {
            public KeyboardHookStruct Data { get; set; }
            public int Message { get; set; }
            public bool Intercepted { get; set; }
            public bool KeyUp => Message == Messages.WM_KEYUP || Message == Messages.WM_SYSKEYUP;
            public bool KeyDown => Message == Messages.WM_KEYDOWN || Message == Messages.WM_SYSKEYDOWN;
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

        private static class HookNativeMethods
        {
            /// <summary>
            ///     The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
            ///     A hook procedure can call this function either before or after processing the hook information.
            /// </summary>
            /// <param name="idHook">This parameter is ignored.</param>
            /// <param name="nCode">[in] Specifies the hook code passed to the current hook procedure.</param>
            /// <param name="wParam">[in] Specifies the wParam value passed to the current hook procedure.</param>
            /// <param name="lParam">[in] Specifies the lParam value passed to the current hook procedure.</param>
            /// <returns>This value is returned by the next hook procedure in the chain.</returns>
            /// <remarks>
            ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
            /// </remarks>
            [DllImport("user32.dll", CharSet = CharSet.Auto,
                CallingConvention = CallingConvention.StdCall)]
            internal static extern IntPtr CallNextHookEx(
                IntPtr idHook,
                int nCode,
                IntPtr wParam,
                IntPtr lParam);

            /// <summary>
            ///     The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain.
            ///     You would install a hook procedure to monitor the system for certain types of events. These events
            ///     are associated either with a specific thread or with all threads in the same desktop as the calling thread.
            /// </summary>
            /// <param name="idHook">
            ///     [in] Specifies the type of hook procedure to be installed. This parameter can be one of the following values.
            /// </param>
            /// <param name="lpfn">
            ///     [in] Pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a
            ///     thread created by a different process, the lpfn parameter must point to a hook procedure in a dynamic-link
            ///     library (DLL). Otherwise, lpfn can point to a hook procedure in the code associated with the current process.
            /// </param>
            /// <param name="hMod">
            ///     [in] Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.
            ///     The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread created by
            ///     the current process and if the hook procedure is within the code associated with the current process.
            /// </param>
            /// <param name="dwThreadId">
            ///     [in] Specifies the identifier of the thread with which the hook procedure is to be associated.
            ///     If this parameter is zero, the hook procedure is associated with all existing threads running in the
            ///     same desktop as the calling thread.
            /// </param>
            /// <returns>
            ///     If the function succeeds, the return value is the handle to the hook procedure.
            ///     If the function fails, the return value is NULL. To get extended error information, call GetLastError.
            /// </returns>
            /// <remarks>
            ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
            /// </remarks>
            [DllImport("user32.dll", CharSet = CharSet.Auto,
                CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern HookProcedureHandle SetWindowsHookEx(
                int idHook,
                HookProcedure lpfn,
                IntPtr hMod,
                int dwThreadId);

            /// <summary>
            ///     The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx
            ///     function.
            /// </summary>
            /// <param name="idHook">
            ///     [in] Handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to
            ///     SetWindowsHookEx.
            /// </param>
            /// <returns>
            ///     If the function succeeds, the return value is nonzero.
            ///     If the function fails, the return value is zero. To get extended error information, call GetLastError.
            /// </returns>
            /// <remarks>
            ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
            /// </remarks>
            [DllImport("user32.dll", CharSet = CharSet.Auto,
                CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern int UnhookWindowsHookEx(IntPtr idHook);
        }

        private static class HookIds
        {
            /// <summary>
            ///     Installs a hook procedure that monitors mouse messages. For more information, see the MouseProc hook procedure.
            /// </summary>
            internal const int WH_MOUSE = 7;

            /// <summary>
            ///     Installs a hook procedure that monitors keystroke messages. For more information, see the KeyboardProc hook
            ///     procedure.
            /// </summary>
            internal const int WH_KEYBOARD = 2;

            /// <summary>
            ///     Windows NT/2000/XP/Vista/7: Installs a hook procedure that monitors low-level mouse input events.
            /// </summary>
            internal const int WH_MOUSE_LL = 14;

            /// <summary>
            ///     Windows NT/2000/XP/Vista/7: Installs a hook procedure that monitors low-level keyboard  input events.
            /// </summary>
            internal const int WH_KEYBOARD_LL = 13;
        }

        /// <summary>
        ///     The KeyboardHookStruct structure contains information about a low-level keyboard input event.
        /// </summary>
        /// <remarks>
        ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookstructures/cwpstruct.asp
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KeyboardHookStruct
        {
            /// <summary>
            ///     Specifies a virtual-key code. The code must be a value in the range 1 to 254.
            /// </summary>
            public int VirtualKeyCode;

            /// <summary>
            ///     Specifies a hardware scan code for the key.
            /// </summary>
            public int ScanCode;

            /// <summary>
            ///     Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
            /// </summary>
            public int Flags;

            /// <summary>
            ///     Specifies the Time stamp for this message.
            /// </summary>
            public int Time;

            /// <summary>
            ///     Specifies extra information associated with the message.
            /// </summary>
            public int ExtraInfo;
        }

        private sealed class HookProcedureHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private static bool _closing;

            static HookProcedureHandle()
            {
                Application.ApplicationExit += (sender, e) => { _closing = true; };
            }

            public HookProcedureHandle()
                : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                //NOTE Calling Unhook during processexit causes delay
                if (_closing) return true;
                return HookNativeMethods.UnhookWindowsHookEx(handle) != 0;
            }
        }

        private static class Messages
        {
            //values from Winuser.h in Microsoft SDK.

            /// <summary>
            ///     The WM_MOUSEMOVE message is posted to a window when the cursor moves.
            /// </summary>
            public const int WM_MOUSEMOVE = 0x200;

            /// <summary>
            ///     The WM_LBUTTONDOWN message is posted when the user presses the left mouse button
            /// </summary>
            public const int WM_LBUTTONDOWN = 0x201;

            /// <summary>
            ///     The WM_RBUTTONDOWN message is posted when the user presses the right mouse button
            /// </summary>
            public const int WM_RBUTTONDOWN = 0x204;

            /// <summary>
            ///     The WM_MBUTTONDOWN message is posted when the user presses the middle mouse button
            /// </summary>
            public const int WM_MBUTTONDOWN = 0x207;

            /// <summary>
            ///     The WM_LBUTTONUP message is posted when the user releases the left mouse button
            /// </summary>
            public const int WM_LBUTTONUP = 0x202;

            /// <summary>
            ///     The WM_RBUTTONUP message is posted when the user releases the right mouse button
            /// </summary>
            public const int WM_RBUTTONUP = 0x205;

            /// <summary>
            ///     The WM_MBUTTONUP message is posted when the user releases the middle mouse button
            /// </summary>
            public const int WM_MBUTTONUP = 0x208;

            /// <summary>
            ///     The WM_LBUTTONDBLCLK message is posted when the user double-clicks the left mouse button
            /// </summary>
            public const int WM_LBUTTONDBLCLK = 0x203;

            /// <summary>
            ///     The WM_RBUTTONDBLCLK message is posted when the user double-clicks the right mouse button
            /// </summary>
            public const int WM_RBUTTONDBLCLK = 0x206;

            /// <summary>
            ///     The WM_RBUTTONDOWN message is posted when the user presses the right mouse button
            /// </summary>
            public const int WM_MBUTTONDBLCLK = 0x209;

            /// <summary>
            ///     The WM_MOUSEWHEEL message is posted when the user presses the mouse wheel.
            /// </summary>
            public const int WM_MOUSEWHEEL = 0x020A;

            /// <summary>
            ///     The WM_XBUTTONDOWN message is posted when the user presses the first or second X mouse
            ///     button.
            /// </summary>
            public const int WM_XBUTTONDOWN = 0x20B;

            /// <summary>
            ///     The WM_XBUTTONUP message is posted when the user releases the first or second X  mouse
            ///     button.
            /// </summary>
            public const int WM_XBUTTONUP = 0x20C;

            /// <summary>
            ///     The WM_XBUTTONDBLCLK message is posted when the user double-clicks the first or second
            ///     X mouse button.
            /// </summary>
            /// <remarks>Only windows that have the CS_DBLCLKS style can receive WM_XBUTTONDBLCLK messages.</remarks>
            public const int WM_XBUTTONDBLCLK = 0x20D;

            /// <summary>
            ///     The WM_MOUSEHWHEEL message Sent to the active window when the mouse's horizontal scroll
            ///     wheel is tilted or rotated.
            /// </summary>
            public const int WM_MOUSEHWHEEL = 0x20E;

            /// <summary>
            ///     The WM_KEYDOWN message is posted to the window with the keyboard focus when a non-system
            ///     key is pressed. A non-system key is a key that is pressed when the ALT key is not pressed.
            /// </summary>
            public const int WM_KEYDOWN = 0x100;

            /// <summary>
            ///     The WM_KEYUP message is posted to the window with the keyboard focus when a non-system
            ///     key is released. A non-system key is a key that is pressed when the ALT key is not pressed,
            ///     or a keyboard key that is pressed when a window has the keyboard focus.
            /// </summary>
            public const int WM_KEYUP = 0x101;

            /// <summary>
            ///     The WM_SYSKEYDOWN message is posted to the window with the keyboard focus when the user
            ///     presses the F10 key (which activates the menu bar) or holds down the ALT key and then
            ///     presses another key. It also occurs when no window currently has the keyboard focus;
            ///     in this case, the WM_SYSKEYDOWN message is sent to the active window. The window that
            ///     receives the message can distinguish between these two contexts by checking the context
            ///     code in the lParam parameter.
            /// </summary>
            public const int WM_SYSKEYDOWN = 0x104;

            /// <summary>
            ///     The WM_SYSKEYUP message is posted to the window with the keyboard focus when the user
            ///     releases a key that was pressed while the ALT key was held down. It also occurs when no
            ///     window currently has the keyboard focus; in this case, the WM_SYSKEYUP message is sent
            ///     to the active window. The window that receives the message can distinguish between
            ///     these two contexts by checking the context code in the lParam parameter.
            /// </summary>
            public const int WM_SYSKEYUP = 0x105;
        }
    }
}