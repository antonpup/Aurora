﻿using System;
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
        External,

        /// <summary>
        /// Debug, will only show when Application is started in debug mode
        /// </summary>
        Debug
    }

    /// <summary>
    /// A logging class
    /// </summary>
    public class Logger
    {
        private bool HasUniqueLogFile = false;
        private bool HasUniqueLogDirectory = false;
        private string LogFile = "log.txt";
        private string LogDirectory = "Aurora/Logs/";
        private Queue<string> MessageQueue = new Queue<string>();
        private readonly int QueueLimit = 255;

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
            catch (Exception exc)
            {
                systeminfo_sb.AppendFormat("Operation System: Could not be retrieved. [Exception: {0}]\r\n", exc.Message);
            }

            systeminfo_sb.AppendFormat("System Architecture: " + (Environment.Is64BitOperatingSystem ? "64 bit" : "32 bit") + "\r\n");

            systeminfo_sb.AppendFormat("Environment OS Version: {0}\r\n", Environment.OSVersion);

            systeminfo_sb.AppendFormat("System Directory: {0}\r\n", Environment.SystemDirectory);
            systeminfo_sb.AppendFormat("Executing Directory: {0}\r\n", Global.ExecutingDirectory);
            systeminfo_sb.AppendFormat("Launch Directory: {0}\r\n", Directory.GetCurrentDirectory());
            systeminfo_sb.AppendFormat("Processor Count: {0}\r\n", Environment.ProcessorCount);
            //systeminfo_sb.AppendFormat("User DomainName: {0}\r\n", Environment.UserDomainName);
            systeminfo_sb.AppendFormat("User Name: {0}\r\n", Environment.UserName);

            systeminfo_sb.AppendFormat("SystemPageSize: {0}\r\n", Environment.SystemPageSize);
            systeminfo_sb.AppendFormat("Environment Version: {0}\r\n", Environment.Version);

            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            systeminfo_sb.AppendFormat("Is Elevated: {0}\r\n", principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator));
            systeminfo_sb.AppendFormat("Aurora Assembly Version: {0}\r\n", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            systeminfo_sb.AppendFormat("Aurora File Version: {0}\r\n", System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion);

            systeminfo_sb.Append("========================================\r\n");

            LogLine(systeminfo_sb.ToString(), Logging_Level.None, false);
        }

        /// <summary>
        /// Gets the path of the currently used log file
        /// </summary>
        /// <returns>A path to the log file</returns>
        public string GetPath()
        {
            if (!HasUniqueLogFile)
                LogFile = System.IO.Path.Combine(GetLogsDirectory(), System.DateTime.Now.ToString("yyyy_dd_MM") + ".log");

            if (!System.IO.File.Exists(LogDirectory))
                System.IO.Directory.CreateDirectory(LogDirectory);

            return LogFile;
        }

        /// <summary>
        /// Gets the path to the Logs directory
        /// </summary>
        /// <returns>The path to the Logs directory</returns>
        public string GetLogsDirectory()
        {
            if (!HasUniqueLogDirectory)
                LogDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), LogDirectory);

            if (!System.IO.File.Exists(LogDirectory))
                System.IO.Directory.CreateDirectory(LogDirectory);

            return LogDirectory;
        }

        /// <summary>
        /// Logs a line into the currently used log file
        /// </summary>
        /// <param name="message">Message you wish to log</param>
        /// <param name="level">The logging level of this message</param>
        /// <param name="timestamp">A boolean value representing if a timestamp should be included</param>
        public void LogLine(string message, Logging_Level level = Logging_Level.None, bool timestamp = true)
        {
            if (level == Logging_Level.Debug && !Global.isDebug)
                return;

            string logLine = PrepareMessage(message, level, timestamp);

            try
            {
                System.IO.StreamWriter sw = System.IO.File.AppendText(GetPath());
                try
                {
                    while (MessageQueue.Count > 0)
                    {
                        string queue_msg = MessageQueue.Dequeue();

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

                if (MessageQueue.Count < QueueLimit)
                    MessageQueue.Enqueue(logLine);
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
                case (Logging_Level.Debug):
                    return "DEBUG";
                default:
                    return "";
            }
        }
    }
}
