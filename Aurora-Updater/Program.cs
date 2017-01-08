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
        public static UpdateManager manager;

        #endregion
    }

    static class Program
    {
        private static string passedArgs = "";
        private static bool isSilent = false;
        private static bool isSilentMinor = false;
        private static string version_major = "";
        private static string version_minor = "";
        public static string exe_path = "";
        private static UpdateType installType = UpdateType.Undefined;
        public static bool isElevated = false;


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

            exe_path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            //Check privilege
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            //Initialize UpdateManager
            StaticStorage.manager = new UpdateManager();

            //Check if update retrieval was successful.
            if (StaticStorage.manager.updatestate == UpdateStatus.Error)
                return;

            if (installType != UpdateType.Undefined)
            {
                if (isElevated)
                {
                    MainForm majform = new MainForm(installType);
                    majform.ShowDialog();
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
                if (File.Exists(Path.Combine(exe_path, "ver_major.txt")))
                    version_major = File.ReadAllText(Path.Combine(exe_path, "ver_major.txt"));

                if (!String.IsNullOrWhiteSpace(version_major))
                {
                    if (!StaticStorage.manager.responce.Major.Version.Equals(version_major))
                    {
                        DialogResult result = MessageBox.Show(
                            "A new version of Aurora is available (" + StaticStorage.manager.responce.Major.Version + ") with following changes:\r\n\r\n" +
                            StaticStorage.manager.responce.Major.Changelog +
                            "\r\n\r\nWould you like to install this update?",
                            "Aurora Updater",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.None,
                            MessageBoxDefaultButton.Button1,
                            (MessageBoxOptions)0x40000);

                        if (result == DialogResult.Yes)
                        {
                            if (isElevated)
                            {
                                MainForm majform = new MainForm(UpdateType.Major);
                                majform.ShowDialog();
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


                if (File.Exists(Path.Combine(exe_path, "ver_minor.txt")))
                    version_minor = File.ReadAllText(Path.Combine(exe_path, "ver_minor.txt"));

                if (!String.IsNullOrWhiteSpace(version_minor))
                {
                    if (!StaticStorage.manager.responce.Minor.Version.Equals(version_minor))
                    {
                        if (isSilentMinor)
                            StaticStorage.manager.retrieveUpdate(UpdateType.Minor);
                        else
                        {
                            DialogResult result = MessageBox.Show(
                                                    "A new minor version of Aurora is available (" + StaticStorage.manager.responce.Minor.Version + ") with following changes:\r\n\r\n" +
                                                    StaticStorage.manager.responce.Minor.Changelog +
                                                    "\r\n\r\nWould you like to install this update?",
                                                    "Aurora Updater",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.None,
                                                    MessageBoxDefaultButton.Button1,
                                                    (MessageBoxOptions)0x40000);

                            if (result == DialogResult.Yes)
                            {
                                if (isElevated)
                                {
                                    MainForm majform = new MainForm(UpdateType.Minor);
                                    majform.ShowDialog();
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
