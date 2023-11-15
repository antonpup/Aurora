using System.Diagnostics.Contracts;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Common.Data;

public class MemorySharedStruct<T> : SignaledMemoryObject where T : struct
{
    private static readonly int ElementSize = Marshal.SizeOf(typeof(T));

    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly byte[] _readBuffer = new byte[ElementSize];
    private readonly byte[] _writeBuffer = new byte[ElementSize];

    private readonly GCHandle _writeHandle;
    private readonly GCHandle _readHandle;

    private readonly IntPtr _writePointer;
    private readonly IntPtr _readPointer;

    public MemorySharedStruct(string fileName) : base(fileName)
    {
        try
        {
            _mmf = MemoryMappedFile.OpenExisting(fileName);
        }
        catch (FileNotFoundException)
        {
            _mmf = MemoryMappedFile.CreateOrOpen(fileName, ElementSize);
        }

        _accessor = _mmf.CreateViewAccessor();

        _writeHandle = GCHandle.Alloc(_writeBuffer, GCHandleType.Pinned);
        _writePointer = _writeHandle.AddrOfPinnedObject();

        _readHandle = GCHandle.Alloc(_readBuffer, GCHandleType.Pinned);
        _readPointer = _readHandle.AddrOfPinnedObject();
    }

    public MemorySharedStruct(string fileName, T initialData) : base(fileName)
    {
        // Create a MemoryMappedFile
        _mmf = MemoryMappedFile.CreateOrOpen(fileName, ElementSize);
        // Create a MemoryMappedViewAccessor to write data
        _accessor = _mmf.CreateViewAccessor();

        // Write element size
        _accessor.Write(0, ElementSize);

        _writeHandle = GCHandle.Alloc(_writeBuffer, GCHandleType.Pinned);
        _writePointer = _writeHandle.AddrOfPinnedObject();

        _readHandle = GCHandle.Alloc(_readBuffer, GCHandleType.Pinned);
        _readPointer = _readHandle.AddrOfPinnedObject();

        // Initialize and write data to the MemoryMappedFile
        WriteObject(initialData);
    }

    [Pure]
    public T ReadElement()
    {
        // Read the data back
        _accessor.ReadArray(0, _readBuffer, 0, _readBuffer.Length);

        // Marshal the byte array back to a struct
        return (T)Marshal.PtrToStructure(_readPointer, typeof(T));

        // Read the data at the calculated offset
        _accessor.Read(0, out T result);

        return result;
    }

    public void WriteObject(T element)
    {
        // Marshal the struct to a byte array
        Marshal.StructureToPtr(element, _writePointer, true);

        _accessor.WriteArray(0, _writeBuffer, 0, _writeBuffer.Length);

        SignalUpdated();
    }

    public override void Dispose()
    {
        base.Dispose();

        _mmf.Dispose();
        _accessor.Dispose();
        _writeHandle.Free();
        _readHandle.Free();
    }
}