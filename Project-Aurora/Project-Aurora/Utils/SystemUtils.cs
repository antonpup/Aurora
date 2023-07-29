using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;

namespace Aurora.Utils;

public static class SystemUtils
{
    public static string GetSystemInfo()
    {
        var systemInfoSb = new StringBuilder(string.Empty);
        systemInfoSb.Append("\r\n========================================\r\n");

        try
        {
            using var winReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var productName = winReg?.GetValue("ProductName");

            systemInfoSb.Append($"Operation System: {productName}\r\n");
        }
        catch (Exception exc)
        {
            systemInfoSb.Append($"Operation System: Could not be retrieved. [Exception: {exc.Message}]\r\n");
        }

        systemInfoSb.Append($"Environment OS Version: {Environment.OSVersion}\r\n");

        systemInfoSb.Append($"System Directory: {Environment.SystemDirectory}\r\n");
        systemInfoSb.Append($"Executing Directory: {Global.ExecutingDirectory}\r\n");
        systemInfoSb.Append($"Launch Directory: {Directory.GetCurrentDirectory()}\r\n");
        systemInfoSb.Append($"Processor Count: {Environment.ProcessorCount}\r\n");

        systemInfoSb.Append($"SystemPageSize: {Environment.SystemPageSize}\r\n");
        systemInfoSb.Append($"Environment Version: {Environment.Version}\r\n");

        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        systemInfoSb.Append($"Is Elevated: {principal.IsInRole(WindowsBuiltInRole.Administrator)}\r\n");
        systemInfoSb.Append($"Aurora Assembly Version: {Assembly.GetExecutingAssembly().GetName().Version}\r\n");
        systemInfoSb.Append(
            $"Aurora File Version: {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}\r\n");

        systemInfoSb.Append("========================================\r\n");

        return systemInfoSb.ToString();
    }
}