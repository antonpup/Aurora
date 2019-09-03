using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.IO;
using Ionic.Zip;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using Octokit;
using SemVer;
using Version = SemVer.Version;

namespace Aurora_Updater
{
    public class LogEntry
    {
        private String message;
        private Color color;

        public LogEntry()
        {
            this.message = "";
            this.color = Color.Black;
        }

        public LogEntry(String message)
        {
            this.message = message;
            this.color = Color.Black;
        }

        public LogEntry(String message, Color color)
        {
            this.message = message;
            this.color = color;
        }

        public Color GetColor()
        {
            return color;
        }

        public String GetMessage()
        {
            return message;
        }

        public override string ToString()
        {
            return message.ToString();
        }
    }

    public enum UpdateStatus
    {
        None,
        InProgress,
        Complete,
        Error
    }

    public class UpdateManager
    {
        private string infoUrl = @"http://project-aurora.com/vcheck.php";
        private string[] ignoreFiles = { };
        //public UpdateResponse response = new UpdateResponse();
        private Queue<LogEntry> log = new Queue<LogEntry>();
        private float downloadProgess = 0.0f;
        private float extractProgess = 0.0f;
        public UpdateStatus updateState = UpdateStatus.None;
        private int downloadProgressCheck = 0;
        private int secondsLeft = 15;
        private Aurora.Settings.Configuration Config;
        private GitHubClient gClient = new GitHubClient(new ProductHeaderValue("aurora-updater"));
        public Release LatestRelease;

        public UpdateManager(Version version)
        {
            LoadSettings();
            PerformCleanup();
            FetchData(version);
        }

        public void LoadSettings()
        {
            try
            {
                Config = Aurora.Settings.ConfigManager.Load();
            }
            catch (Exception e)
            {
                Config = new Aurora.Settings.Configuration();
            }
        }

        public void ClearLog()
        {
            log.Clear();
        }

        public LogEntry[] GetLog()
        {
            return log.ToArray();
        }

        public int GetTotalProgress()
        {
            return (int)((downloadProgess + extractProgess) / 2.0f * 100.0f);
        }

        private bool FetchData(Version version)
        {

            


            try
            {
                if (Config.GetDevReleases || !String.IsNullOrWhiteSpace(version.PreRelease))
                    LatestRelease = gClient.Repository.Release.GetAll("antonpup", "Aurora", new ApiOptions { PageCount = 1, PageSize = 1 }).Result[0];
                else
                    LatestRelease = gClient.Repository.Release.GetLatest("antonpup", "Aurora").Result;

                //Console.WriteLine(reply);
            }
            catch (Exception exc)
            {
                updateState = UpdateStatus.Error;
                return false;
            }

            return true;
        }

        public bool RetrieveUpdate(UpdateType type)
        {
            //string url = @"http://project-aurora.com/download.php?id=" + (type == UpdateType.Major ? response.Major.ID : response.Minor.ID);
            //updateState = UpdateStatus.InProgress;
            try
            {
                string url = LatestRelease.Assets.First(s => s.Name.StartsWith("release") || s.Name.StartsWith("Aurora-v")).BrowserDownloadUrl;
                
                if (!String.IsNullOrWhiteSpace(url))
                {
                    this.log.Enqueue(new LogEntry("Starting download... "));

                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

                    // Starts the download
                    client.DownloadFileAsync(new Uri(url), Path.Combine(Program.exePath, "update.zip"));

                }

            }
            catch (Exception exc)
            {
                log.Enqueue(new LogEntry(exc.Message, Color.Red));
                updateState = UpdateStatus.Error;
                return false;
            }
            return false;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes;

            downloadProgressCheck++;

            if (downloadProgressCheck % 10 == 0)
            {
                log.Enqueue(new LogEntry("Download " + Math.Truncate(percentage * 100) + "%"));
                this.downloadProgess = (float)(Math.Truncate(percentage * 100) / 100.0f);
            }
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.log.Enqueue(new LogEntry("Download complete."));
            this.log.Enqueue(new LogEntry());
            this.downloadProgess = 1.0f;

            if (ExtractUpdate())
            {
                System.Timers.Timer shutdownTimer = new System.Timers.Timer(1000);
                shutdownTimer.Elapsed += ShutdownTimerElapsed;
                shutdownTimer.Start();

                updateState = UpdateStatus.Complete;
            }
            else
                updateState = UpdateStatus.Error;
        }

