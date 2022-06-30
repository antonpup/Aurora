using System;
using System.Diagnostics;

namespace Aurora.Utils
{
    public class MemoryReader : IDisposable
    {
        private IntPtr processHandle;
        private IntPtr moduleAddress;
        private bool is64Bit;

        public MemoryReader(Process process, bool is64Bit)
        {
            processHandle = MemoryUtils.OpenProcess(0x0010, false, process.Id);
            moduleAddress = process.MainModule.BaseAddress;
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

            processHandle = MemoryUtils.OpenProcess(0x0010, false, process.Id);
            try
            {
                moduleAddress = dll.BaseAddress;
            }
            catch (NullReferenceException)
            {
                moduleAddress = process.MainModule.BaseAddress;
            }
            this.is64Bit = is64Bit;
        }

        public int ReadInt(IntPtr address) => BitConverter.ToInt32(MemoryUtils.ReadMemory(processHandle, address, 4, out _), 0);
        public int ReadInt(int baseAddress, int[] offsets) => ReadInt(CalculateAddress(baseAddress, offsets));
        public int ReadInt(PointerData pointerData) => ReadInt(pointerData.baseAddress, pointerData.pointers);

        public long ReadLong(IntPtr address) => BitConverter.ToInt64(MemoryUtils.ReadMemory(processHandle, address, 8, out _), 0);
        public long ReadLong(int baseAddress, int[] offsets) => ReadLong(CalculateAddress(baseAddress, offsets));
        public long ReadLine(PointerData pointerData) => ReadLong(pointerData.baseAddress, pointerData.pointers);

        public float ReadFloat(IntPtr address) => BitConverter.ToSingle(MemoryUtils.ReadMemory(processHandle, address, 4, out _), 0);
        public float ReadFloat(int baseAddress, int[] offsets) => ReadFloat(CalculateAddress(baseAddress, offsets));
        public float ReadFloat(PointerData pointerData) => ReadFloat(pointerData.baseAddress, pointerData.pointers);

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
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    MemoryUtils.CloseHandle(processHandle);
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