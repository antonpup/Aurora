using System.IO.MemoryMappedFiles;
using Common;

namespace AurorDeviceManager;

public class SharedDeviceColor : IDisposable
{
    public int Count => Constants.MaxKeyId;

    private readonly MemoryMappedFile _mmf = MemoryMappedFile.OpenExisting("DeviceLedMap");
    
    private static readonly int ElementSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(SimpleColor));
    private readonly EventWaitHandle _deviceUpdatedHandle = EventWaitHandle.OpenExisting("DeviceUpdated");
    private readonly MemoryMappedViewAccessor _accessor;

    public SharedDeviceColor()
    {
        _accessor = _mmf.CreateViewAccessor();
    }

    public SimpleColor ReadElement(int index)
    {
        // Create a MemoryMappedViewAccessor to read data
        // Calculate the offset for the specified element
        long offset = index * ElementSize;

        // Read the data at the calculated offset
        _accessor.Read(offset, out SimpleColor result);

        return result;
    }

    public void WaitForUpdate()
    {
        _deviceUpdatedHandle.WaitOne();
    }

    public void Dispose()
    {
        _mmf.Dispose();
        _deviceUpdatedHandle.Dispose();
        _accessor.Dispose();
    }
}