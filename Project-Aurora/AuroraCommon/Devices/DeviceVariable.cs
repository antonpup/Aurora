using System.Runtime.InteropServices;

namespace Common.Devices;

public enum VariableType
{
    None = 0,
    Boolean = 1,
    Float = 2,
    Double = 3,
    Int = 4,
    Long = 5,
    String = 6,
    Color = 7,
    DeviceKeys = 8,
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct DeviceVariable(string StringValue)
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public readonly string DeviceName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public readonly string Name = "";

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly byte[]? Value = new byte[8];

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly byte[]? Default = new byte[8];

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly byte[]? Max = new byte[8];

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public readonly byte[]? Min = new byte[8];

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public readonly string Title = "";

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
    public readonly string Remark = "";

    public readonly int Flags = 0;
    public readonly VariableType ValueType = 0;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
    public readonly string StringValue = StringValue;

    public DeviceVariable(
        string deviceName, string name,
        byte[]? value, byte[]? dDefault, byte[]? max, byte[]? min, 
        string title, string remark, int flags,
        VariableType valueType, string stringValue = "") : this(stringValue)
    {
        DeviceName = deviceName;
        Name = name;
        Value = value;
        Default = dDefault;
        Max = max;
        Min = min;
        Title = title;
        Remark = remark;
        Flags = flags;
        ValueType = valueType;
    }
}