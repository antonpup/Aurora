using System;
using System.Runtime.InteropServices;

namespace MirrSharp.Driver
{

	public enum OperationType
	{
		dmf_dfo_IGNORE = 0,
		dmf_dfo_FROM_SCREEN = 1,
		dmf_dfo_FROM_DIB = 2,
		dmf_dfo_TO_SCREEN = 3,

		dmf_dfo_SCREEN_SCREEN = 11,
		dmf_dfo_BLIT = 12,
		dmf_dfo_SOLIDFILL = 13,
		dmf_dfo_BLEND = 14,
		dmf_dfo_TRANS = 15,
		dmf_dfo_PLG = 17,
		dmf_dfo_TEXTOUT = 18,

		dmf_dfo_Ptr_Engage = 48,	// point is used with this record
		dmf_dfo_Ptr_Avert = 49,

		// 1.0.9.0
		// mode-assert notifications to manifest PDEV limbo status
		dmf_dfn_assert_on = 64,	// DrvAssert(TRUE): PDEV reenabled
		dmf_dfn_assert_off = 65,	// DrvAssert(FALSE): PDEV disabled
	}

    [StructLayout(LayoutKind.Sequential)]
    public struct ChangesBuffer
    {
        public uint counter;

    	public const int MAXCHANGES_BUF = 20000;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXCHANGES_BUF)]
        public ChangesRecord[] pointrect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ChangesRecord
    {
        public uint type;
        public Rectangle rect;
        public Rectangle origrect;
        public Point point;
        public uint color;
        public uint refcolor;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DisplayDevice
    {
        public int CallBack;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public int StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceMode
    {

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmSpecVersion;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmDriverVersion;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmSize;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmDriverExtra;
		[MarshalAs(UnmanagedType.U4)] // DWORD
        public int dmFields;

		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmOrientation;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmPaperSize;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmPaperLength;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmPaperWidth;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmScale;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmCopies;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmDefaultSource;
		[MarshalAs(UnmanagedType.U2)] // WORD
        public short dmPrintQuality;


		[MarshalAs(UnmanagedType.I2)] // short
		public short dmColor;
		[MarshalAs(UnmanagedType.I2)] // short
		public short dmDuplex;
		[MarshalAs(UnmanagedType.I2)] // short
		public short dmYResolution;
		[MarshalAs(UnmanagedType.I2)] // short
		public short dmTTOption;
		[MarshalAs(UnmanagedType.I2)] // short
		public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
		[MarshalAs(UnmanagedType.U2)] // WORD
		public short dmLogPixels;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmBitsPerPel;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmPelsWidth;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmPelsHeight;

		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmDisplayFlags;

		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmDisplayFrequency;

		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmICMMethod;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmICMIntent;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmMediaType;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmDitherType;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmReserved1;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmReserved2;

		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmPanningWidth;
		[MarshalAs(UnmanagedType.U4)] // DWORD
		public int dmPanningHeight;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct GetChangesBuffer
    {
		/// <summary>
		/// Pointer to the <see cref="ChangesBuffer"/> structure. To be casted to the neccesary type a bit later...
		/// </summary>
        public IntPtr Buffer;
        public IntPtr UserBuffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {

        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;
    }
}
