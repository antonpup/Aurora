using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;
using System.Linq;
using Version = SemVer.Version;

namespace Aurora_Updater
{
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

    static class Program
    {
        private static string passedArgs = "";
        private static bool isSilent = false;
        private static bool isSilentMinor = false;
        private static Version versionMajor;
        private static Version versionMinor;
        public static string exePath = "";
        private static UpdateType installType = UpdateType.Undefined;
        public static bool isElevated = false;

        private static MainForm updateForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (String.IsNullOrWhiteSpace(arg))
                    continue;

                if (arg.Equals("-silent"))
                    isSilent = true;
                else if (arg.Equals("-silent_minor"))
                    isSilentMinor = true;
                else if (arg.Equals("-update_major"))
                    installType = UpdateType.Major;
                else if (arg.Equals("-update_minor"))
                    installType = UpdateType.Minor;

                passedArgs += arg + " ";
            }

            passedArgs.TrimEnd(' ');

            exePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            //Check privilege
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            string _maj = "";

            string auroraPath;
            if (File.Exists(auroraPath = Path.Combine(exePath, "Aurora.exe")))
                _maj = FileVersionInfo.GetVersionInfo(auroraPath).FileVersion;

            if (String.IsNullOrWhiteSpace(_maj))
            {
                if (!isSilent)
                    MessageBox.Show(
                        "Application launched incorrectly, no version was specified.\r\nPlease use Aurora if you want to check for updates.\r\nOptions -> \"Updates\" \"Check for Updates\"",
                        "Aurora Updater",
                        MessageBoxButtons.OK);
                return;
            }
            versionMajor = new Version(_maj, true);

            //Initialize UpdateManager
            StaticStorage.Manager = new UpdateManager(versionMajor);

            //Check if update retrieval was successful.
            if (StaticStorage.Manager.updateState == UpdateStatus.Error)
                return;

            if (installType != UpdateType.Undefined)
            {
                if (isElevated)
                {
                    updateForm = new MainForm(installType);
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
                Version latestV = new Version(StaticStorage.Manager.LatestRelease.TagName.TrimStart('v'), true);

                if (latestV > versionMajor)
                {
                    UpdateInfoForm userResult = new UpdateInfoForm()
                    {
                        changelog = StaticStorage.Manager.LatestRelease.Body,
                        updateDescription = StaticStorage.Manager.LatestRelease.Name,
                        updateVersion = latestV.ToString(),
                        currentVersion = versionMajor.ToString(),
                        updateSize = StaticStorage.Manager.LatestRelease.Assets.First(s => s.Name.StartsWith("release") || s.Name.StartsWith("Aurora-v")).Size,
                        preRelease = StaticStorage.Manager.LatestRelease.Prerelease
                    };

                    userResult.ShowDialog();

                    if (userResult.DialogResult == DialogResult.OK)
                    {
                        if (isElevated)
                        {
                            updateForm = new MainForm(UpdateType.Major);
                            updateForm.ShowDialog();
                        }
                        else
                        {
                            //Request user to grant admin rights
                            try
                            {
                                ProcessStartInfo updaterProc = new ProcessStartInfo();
                                updaterProc.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                                updaterProc.Arguments = passedArgs + " -update_major";
                                updaterProc.Verb = "runas";
                                Process.Start(updaterProc);

                                return; //Exit, no further action required
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
                else
                {
                    if (!isSilent)
                        MessageBox.Show(
                            "You have latest version of Aurora installed.",
                            "Aurora Updater",
                            MessageBoxButtons.OK);
                }

                /*string _min = "";

                if (File.Exists(Path.Combine(exePath, "ver_minor.txt")))
                    _min = File.ReadAllText(Path.Combine(exePath, "ver_minor.txt"));

                if (!String.IsNullOrWhiteSpace(_min))
                {
                    versionMinor = new UpdateVersion(_min);

                    if (!(StaticStorage.Manager.response.Minor.Version <= versionMinor))
                    {
                        if (isSilentMinor)
                            StaticStorage.Manager.RetrieveUpdate(UpdateType.Minor);
                        else
                        {
                            UpdateInfoForm userResult = new UpdateInfoForm()
                            {
                                changelog = StaticStorage.Manager.response.Minor.Changelog,
                                updateDescription = StaticStorage.Manager.response.Minor.Description,
                                updateVersion = StaticStorage.Manager.response.Minor.Version.ToString(),
                                currentVersion = versionMinor.ToString(),
                                updateSize = StaticStorage.Manager.response.Minor.FileSize,
                                preRelease = StaticStorage.Manager.response.Minor.PreRelease
                            };

                            userResult.ShowDialog();

                            if (userResult.DialogResult == DialogResult.Yes)
                            {
                                if (isElevated)
                                {
                                    updateForm = new MainForm(UpdateType.Minor);
                                    updateForm.ShowDialog();
                                }
                                else
                                {
                                    //Request user to grant admin rights
                                    try
                                    {
                                        ProcessStartInfo updaterProc = new ProcessStartInfo();
                                        updaterProc.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                                        updaterProc.Arguments = passedArgs + " -update_minor";
                                        updaterProc.Verb = "runas";
                                        Process.Start(updaterProc);

                                        return; //Exit, no further action required
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
                    else
                    {
                        if (!isSilent && !isSilentMinor)
                            MessageBox.Show(
                                "You have latest Minor version of Aurora installed.",
                                "Aurora Updater",
                                MessageBoxButtons.OK);
                    }
                }
                else
                {
                    if (!isSilent)
                        MessageBox.Show(
                            "Application launched incorrectly, no version was specified.\r\nPlease use Aurora if you want to check for updates.\r\nOptions -> \"Updates\" \"Check for Updates\"",
                            "Aurora Updater",
                            MessageBoxButtons.OK);
                }*/
            }
        }
    }
}
