using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Aurora.Utils;

internal static class User32
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr CallWindowProc(nint lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [Pure]
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

    [Pure]
    [DllImport("user32.dll")]
    internal static extern IntPtr GetForegroundWindow();
    
    [Pure]
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [Pure]
    [DllImport("user32.dll")]
    internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [Pure]
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SetWindowLongPtr(nint hWnd, int nIndex, IntPtr dwNewLong);

    internal delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
        int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]
    internal static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [Pure]
    [DllImport("user32.dll")]
    internal static extern bool GetLastInputInfo(ref tagLASTINPUTINFO plii);
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct tagLASTINPUTINFO
    {
        public uint cbSize;
        public Int32 dwTime;
    }
}