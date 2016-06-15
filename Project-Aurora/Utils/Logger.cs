using System;
using System.IO;
using System.Text;

namespace Aurora
{
    public enum Logging_Level
    {
        None,
        Info,
        Warning,
        Error
    }

    public class Logger
    {
        private bool retrieved_unique_logfile = false;
        private string logfile = "log.txt";
        private string logdir = "logs/";

        public Logger()
        {
            // Display System information
            StringBuilder systeminfo_sb = new StringBuilder(string.Empty);
            systeminfo_sb.Append("========================================\r\n");

            try
            {
                var win_reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string productName = (string)win_reg.GetValue("ProductName");

                systeminfo_sb.AppendFormat("Operation System: {0}\r\n", productName);
            }
            catch(Exception exc)
            {
                systeminfo_sb.AppendFormat("Operation System: Could not retrieve. [Exception: {0}]\r\n", exc.Message);
            }

            systeminfo_sb.AppendFormat("System Architecture: " + (Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit") + "\r\n");

            systeminfo_sb.AppendFormat("Environment OS Version: {0}\r\n", Environment.OSVersion);


            systeminfo_sb.AppendFormat("System Directory: {0}\r\n", Environment.SystemDirectory);
            systeminfo_sb.AppendFormat("Processor Count: {0}\r\n", Environment.ProcessorCount);
            systeminfo_sb.AppendFormat("User DomainName: {0}\r\n", Environment.UserDomainName);
            systeminfo_sb.AppendFormat("User Name: {0}\r\n", Environment.UserName);

            systeminfo_sb.AppendFormat("SystemPageSize: {0}\r\n", Environment.SystemPageSize);
            systeminfo_sb.AppendFormat("Version: {0}\r\n", Environment.Version);
            systeminfo_sb.Append("========================================\r\n");

            LogLine(systeminfo_sb.ToString(), Logging_Level.None, false);
        }

        public string GetPath()
        {
            if (!retrieved_unique_logfile)
                logfile = System.IO.Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), logdir, System.DateTime.Now.ToString("yyyy_dd_MM") + ".log");

            if (!System.IO.File.Exists(logdir))
                System.IO.Directory.CreateDirectory(logdir);

            return logfile;
        }

        public void LogLine(string message, Logging_Level level = Logging_Level.None, bool timestamp = true)
        {
            try
            {
                System.IO.StreamWriter sw = System.IO.File.AppendText(GetPath());
                try
                {
                    string logLine = (level != Logging_Level.None ? "[" + LevelToString(level) + "] " : "") + (timestamp ? System.String.Format("{0:G}: ", System.DateTime.Now) : "" ) + System.String.Format("{0}", message);
                    System.Diagnostics.Debug.WriteLine(logLine);
                    sw.WriteLine(logLine);
                }
                finally
                {
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("There was an exception during logging, " + e.Message);
            }
        }

        private string LevelToString(Logging_Level level)
        {
            switch (level)
            {
                case (Logging_Level.Info):
                    return "INFO";
                case (Logging_Level.Warning):
                    return "WARNING";
                case (Logging_Level.Error):
                    return "ERROR";
                default:
                    return "";
            }
        }
    }
}
