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

        public MemoryReader(Process process, bool is64Bit)
        {
            this.process = process;
            processHandle = Utils.MemoryUtils.OpenProcess(0x0010, false, process.Id);
            moduleAddress = process.MainModule.BaseAddress;
            this.is64Bit = is64Bit;
        }

        public MemoryReader(string ProcessName, bool is64Bit) : this(Process.GetProcessesByName(ProcessName).FirstOrDefault(), is64Bit) { }

        public MemoryReader(Process process, ProcessModule module, bool is64Bit)
        {
            this.process = process;
            processHandle = Utils.MemoryUtils.OpenProcess(0x0010, false, process.Id);
            try
            {
                moduleAddress = module.BaseAddress;
            }
            catch (System.NullReferenceException)
            {
                moduleAddress = process.MainModule.BaseAddress;
            }
            this.is64Bit = is64Bit;
        }

        public MemoryReader(Process process, string module, bool is64Bit)
        {
            ProcessModuleCollection modules = process.Modules;
            ProcessModule dll = null;
            foreach (ProcessModule i in modules)
            {
                if (i.ModuleName == module)
                {
                    dll = i;
                    break;
                }
            }

            this.process = process;
            processHandle = Utils.MemoryUtils.OpenProcess(0x0010, false, process.Id);
            try
            {
                moduleAddress = dll.BaseAddress;
            }
            catch (System.NullReferenceException)
            {
                moduleAddress = process.MainModule.BaseAddress;
            }
            this.is64Bit = is64Bit;
        }

        public int ReadInt(IntPtr address) => BitConverter.ToInt32(MemoryUtils.ReadMemory(processHandle, address, 4, out int bytesRead), 0);
        public int ReadInt(int baseAddress, int[] offsets) => ReadInt(CalculateAddress(baseAddress, offsets));
        public int ReadInt(PointerData pointerData) => ReadInt(pointerData.baseAddress, pointerData.pointers);

        public long ReadLong(IntPtr address) => BitConverter.ToInt64(MemoryUtils.ReadMemory(processHandle, address, 8, out int bytesRead), 0);
        public long ReadLong(int baseAddress, int[] offsets) => ReadLong(CalculateAddress(baseAddress, offsets));
        public long ReadLine(PointerData pointerData) => ReadLong(pointerData.baseAddress, pointerData.pointers);

        public float ReadFloat(IntPtr address) => BitConverter.ToSingle(MemoryUtils.ReadMemory(processHandle, address, 4, out int bytesRead), 0);
        public float ReadFloat(int baseAddress, int[] offsets) => ReadFloat(CalculateAddress(baseAddress, offsets));
        public float ReadFloat(PointerData pointerData) => ReadFloat(pointerData.baseAddress, pointerData.pointers);

        public double ReadDouble(IntPtr address) => BitConverter.ToDouble(MemoryUtils.ReadMemory(processHandle, address, 8, out int bytesRead), 0);
        public double ReadDouble(int baseAddress, int[] offsets) => ReadDouble(CalculateAddress(baseAddress, offsets));
        public double ReadDouble(PointerData pointerData) => ReadDouble(pointerData.baseAddress, pointerData.pointers);

        private IntPtr CalculateAddress(int baseAddress, int[] offsets)
        {
            IntPtr currentAddress = IntPtr.Add(moduleAddress, baseAddress);            
            for (int x = 0; x < offsets.Length; x++) {
                currentAddress = (IntPtr)(is64Bit ? ReadLong(currentAddress) : ReadInt(currentAddress));
                currentAddress = IntPtr.Add(currentAddress, offsets[x]);
            }
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

    public class PointerData {
        public int baseAddress { get; set; }
        public int[] pointers { get; set; }
    }
}