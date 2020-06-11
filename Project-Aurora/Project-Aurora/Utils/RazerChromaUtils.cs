using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using RazerSdkWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Aurora.Utils
{
    public enum RazerChromaInstallerExitCode
    {
        Success = 0,
        InvalidState = 1,
        RestartRequired = 3010
    }

    public static class RazerChromaUtils
    {
        public static Task<int> UninstallAsync() => Task.Run(() =>
        {
            if (RzHelper.GetSdkVersion() == new RzSdkVersion())
                return (int)RazerChromaInstallerExitCode.InvalidState;

            int DoUninstall(string filepath)
            {
                var filename = Path.GetFileName(filepath);
                var path = Path.GetDirectoryName(filepath);
                var processInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    WorkingDirectory = path,
                    Arguments = $"/S _?={path}",
                    ErrorDialog = true
                };

                var process = Process.Start(processInfo);
                process.WaitForExit(120000);
                return process.ExitCode;
            }

            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                var key = hklm.OpenSubKey(@"Software\Razer\Synapse3\PID0302MW");
                if (key != null)
                {
                    var filepath = (string)key.GetValue("UninstallPath", null);

                    var exitcode = DoUninstall(filepath);
                    if (exitcode == (int)RazerChromaInstallerExitCode.RestartRequired)
                        return exitcode;
                }

                key = hklm.OpenSubKey(@"Software\Razer\Synapse3\RazerChromaBroadcaster");
                if (key != null)
                {
                    var filepath = (string)key.GetValue("UninstallerPath", null);

                    var exitcode = DoUninstall(filepath);
                    if (exitcode == (int)RazerChromaInstallerExitCode.RestartRequired)
                        return exitcode;
                }

                key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
                if (key != null)
                {
                    var path = (string)key.GetValue("UninstallPath", null);
                    var filename = (string)key.GetValue("UninstallFilename", null);

                    var exitcode = DoUninstall($@"{path}\{filename}");
                    if (exitcode == (int)RazerChromaInstallerExitCode.RestartRequired)
                        return exitcode;
                }

                return (int)RazerChromaInstallerExitCode.Success;
            }
        });

        public static Task<string> GetDownloadUrlAsync() => Task.Run(() =>
        {
            using (var client = new WebClient())
            {
                var endpoint = "prod";
                var json = JObject.Parse(client.DownloadString("https://discovery.razerapi.com/user/endpoints"));
                var hash = json["endpoints"].Children().FirstOrDefault(c => c.Value<string>("name") == endpoint)?.Value<string>("hash");

                if (hash == null)
                    return null;

                var platformData = @"
<PlatformRoot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Platform>
    <Arch>64</Arch>
    <Locale>en</Locale>
    <Mfr>Generic-MFR</Mfr>
    <Model>Generic-MDL</Model>
    <OS>Windows</OS>
    <OSVer>10</OSVer>
    <SKU>Generic-SKU</SKU>
  </Platform>
</PlatformRoot>
";

                client.Headers.Set("Content-Type", "application/xml");
                var xml = client.UploadString($"https://manifest.razerapi.com/api/legacy/{hash}/{endpoint}/productlist/get", platformData);
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                foreach(XmlNode node in doc.DocumentElement.SelectNodes("//Module"))
                    if (node["Name"].InnerText == "CHROMABROADCASTER")
                        return node["DownloadURL"].InnerText;

                return null;
            }
        });

        public static Task<string> DownloadAsync() => Task.Run(async () =>
        {
            var url = await GetDownloadUrlAsync();
            if (url == null)
                return null;

            using (var client = new WebClient())
            {
                var path = Path.ChangeExtension(Path.GetTempFileName(), ".exe");
                client.DownloadFile(url, path);
                return path;
            }
        });

        public static Task<int> InstallAsync(string installerPath) => Task.Run(() =>
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Path.GetFileName(installerPath),
                WorkingDirectory = Path.GetDirectoryName(installerPath),
                Arguments = "/S",
                ErrorDialog = true
            };

            var process = Process.Start(processInfo);
            process.WaitForExit(120000);
            return process.ExitCode;
        });
    }
}
