using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public class MemoryReader : IDisposable
    {
        private Process process;
        private IntPtr processHandle;
        private IntPtr moduleAddress;
        private bool is64Bit = false;

        public MemoryReader(Process process)
        {
            this.process = process;
            processHandle = Utils.MemoryUtils.OpenProcess(0x0010, false, process.Id);
            moduleAddress = process.MainModule.BaseAddress;
        }

        public MemoryReader(string ProcessName)
        {
            Process process = Process.GetProcessesByName(ProcessName).FirstOrDefault();
            processHandle = Utils.MemoryUtils.OpenProcess(0x0010, false, process.Id);
            moduleAddress = process.MainModule.BaseAddress;
        }

        public int ReadInt(IntPtr address)
        {
            int bytesRead = 0;

            return BitConverter.ToInt32(Utils.MemoryUtils.ReadMemory(processHandle, address, 4, out bytesRead), 0);
        }

        public int ReadInt(int baseAddress, int[] offsets)
        {
            return ReadInt(CalculateAddress(baseAddress, offsets));
        }

        public long ReadLong(IntPtr address)
        {
            int bytesRead = 0;

            return BitConverter.ToInt64(Utils.MemoryUtils.ReadMemory(processHandle, address, 8, out bytesRead), 0);
        }

        public long ReadLong(int baseAddress, int[] offsets)
        {
            return ReadLong(CalculateAddress(baseAddress, offsets));
        }

        public float ReadFloat(IntPtr address)
        {
            int bytesRead = 0;

            return BitConverter.ToSingle(Utils.MemoryUtils.ReadMemory(processHandle, address, 4, out bytesRead), 0);
        }

        public float ReadFloat(int baseAddress, int[] offsets)
        {
            return ReadFloat(CalculateAddress(baseAddress, offsets));
        }

        public double ReadDouble(IntPtr address)
        {
            int bytesRead = 0;

            return BitConverter.ToDouble(Utils.MemoryUtils.ReadMemory(processHandle, address, 8, out bytesRead), 0);
        }

        public double ReadDouble(int baseAddress, int[] offsets)
        {
            return ReadDouble(CalculateAddress(baseAddress, offsets));
        }

        private IntPtr CalculateAddress(int baseAddress, int[] offsets)
        {
            IntPtr currentAddress = IntPtr.Add(moduleAddress, baseAddress);
            currentAddress = (IntPtr)ReadInt(currentAddress);

            for (int x = 0; x < offsets.Length - 1; x++)
            {
                currentAddress = IntPtr.Add(currentAddress, offsets[x]);
                currentAddress = (IntPtr)ReadInt(currentAddress);
            }

            currentAddress = IntPtr.Add(currentAddress, offsets[offsets.Length - 1]);

            return currentAddress;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Utils.MemoryUtils.CloseHandle(processHandle);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MemoryReader() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