        private void ShutdownTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (secondsLeft > 0)
            {
                this.log.Enqueue(new LogEntry($"Restarting Aurora in {secondsLeft} second{(secondsLeft == 1 ? "" : "s")}..."));
                secondsLeft--;
            }
            else
            {
                //Kill all Aurora instances
                foreach (Process proc in Process.GetProcessesByName("Aurora"))
                    proc.Kill();

                try
                {
                    ProcessStartInfo auroraProc = new ProcessStartInfo();
                    auroraProc.FileName = Path.Combine(Program.exePath, "Aurora.exe");
                    Process.Start(auroraProc);

                    Environment.Exit(0); //Exit, no further action required
                }
                catch (Exception exc)
                {
                    this.log.Enqueue(new LogEntry($"Could not restart Aurora. Error:\r\n{exc}", Color.Red));
                    this.log.Enqueue(new LogEntry("Please restart Aurora manually.", Color.Red));

                    MessageBox.Show(
                        $"Could not restart Aurora.\r\nPlease restart Aurora manually.\r\nError:\r\n{exc}",
                        "Aurora Updater - Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private bool ExtractUpdate()
        {
            if (File.Exists(Path.Combine(Program.exePath, "update.zip")))
            {
                log.Enqueue(new LogEntry("Unpacking update..."));

                try
                {
                    ZipFile updateFile = new ZipFile(Path.Combine(Program.exePath, "update.zip"));
                    log.Enqueue(new LogEntry(updateFile.Count + " files detected."));

                    for (int i = 0; i < updateFile.Count; i++)
                    {
                        float percentage = ((float)i / (float)updateFile.Count);

                        ZipEntry fileEntry = updateFile[i];
                        log.Enqueue(new LogEntry("[" + Math.Truncate(percentage * 100) + "%] Updating: " + fileEntry.FileName));
                        this.extractProgess = (float)(Math.Truncate(percentage * 100) / 100.0f);

                        if (ignoreFiles.Contains(fileEntry.FileName))
                            continue;

                        try
                        {
                            if (File.Exists(Path.Combine(Program.exePath, fileEntry.FileName)))
                                File.Move(Path.Combine(Program.exePath, fileEntry.FileName), Path.Combine(Program.exePath, fileEntry.FileName + ".updateremove"));
                            fileEntry.Extract(Program.exePath);
                        }
                        catch (IOException e)
                        {
                            log.Enqueue(new LogEntry(fileEntry.FileName + " is inaccessible.", Color.Red));

                            MessageBox.Show(fileEntry.FileName + " is inaccessible.\r\nPlease close Aurora.\r\n\r\n" + e.Message);
                            i--;
                            continue;
                        }
                    }

                    updateFile.Dispose();
                    File.Delete(Path.Combine(Program.exePath, "update.zip"));
                }
                catch (Exception exc)
                {
                    log.Enqueue(new LogEntry(exc.Message, Color.Red));

                    return false;
                }

                log.Enqueue(new LogEntry("All files updated."));
                log.Enqueue(new LogEntry());
                log.Enqueue(new LogEntry("Updater will automatically restart Aurora."));
                this.extractProgess = 1.0f;

                return true;
            }
            else
            {
                this.log.Enqueue(new LogEntry("Update file not found.", Color.Red));
                return false;
            }
        }

        private void PerformCleanup()
        {
            string[] _messyFiles = Directory.GetFiles(Program.exePath, "*.updateremove", SearchOption.AllDirectories);

            foreach (string file in _messyFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception exc)
                {
                    log.Enqueue(new LogEntry("Unable to delete file - " + file, Color.Red));
                }
            }

            if (File.Exists(Path.Combine(Program.exePath, "update.zip")))
                File.Delete(Path.Combine(Program.exePath, "update.zip"));
        }
    }


    public class UpdateResponse
    {
        protected Newtonsoft.Json.Linq.JObject _ParsedData;
        protected string json;

        private UpdateInfoNode _Major;
        public UpdateInfoNode Major
        {
            get
            {
                if (_Major == null)
                {
                    _Major = new UpdateInfoNode(_ParsedData["major"]?.ToString() ?? "");
                }

                return _Major;
            }
        }

        private UpdateInfoNode _Minor;
        public UpdateInfoNode Minor
        {
            get
            {
                if (_Minor == null)
                {
                    _Minor = new UpdateInfoNode(_ParsedData["minor"]?.ToString() ?? "");
                }

                return _Minor;
            }
        }

        public UpdateResponse()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        public UpdateResponse(string json_data)
        {
            if (String.IsNullOrWhiteSpace(json_data))
            {
                json_data = "{}";
            }

            json = json_data;
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json_data);
        }

        public UpdateResponse(UpdateResponse other_state)
        {
            _ParsedData = other_state._ParsedData;
            json = other_state.json;
        }

        internal String GetNode(string name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(name, out value))
                return value.ToString();
            else
                return "";
        }

