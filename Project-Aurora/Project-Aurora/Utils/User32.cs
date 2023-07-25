using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Aurora.Utils;

internal static class User32
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

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr CallWindowProc(nint lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    internal static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();
    
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SetWindowLongPtr(nint hWnd, int nIndex, IntPtr dwNewLong);

    internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
        int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    internal static extern bool GetLastInputInfo(ref tagLASTINPUTINFO plii);

    [DllImport("user32.dll")]
    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowTextLength(HandleRef hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);
    
    /// <remarks>
    ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
    /// </remarks>
    [DllImport("user32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    internal static extern int UnhookWindowsHookEx(IntPtr idHook);

    internal struct tagLASTINPUTINFO
    {
        public uint cbSize;
        public Int32 dwTime;
    }
}