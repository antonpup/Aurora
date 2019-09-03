using System.Runtime.InteropServices;


namespace DrevoRadi
{
    public class DrevoRadiSDK
    {
        [DllImport("DrevoRadi", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DrevoRadiInit();

        [DllImport("DrevoRadi", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DrevoRadiSetRGB(byte[] bitmap, int length);

        [DllImport("DrevoRadi", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DrevoRadiShutdown();

        [DllImport("DrevoRadi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ToDrevoBitmap(int key);
    }
}