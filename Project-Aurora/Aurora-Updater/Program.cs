using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

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
        private static UpdateVersion versionMajor;
        private static UpdateVersion versionMinor;
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

            exePath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            //Check privilege
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            //Initialize UpdateManager
            StaticStorage.Manager = new UpdateManager();

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
                string _maj = "";

                if (File.Exists(Path.Combine(exePath, "ver_major.txt")))
                    _maj = File.ReadAllText(Path.Combine(exePath, "ver_major.txt"));

                if (!String.IsNullOrWhiteSpace(_maj))
                {
                    versionMajor = new UpdateVersion(_maj);

                    if (!(StaticStorage.Manager.responce.Major.Version <= versionMajor))
                    {
                        UpdateInfoForm userResult = new UpdateInfoForm()
                        {
                            changelog = StaticStorage.Manager.responce.Major.Changelog,
                            updateDescription = StaticStorage.Manager.responce.Major.Description,
                            updateVersion = StaticStorage.Manager.responce.Major.Version.ToString(),
                            currentVersion = versionMajor.ToString(),
                            updateSize = StaticStorage.Manager.responce.Major.FileSize
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
                }
                else
                {
                    if (!isSilent)
                        MessageBox.Show(
                            "Application launched incorrectly, no version was specified.\r\nPlease use Aurora if you want to check for updates.\r\nOptions -> \"Updates\" \"Check for Updates\"",
                            "Aurora Updater",
                            MessageBoxButtons.OK);
                }

                string _min = "";

                if (File.Exists(Path.Combine(exePath, "ver_minor.txt")))
                    _min = File.ReadAllText(Path.Combine(exePath, "ver_minor.txt"));

                if (!String.IsNullOrWhiteSpace(_min))
                {
                    versionMinor = new UpdateVersion(_min);

                    if (!(StaticStorage.Manager.responce.Minor.Version <= versionMinor))
                    {
                        if (isSilentMinor)
                            StaticStorage.Manager.RetrieveUpdate(UpdateType.Minor);
                        else
                        {
                            UpdateInfoForm userResult = new UpdateInfoForm()
                            {
                                changelog = StaticStorage.Manager.responce.Minor.Changelog,
                                updateDescription = StaticStorage.Manager.responce.Minor.Description,
                                updateVersion = StaticStorage.Manager.responce.Minor.Version.ToString(),
                                currentVersion = versionMinor.ToString(),
                                updateSize = StaticStorage.Manager.responce.Minor.FileSize
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
                }
            }
        }
    }
}