        public override string ToString()
        {
            return json;
        }
    }

    public class Node
    {
        protected Newtonsoft.Json.Linq.JObject _ParsedData;

        internal Node(string json_data)
        {
            if (String.IsNullOrWhiteSpace(json_data))
            {
                json_data = "{}";
            }
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json_data);
        }

        internal string GetString(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, out value))
                return value.ToString();
            else
                return "";
        }

        internal int GetInt(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, out value))
                return Convert.ToInt32(value.ToString());
            else
                return -1;
        }

        internal float GetFloat(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, out value))
                return Convert.ToSingle(value.ToString());
            else
                return -1.0f;
        }

        internal long GetLong(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, out value))
                return Convert.ToInt64(value.ToString());
            else
                return -1;
        }

        internal T GetEnum<T>(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, out value) && !String.IsNullOrWhiteSpace(value.ToString()))
            {
                var type = typeof(T);
                if (!type.IsEnum) throw new InvalidOperationException();
                foreach (var field in type.GetFields())
                {
                    var attribute = Attribute.GetCustomAttribute(field,
                        typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attribute != null)
                    {
                        if (attribute.Description.ToLowerInvariant().Equals(Name))
                            return (T)field.GetValue(null);
                    }

                    if (field.Name.ToLowerInvariant().Equals(Name))
                        return (T)field.GetValue(null);
                }

                return (T)Enum.Parse(typeof(T), "Undefined", true);
            }
            else
                return (T)Enum.Parse(typeof(T), "Undefined", true);
        }

        internal bool GetBool(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, out value) && value.ToObject<bool>())
                return value.ToObject<bool>();
            else
                return false;
        }

        internal T[] GetArray<T>(string Name)
        {
            Newtonsoft.Json.Linq.JToken value;

            if (_ParsedData.TryGetValue(Name, out value))
                return value.ToObject<T[]>();
            else
                return new T[] { };
        }
    }

    public enum UpdateType
    {
        Undefined,
        Major,
        Minor
    }

    public class UpdateInfoNode : Node
    {
        public readonly int ID;
        public readonly Version Version;
        public readonly string Title;
        public readonly string Description;
        public readonly string Changelog;
        public readonly string File;
        public readonly UpdateType Type;
        public readonly long FileSize;
        public readonly bool PreRelease;

        internal UpdateInfoNode(string JSON)
            : base(JSON)
        {
            ID = GetInt("id");
            Version = new Version(GetString("vnr"), true);
            Title = GetString("title");
            Description = GetString("desc").Replace("\\r\\n", "\r\n");
            Changelog = GetString("clog").Replace("\\r\\n", "\r\n");
            File = GetString("file");
            Type = GetEnum<UpdateType>("type");
            FileSize = GetLong("size");
            PreRelease = GetInt("prerelease") == 1 ? true : false;
        }
    }
}
