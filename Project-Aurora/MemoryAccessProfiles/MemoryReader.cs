using System.Diagnostics;

namespace MemoryAccessProfiles;

public sealed class MemoryReader : IDisposable
{
    private readonly IntPtr _processHandle;
    private readonly IntPtr _moduleAddress;
    private readonly bool _is64Bit;

    public MemoryReader(Process process, bool is64Bit)
    {
        _processHandle = MemoryUtils.OpenProcess(0x0010, false, process.Id);
        _moduleAddress = process.MainModule.BaseAddress;
        _is64Bit = is64Bit;
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

        _processHandle = MemoryUtils.OpenProcess(0x0010, false, process.Id);
        try
        {
            _moduleAddress = dll.BaseAddress;
        }
        catch (NullReferenceException)
        {
            _moduleAddress = process.MainModule.BaseAddress;
        }
        this._is64Bit = is64Bit;
    }

    public int ReadInt(IntPtr address) => BitConverter.ToInt32(MemoryUtils.ReadMemory(_processHandle, address, 4, out _), 0);
    public int ReadInt(int baseAddress, int[] offsets) => ReadInt(CalculateAddress(baseAddress, offsets));
    public int ReadInt(PointerData pointerData) => ReadInt(pointerData.baseAddress, pointerData.pointers);

    public long ReadLong(IntPtr address) => BitConverter.ToInt64(MemoryUtils.ReadMemory(_processHandle, address, 8, out _), 0);
    public long ReadLong(int baseAddress, int[] offsets) => ReadLong(CalculateAddress(baseAddress, offsets));
    public long ReadLine(PointerData pointerData) => ReadLong(pointerData.baseAddress, pointerData.pointers);

    public float ReadFloat(IntPtr address) => BitConverter.ToSingle(MemoryUtils.ReadMemory(_processHandle, address, 4, out _), 0);
    public float ReadFloat(int baseAddress, int[] offsets) => ReadFloat(CalculateAddress(baseAddress, offsets));
    public float ReadFloat(PointerData pointerData) => ReadFloat(pointerData.baseAddress, pointerData.pointers);

    private IntPtr CalculateAddress(int baseAddress, int[] offsets)
    {
        IntPtr currentAddress = IntPtr.Add(_moduleAddress, baseAddress);            
        foreach (var t in offsets)
        {
            currentAddress = (IntPtr)(_is64Bit ? ReadLong(currentAddress) : ReadInt(currentAddress));
            currentAddress = IntPtr.Add(currentAddress, t);
        }
        return currentAddress;
    }

    #region IDisposable Support
    private bool _disposed; // To detect redundant calls

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            MemoryUtils.CloseHandle(_processHandle);
        }

        _disposed = true;
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
    }
    #endregion

}

public class PointerData {
    public int baseAddress { get; set; }
    public int[] pointers { get; set; }
}