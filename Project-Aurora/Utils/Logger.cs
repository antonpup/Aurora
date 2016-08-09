using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aurora
{
    /// <summary>
    /// Enum list for every logging levels
    /// </summary>
    public enum Logging_Level
    {
        /// <summary>
        /// None, no logging level
        /// </summary>
        None,

        /// <summary>
        /// Information
        /// </summary>
        Info,

        /// <summary>
        /// Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Error
        /// </summary>
        Error,

        /// <summary>
        /// External, should be used for scripts
        /// </summary>
        External
    }

    /// <summary>
    /// A logging class
    /// </summary>
    public class Logger
    {
        private bool retrieved_unique_logfile = false;
        private string logfile = "log.txt";
        private string logdir = "logs/";
        private Queue<string> message_queue = new Queue<string>();

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

        /// <summary>
        /// Gets the path of the currently used log file
        /// </summary>
        /// <returns>A path to the log file</returns>
        public string GetPath()
        {
            if (!retrieved_unique_logfile)
                logfile = System.IO.Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), logdir, System.DateTime.Now.ToString("yyyy_dd_MM") + ".log");

            if (!System.IO.File.Exists(logdir))
                System.IO.Directory.CreateDirectory(logdir);

            return logfile;
        }

        /// <summary>
        /// Logs a line into the currently used log file
        /// </summary>
        /// <param name="message">Message you wish to log</param>
        /// <param name="level">The logging level of this message</param>
        /// <param name="timestamp">A boolean value representing if a timestamp should be included</param>
        public void LogLine(string message, Logging_Level level = Logging_Level.None, bool timestamp = true)
        {
            string logLine = PrepareMessage(message, level, timestamp);

            try
            {
                System.IO.StreamWriter sw = System.IO.File.AppendText(GetPath());
                try
                {
                    while(message_queue.Count > 0)
                    {
                        string queue_msg = message_queue.Dequeue();

                        System.Diagnostics.Debug.WriteLine(queue_msg);
                        sw.WriteLine(queue_msg);
                    }

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

                message_queue.Enqueue(logLine);
            }
        }

        private string PrepareMessage(string message, Logging_Level level, bool timestamp)
        {
            return (level != Logging_Level.None ? "[" + LevelToString(level) + "] " : "") + (timestamp ? System.String.Format("{0:G}: ", System.DateTime.Now) : "") + System.String.Format("{0}", message);
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
                case (Logging_Level.External):
                    return "EXTERNAL";
                default:
                    return "";
            }
        }
    }
}
