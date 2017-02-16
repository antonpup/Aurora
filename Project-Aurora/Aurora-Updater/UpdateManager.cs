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
        public UpdateResponce responce = new UpdateResponce();
        private Queue<LogEntry> log = new Queue<LogEntry>();
        private float downloadProgess = 0.0f;
        private float extractProgess = 0.0f;
        public UpdateStatus updateState = UpdateStatus.None;
        private int downloadProgressCheck = 0;
        private int secondsLeft = 15;

        public UpdateManager()
        {
            PerformCleanup();
            FetchData();
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

        private bool FetchData()
        {
            if (String.IsNullOrWhiteSpace(infoUrl))
                return false;

            try
            {
                WebClient client = new WebClient();
                string reply = client.DownloadString(infoUrl);

                responce = new UpdateResponce(reply);

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
            string url = @"http://project-aurora.com/download.php?id=" + (type == UpdateType.Major ? responce.Major.ID : responce.Minor.ID);
            updateState = UpdateStatus.InProgress;
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
            if (File.Exists("update.zip"))
            {
                log.Enqueue(new LogEntry("Unpacking update..."));

                try
                {
                    ZipFile updateFile = new ZipFile("update.zip");
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
                            if (File.Exists(fileEntry.FileName))
                                File.Move(fileEntry.FileName, fileEntry.FileName + ".updateremove");
                            fileEntry.Extract();
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
            string[] _messyFiles = Directory.GetFiles(".", "*.updateremove", SearchOption.AllDirectories);

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

    public class UpdateVersion
    {
        //0.6.0b
        //FinalVersion.MajorVersion.MinorVersion{AdditionalVersionModifier}
        public int FinalVersion = 0;
        public int MajorVersion = 0;
        public int MinorVersion = 0;
        public string AdditionalVersionModifier = "";

        public UpdateVersion(string version)
        {
            int _final = 0;
            int _major = 0;
            int _minor = 0;

            var verSplit = version.Split('.');

            if (verSplit.Count() == 3)
            {
                if (int.TryParse(verSplit[0], out _final))
                    FinalVersion = _final;

                if (int.TryParse(verSplit[1], out _major))
                    MajorVersion = _major;

                if (int.TryParse(verSplit[2], out _minor))
                    MinorVersion = _minor;
                else
                {
                    StringBuilder sbMinor = new StringBuilder();
                    StringBuilder sbAdditional = new StringBuilder();
                    bool _readingMinor = true;

                    foreach (char c in verSplit[2])
                    {
                        switch (c)
                        {
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                if (_readingMinor)
                                    sbMinor.Append(c);
                                else
                                    sbAdditional.Append(c);
                                break;
                            default:
                                _readingMinor = false;
                                sbAdditional.Append(c);
                                break;
                        }

                    }

                    if (int.TryParse(sbMinor.ToString(), out _minor))
                        MinorVersion = _minor;

                    AdditionalVersionModifier = sbAdditional.ToString();
                }
            }
        }

        public static bool operator ==(UpdateVersion lhs, UpdateVersion rhs)
        {
            if (lhs.FinalVersion == rhs.FinalVersion ||
                lhs.MajorVersion == rhs.MajorVersion ||
                lhs.MinorVersion == rhs.MinorVersion ||
                String.Compare(lhs.AdditionalVersionModifier, rhs.AdditionalVersionModifier) == 0
                )
                return true;

            return false;
        }

        public static bool operator !=(UpdateVersion lhs, UpdateVersion rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator <(UpdateVersion lhs, UpdateVersion rhs)
        {
            if (lhs.FinalVersion < rhs.FinalVersion ||
                lhs.MajorVersion < rhs.MajorVersion ||
                lhs.MinorVersion < rhs.MinorVersion ||
                String.Compare(lhs.AdditionalVersionModifier, rhs.AdditionalVersionModifier) < 0
                )
                return true;

            return false;
        }

        public static bool operator >(UpdateVersion lhs, UpdateVersion rhs)
        {
            if (lhs.FinalVersion > rhs.FinalVersion ||
                lhs.MajorVersion > rhs.MajorVersion ||
                lhs.MinorVersion > rhs.MinorVersion ||
                String.Compare(lhs.AdditionalVersionModifier, rhs.AdditionalVersionModifier) > 0
                )
                return true;

            return false;
        }

        public static bool operator <=(UpdateVersion lhs, UpdateVersion rhs)
        {
            if (lhs.FinalVersion <= rhs.FinalVersion &&
                lhs.MajorVersion <= rhs.MajorVersion &&
                lhs.MinorVersion <= rhs.MinorVersion &&
                String.Compare(lhs.AdditionalVersionModifier, rhs.AdditionalVersionModifier) <= 0
                )
                return true;

            return false;
        }

        public static bool operator >=(UpdateVersion lhs, UpdateVersion rhs)
        {
            if (lhs.FinalVersion >= rhs.FinalVersion &&
                lhs.MajorVersion >= rhs.MajorVersion &&
                lhs.MinorVersion >= rhs.MinorVersion &&
                String.Compare(lhs.AdditionalVersionModifier, rhs.AdditionalVersionModifier) >= 0
                )
                return true;

            return false;
        }

        public override string ToString()
        {
            return $"{FinalVersion}.{MajorVersion}.{MinorVersion}{AdditionalVersionModifier}";
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
        public readonly UpdateVersion Version;
        public readonly string Title;
        public readonly string Description;
        public readonly string Changelog;
        public readonly string File;
        public readonly UpdateType Type;
        public readonly long FileSize;

        internal UpdateInfoNode(string JSON)
            : base(JSON)
        {
            ID = GetInt("id");
            Version = new UpdateVersion(GetString("vnr"));
            Title = GetString("title");
            Description = GetString("desc").Replace("\\r\\n", "\r\n");
            Changelog = GetString("clog").Replace("\\r\\n", "\r\n");
            File = GetString("file");
            Type = GetEnum<UpdateType>("type");
            FileSize = GetLong("size");
        }
    }
}
