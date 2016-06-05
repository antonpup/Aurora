using System;
using System.IO;
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
        private static bool isSilent = false;
        private static bool isSilentMinor = false;
        private static string version_major = "";
        private static string version_minor = "";
        public static string exe_path = "";


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
                {
                    isSilent = true;
                }
                else if (arg.Equals("-silent_minor"))
                {
                    isSilentMinor = true;
                }
                else
                {
                }
            }

            exe_path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            StaticStorage.manager = new UpdateManager();

            if (File.Exists(Path.Combine(exe_path, "ver_major.txt")))
                version_major = File.ReadAllText(Path.Combine(exe_path, "ver_major.txt"));

            if (!String.IsNullOrWhiteSpace(version_major))
            {
                if(!StaticStorage.manager.responce.Major.Version.Equals(version_major))
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
                        MainForm majform = new MainForm(UpdateType.Major);
                        majform.ShowDialog();
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
                    if(isSilentMinor)
                    {
                        StaticStorage.manager.retrieveUpdate(UpdateType.Minor);
                    }
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
                            MainForm minform = new MainForm(UpdateType.Minor);
                            minform.ShowDialog();
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
