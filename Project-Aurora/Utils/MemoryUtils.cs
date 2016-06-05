using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public class MemoryUtils
    {
        const int PROCESS_WM_READ = 0x0010;

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
}
