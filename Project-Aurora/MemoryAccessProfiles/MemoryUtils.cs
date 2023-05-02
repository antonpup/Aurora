using System.Runtime.InteropServices;

namespace MemoryAccessProfiles;

public static class MemoryUtils
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    public static byte[] ReadMemory(IntPtr targetProcess, IntPtr address, int readCount, out int bytesRead)
    {
        byte[] buffer = new byte[readCount];

        ReadProcessMemory(targetProcess, address, buffer, readCount, out bytesRead);

        return buffer;
    }

    [DllImport("kernel32.dll")]
    public static extern int CloseHandle(IntPtr objectHandle);
}