using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Common;
using Common.Devices;

namespace Aurora.Devices;

public class SharedDeviceColor : IDisposable
{
    public int Count { get; }

    private readonly MemoryMappedFile _mmf;
    private readonly int _elementSize;
    private readonly EventWaitHandle _deviceUpdatedHandle;
    private readonly MemoryMappedViewAccessor _accessor;

    public SharedDeviceColor()
    {
        Count = Constants.MaxKeyId;
        _elementSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(SimpleColor));

        // Calculate the total size for the MemoryMappedFile
        long totalSize = Count * _elementSize;

        // Create a MemoryMappedFile
        _mmf = MemoryMappedFile.CreateOrOpen("DeviceLedMap", totalSize);
        // Create a MemoryMappedViewAccessor to write data
        using var accessor = _mmf.CreateViewAccessor();
        // Initialize and write data to the MemoryMappedFile
        var data = new SimpleColor();
        for (var i = 0; i < Count; i++)
        {
            // Calculate the offset for the current element
            long offset = i * _elementSize;

            // Write the data at the calculated offset
            accessor.Write(offset, ref data);
        }

        _deviceUpdatedHandle = EventWaitHandleAcl.Create(true, EventResetMode.AutoReset, "DeviceUpdated", out _, null);
        _accessor = _mmf.CreateViewAccessor();
    }

    public void WriteDictionary(IReadOnlyDictionary<DeviceKeys, SimpleColor> dictionary)
    {
        foreach (var pair in dictionary)
        {
            long offset = (int)pair.Key * _elementSize;
            if (offset < 0)
            {
                continue;
            }

            // Write the data at the calculated offset
            var simpleColor = pair.Value;
            _accessor.Write(offset, ref simpleColor);
        }

        _deviceUpdatedHandle.Set();
    }

    public void Dispose()
    {
        _mmf.Dispose();
        _deviceUpdatedHandle.Dispose();
        _accessor.Dispose();
    }
}