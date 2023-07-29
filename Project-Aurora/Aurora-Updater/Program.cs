using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using System.Linq;
using System.Threading;
using Version = SemanticVersioning.Version;

namespace Aurora_Updater;

/// <summary>
///     The static storage.
/// </summary>
public static class StaticStorage
{
    #region Static Fields

    /// <summary>
    ///     The index.
    /// </summary>
    public static UpdateManager Manager;

    #endregion
}

internal static class Program
{
    private static string _passedArgs = "";
    private static bool _isSilent;
    public static string ExePath = "";
    private static UpdateType _installType = UpdateType.Undefined;
    private static bool _isElevated;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        using Mutex mutex = new(false, "Aurora-Updater");
        try
        {
            if (!mutex.WaitOne(TimeSpan.FromMilliseconds(0), true))
            {
                //Updater is already up
                return;
            }
        }
        catch(AbandonedMutexException) { /* Means previous instance closed anyway */ }

        foreach (var arg in args)
        {
            if (string.IsNullOrWhiteSpace(arg))
                continue;

            switch (arg)
            {
                case "-silent":
                    _isSilent = true;
                    break;
                case "-update_major":
                    _installType = UpdateType.Major;
                    break;
                case "-update_minor":
                    _installType = UpdateType.Minor;
                    break;
            }

            _passedArgs += arg + " ";
        }

        _passedArgs = _passedArgs.TrimEnd(' ');

        ExePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        //Check privilege
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        _isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
        var maj = "";

        string auroraPath;
        if (File.Exists(auroraPath = Path.Combine(ExePath, "Aurora.exe")))
            maj = FileVersionInfo.GetVersionInfo(auroraPath).FileVersion;

        if (string.IsNullOrWhiteSpace(maj))
        {
            if (!_isSilent)
                MessageBox.Show(
                    "Application launched incorrectly, no version was specified.\r\n" +
                    "Please use Aurora if you want to check for updates.\r\n" +
                    "Options -> \"Updates\" \"Check for Updates\"",
                    "Aurora Updater",
                    MessageBoxButtons.OK);
            return;
        }
        var versionMajor = new Version(maj.TrimStart('v') + ".0.0", true);

        var owner = FileVersionInfo.GetVersionInfo(auroraPath).CompanyName;
        var repository = FileVersionInfo.GetVersionInfo(auroraPath).ProductName;
        if (owner == null || repository == null)
        {
            MessageBox.Show(
                "Exe owner/repository is not set. This is needed for checking releases on Github",
                "Aurora Updater - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        //Initialize UpdateManager
        try
        {
            StaticStorage.Manager = new UpdateManager(versionMajor, owner, repository);
        }
        catch (Exception exc)
        {
            MessageBox.Show(
                $"Could not find update.\r\nError:\r\n{exc}",
                "Aurora Updater - Error");
            return;
        }

        if (_installType != UpdateType.Undefined)
        {
            if (_isElevated)
            {
                var updateForm = new MainForm();
                updateForm.ShowDialog();
            }
            else
            {
                MessageBox.Show(
                    "Updater was not granted Admin rights.",
                    "Aurora Updater - Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        else
        {
            var latestV = new Version(StaticStorage.Manager.LatestRelease.TagName.TrimStart('v') + ".0.0", true);
            if (File.Exists("skipversion.txt"))
            {
                var skippedVersion = Version.Parse(File.ReadAllText("skipversion.txt"), true);
                if (skippedVersion >= latestV)
                {
                    return;
                }
            }

            if (latestV <= versionMajor)
            {
                if (!_isSilent)
                    MessageBox.Show(
                        "You have latest version of Aurora installed.",
                        "Aurora Updater",
                        MessageBoxButtons.OK);
            }
            else
            {
                var userResult = new UpdateInfoForm
                {
                    Changelog = StaticStorage.Manager.LatestRelease.Body,
                    UpdateDescription = StaticStorage.Manager.LatestRelease.Name,
                    UpdateVersion = latestV.ToString(),
                    CurrentVersion = versionMajor.ToString(),
                    UpdateSize = StaticStorage.Manager.LatestRelease.Assets
                        .First(s => s.Name.StartsWith("release") || s.Name.StartsWith("Aurora-v")).Size,
                    PreRelease = StaticStorage.Manager.LatestRelease.Prerelease
                };

                userResult.ShowDialog();

                if (userResult.DialogResult != DialogResult.OK) return;
                if (_isElevated)
                {
                    var updateForm = new MainForm();
                    updateForm.ShowDialog();
                }
                else
                {
                    //Request user to grant admin rights
                    try
                    {
                        var updaterProc = new ProcessStartInfo();
                        updaterProc.FileName = Process.GetCurrentProcess().MainModule.FileName;
                        updaterProc.Arguments = _passedArgs + " -update_major";
                        updaterProc.Verb = "runas";
                        Process.Start(updaterProc);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(
                            $"Could not start Aurora Updater. Error:\r\n{exc}",
                            "Aurora Updater - Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}