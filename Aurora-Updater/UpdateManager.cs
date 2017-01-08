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

        public Color getColor()
        {
            return color;
        }

        public String getMessage()
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
        private string infourl = @"http://aurora.lastbullet.net/vcheck.php";
        private string[] ignorefiles = { };
        public UpdateResponce responce = new UpdateResponce();
        private Queue<LogEntry> log = new Queue<LogEntry>();
        private float downloadprogess = 0.0f;
        private float extractprogess = 0.0f;
        public UpdateStatus updatestate = UpdateStatus.None;
        private int downloadprogresscheck = 0;
        private int seconds_left = 15;

        public UpdateManager()
        {
            performCleanup();
            fetchData();
        }

        public void clearLog()
        {
            log.Clear();
        }

        public LogEntry[] getLog()
        {
            return log.ToArray();
        }

        public int getTotalProgress()
        {
            return (int)((downloadprogess + extractprogess) / 2.0f * 100.0f);
        }

        private bool fetchData()
        {
            if (String.IsNullOrWhiteSpace(infourl))
                return false;

            try
            {
                WebClient client = new WebClient();
                string reply = client.DownloadString(infourl);

                responce = new UpdateResponce(reply);

                //Console.WriteLine(reply);
            }
            catch (Exception exc)
            {
                updatestate = UpdateStatus.Error;
                return false;
            }

            return true;
        }

        public bool retrieveUpdate(UpdateType type)
        {
            string url = @"http://aurora.modworkshop.net/files/" + (type == UpdateType.Major ? responce.Major.File : responce.Minor.File);
            updatestate = UpdateStatus.InProgress;
            try
            {
                if (!String.IsNullOrWhiteSpace(url))
                {
                    this.log.Enqueue(new LogEntry("Starting download... "));

                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

                    // Starts the download
                    client.DownloadFileAsync(new Uri(url), "update.zip");

                }

            }
            catch (Exception exc)
            {
                log.Enqueue(new LogEntry(exc.Message, Color.Red));
                updatestate = UpdateStatus.Error;
                return false;
            }
            return false;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes;

            downloadprogresscheck++;

            if (downloadprogresscheck % 10 == 0)
            {
                log.Enqueue(new LogEntry("Download " + Math.Truncate(percentage * 100) + "%"));
                this.downloadprogess = (float)(Math.Truncate(percentage * 100) / 100.0f);
            }
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.log.Enqueue(new LogEntry("Download complete."));
            this.log.Enqueue(new LogEntry());
            this.downloadprogess = 1.0f;

            if (extractUpdate())
            {
                System.Timers.Timer shutdown_timer = new System.Timers.Timer(1000);
                shutdown_timer.Elapsed += Shutdown_timer_Elapsed;
                shutdown_timer.Start();

                updatestate = UpdateStatus.Complete;
            }
            else
                updatestate = UpdateStatus.Error;
        }

        private void Shutdown_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (seconds_left > 0)
            {
                this.log.Enqueue(new LogEntry($"Restarting Aurora in {seconds_left} second{(seconds_left == 1 ? "" : "s")}..."));
                seconds_left--;
            }
            else
            {
                //Kill all Aurora instances
                foreach (Process proc in Process.GetProcessesByName("Aurora"))
                    proc.Kill();

                try
                {
                    ProcessStartInfo auroraProc = new ProcessStartInfo();
                    auroraProc.FileName = Path.Combine(Program.exe_path, "Aurora.exe");
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

        private bool extractUpdate()
        {
            if (File.Exists("update.zip"))
            {
                log.Enqueue(new LogEntry("Unpacking update..."));

                try
                {
                    ZipFile update_file = new ZipFile("update.zip");
                    log.Enqueue(new LogEntry(update_file.Count + " files detected."));

                    for (int i = 0; i < update_file.Count; i++)
                    {
                        float percentage = ((float)i / (float)update_file.Count);

                        ZipEntry fileentry = update_file[i];
                        log.Enqueue(new LogEntry("[" + Math.Truncate(percentage * 100) + "%] Updating: " + fileentry.FileName));
                        this.extractprogess = (float)(Math.Truncate(percentage * 100) / 100.0f);

                        if (ignorefiles.Contains(fileentry.FileName))
                            continue;

                        try
                        {
                            if (File.Exists(fileentry.FileName))
                                File.Move(fileentry.FileName, fileentry.FileName + ".updateremove");
                            fileentry.Extract();
                        }
                        catch (IOException e)
                        {
                            log.Enqueue(new LogEntry(fileentry.FileName + " is inaccessible.", Color.Red));

                            MessageBox.Show(fileentry.FileName + " is inaccessible.\r\nPlease close Aurora.\r\n\r\n" + e.Message);
                            i--;
                            continue;
                        }
                    }

                    update_file.Dispose();
                    File.Delete("update.zip");
                }
                catch (Exception exc)
                {
                    log.Enqueue(new LogEntry(exc.Message, Color.Red));

                    return false;
                }

                log.Enqueue(new LogEntry("All files updated."));
                log.Enqueue(new LogEntry());
                log.Enqueue(new LogEntry("Updater will automatically restart Aurora."));
                this.extractprogess = 1.0f;

                return true;
            }
            else
            {
                this.log.Enqueue(new LogEntry("Update file not found.", Color.Red));
                return false;
            }
        }

        private void performCleanup()
        {
            string[] messyfiles = Directory.GetFiles(".", "*.updateremove", SearchOption.AllDirectories);

            foreach (string file in messyfiles)
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

            if (File.Exists("update.zip"))
                File.Delete("update.zip");
        }
    }


    public class UpdateResponce
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

        public UpdateResponce()
        {
            json = "{}";
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        public UpdateResponce(string json_data)
        {
            if (String.IsNullOrWhiteSpace(json_data))
            {
                json_data = "{}";
            }

            json = json_data;
            _ParsedData = Newtonsoft.Json.Linq.JObject.Parse(json_data);
        }

        public UpdateResponce(UpdateResponce other_state)
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
                        if (attribute.Description.ToLowerInvariant().Equals(value.ToString().ToLowerInvariant()))
                            return (T)field.GetValue(null);
                    }

                    if (field.Name.ToLowerInvariant().Equals(value.ToString().ToLowerInvariant()))
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
        public readonly string Version;
        public readonly string Title;
        public readonly string Description;
        public readonly string Changelog;
        public readonly string File;
        public readonly UpdateType Type;

        internal UpdateInfoNode(string JSON)
            : base(JSON)
        {
            ID = GetInt("id");
            Version = GetString("vnr");
            Title = GetString("title");
            Description = GetString("desc").Replace("\\r\\n", "\r\n");
            Changelog = GetString("clog").Replace("\\r\\n", "\r\n");
            File = GetString("file");
            Type = GetEnum<UpdateType>("type");
        }
    }


}
