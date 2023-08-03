using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Aurora.Utils;

/// <summary>
/// http://peterkellyonline.blogspot.com/2011/04/configuring-windows-service.html
/// </summary>
public static class ServiceHelper
{
    [DllImport(Advapi32Dll, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool ChangeServiceConfig(
        IntPtr hService,
        uint nServiceType,
        uint nStartType,
        uint nErrorControl,
        string? lpBinaryPathName,
        string? lpLoadOrderGroup,
        IntPtr lpdwTagId,
        [In] char[]? lpDependencies,
        string? lpServiceStartName,
        string? lpPassword,
        string? lpDisplayName);

    [DllImport(Advapi32Dll, SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr OpenService(IntPtr hScManager, string lpServiceName, uint dwDesiredAccess);

    [DllImport(Advapi32Dll, EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern IntPtr OpenSCManager(
        string machineName, string databaseName, uint dwAccess);

    [DllImport(Advapi32Dll, EntryPoint = "CloseServiceHandle")]
    private static extern int CloseServiceHandle(IntPtr hScObject);

    private const uint ServiceNoChange = 0xFFFFFFFF;
    private const uint ServiceQueryConfig = 0x00000001;
    private const uint ServiceChangeConfig = 0x00000002;
    private const uint ScManagerAllAccess = 0x000F003F;
    private const string Advapi32Dll = "advapi32.dll";

    public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
    {
        var scManagerHandle = OpenSCManager(null, null, ScManagerAllAccess);
        if (scManagerHandle == IntPtr.Zero)
        {
            throw new ExternalException("Open Service Manager Error");
        }

        var serviceHandle = OpenService(
            scManagerHandle,
            svc.ServiceName,
            ServiceQueryConfig | ServiceChangeConfig);

        if (serviceHandle == IntPtr.Zero)
        {
            throw new ExternalException("Open Service Error");
        }

        var result = ChangeServiceConfig(
            serviceHandle,
            ServiceNoChange,
            (uint)mode,
            ServiceNoChange,
            null,
            null,
            IntPtr.Zero,
            null,
            null,
            null,
            null);

        if (result == false)
        {
            var nError = Marshal.GetLastWin32Error();
            var win32Exception = new Win32Exception(nError);
            throw new ExternalException("Could not change service start type: "
                                        + win32Exception.Message);
        }

        CloseServiceHandle(serviceHandle);
        CloseServiceHandle(scManagerHandle);
    }
}