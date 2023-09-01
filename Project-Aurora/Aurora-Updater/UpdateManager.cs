using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Octokit;
using Timer = System.Timers.Timer;
using Version = SemanticVersioning.Version;

namespace Aurora_Updater;

public class LogEntry
{
    private readonly string _message;
    private readonly Color _color;

    public LogEntry()
    {
        _message = string.Empty;
        _color = Color.Black;
    }

    public LogEntry(string message)
    {
        _message = message;
        _color = Color.Black;
    }

    public LogEntry(string message, Color color)
    {
        _message = message;
        _color = color;
    }

    public Color GetColor()
    {
        return _color;
    }

    public string GetMessage()
    {
        return _message;
    }

    public override string ToString()
    {
        return _message;
    }
}

public class UpdateManager
{
    private readonly string[] _ignoreFiles = { };
    private readonly Queue<LogEntry> _log = new();
    private float _downloadProgress;
    private float _extractProgress;
    private int? _previousPercentage;
    private int _secondsLeft = 3;
    private readonly UpdaterConfiguration _config;
    private readonly GitHubClient _gClient;
    public readonly Release LatestRelease;

    public UpdateManager(Version version, string author, string repoName)
    {
        _gClient = new(new ProductHeaderValue("aurora-updater", version.ToString()));
        try
        {
            _config = UpdaterConfiguration.Load();
        }
        catch
        {
            _config = new UpdaterConfiguration();
        }

        PerformCleanup();
        var tries = 20;
        while (LatestRelease == null && tries-- != 0)
        {
            try
            {
                LatestRelease = FetchData(version, author, repoName);
            }
            catch (AggregateException e)
            {
                if (e.InnerException is HttpRequestException && tries != 0)
                {
                    Thread.Sleep(5000);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    public void ClearLog()
    {
        _log.Clear();
    }

    public LogEntry[] GetLog()
    {
        return _log.ToArray();
    }

    public int GetTotalProgress()
    {
        return (int)((_downloadProgress + _extractProgress) / 2.0f * 100.0f);
    }

    private Release FetchData(Version version, string owner, string repositoryName)
    {
        if (_config.GetDevReleases || !string.IsNullOrWhiteSpace(version.PreRelease))
            return _gClient.Repository.Release.GetAll(owner, repositoryName, new ApiOptions { PageCount = 1, PageSize = 1 }).Result[0];
        return  _gClient.Repository.Release.GetLatest(owner, repositoryName).Result;
    }

    public async Task RetrieveUpdate()
    {
        try
        {
            var assets = LatestRelease.Assets;
            var url = assets.First(s => s.Name.StartsWith("release") || s.Name.StartsWith("Aurora-v")).BrowserDownloadUrl;

            if (string.IsNullOrWhiteSpace(url)) return;
            _log.Enqueue(new LogEntry("Starting download... "));

            using var client = new WebClient();
            client.DownloadProgressChanged += client_DownloadProgressChanged;

            // Starts the download
            await client.DownloadFileTaskAsync(new Uri(url), Path.Combine(Program.ExePath, "update.zip"));

            foreach (var pluginDll in assets.Where(a => a.Name.EndsWith(".dll")))
            {
                var address = new Uri(pluginDll.BrowserDownloadUrl);
                var installDirPlugin = Path.Combine(Program.ExePath, "Plugins", pluginDll.Name);
                if (File.Exists(installDirPlugin))
                {
                    _log.Enqueue(new LogEntry("Updating " + pluginDll.Name));
                    await client.DownloadFileTaskAsync(address, installDirPlugin);
                }

                var userDirPlugin = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Plugin");
                if (File.Exists(userDirPlugin))
                {
                    _log.Enqueue(new LogEntry("Updating " + pluginDll.Name));
                    await client.DownloadFileTaskAsync(address, userDirPlugin);
                }
            }

            _log.Enqueue(new LogEntry("Download complete."));
            _log.Enqueue(new LogEntry());
            _downloadProgress = 1.0f;

            if (ExtractUpdate())
            {
                var shutdownTimer = new Timer(1000);
                shutdownTimer.Elapsed += ShutdownTimerElapsed;
                shutdownTimer.Start();
            }
        }
        catch (Exception exc)
        {
            _log.Enqueue(new LogEntry(exc.Message, Color.Red));
        }
    }

    private void client_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        var bytesIn = double.Parse(e.BytesReceived.ToString());
        var totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
        var percentage = bytesIn / totalBytes;

        var newPercentage = (int)(percentage * 100);
        if (_previousPercentage == newPercentage)
            return;

        _previousPercentage = newPercentage;

        _log.Enqueue(new LogEntry("Download " + newPercentage + "%"));
        _downloadProgress = newPercentage / 100.0f;
    }

    private void ShutdownTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_secondsLeft > 0)
        {
            _log.Enqueue(new LogEntry($"Restarting Aurora in {_secondsLeft} second{(_secondsLeft == 1 ? "" : "s")}..."));
            _secondsLeft--;
        }
        else
        {
            //Kill all Aurora instances
            foreach (var proc in Process.GetProcessesByName("Aurora"))
                proc.Kill();

            try
            {
                var auroraProc = new ProcessStartInfo();
                auroraProc.FileName = Path.Combine(Program.ExePath, "Aurora.exe");
                Process.Start(auroraProc);

                Environment.Exit(0); //Exit, no further action required
            }
            catch (Exception exc)
            {
                _log.Enqueue(new LogEntry($"Could not restart Aurora. Error:\r\n{exc}", Color.Red));
                _log.Enqueue(new LogEntry("Please restart Aurora manually.", Color.Red));

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
        if (File.Exists(Path.Combine(Program.ExePath, "update.zip")))
        {
            _log.Enqueue(new LogEntry("Unpacking update..."));

            try
            {
                var updateFile = ZipFile.OpenRead(Path.Combine(Program.ExePath, "update.zip"));
                var countOfEntries = updateFile.Entries.Count;
                _log.Enqueue(new LogEntry($"{countOfEntries} files detected."));

                for (var i = 0; i < countOfEntries; i++)
                {
                    var percentage = i / (float)countOfEntries;

                    var fileEntry = updateFile.Entries[i];
                    _log.Enqueue(new LogEntry($"[{Math.Truncate(percentage * 100)}%] Updating: {fileEntry.FullName}"));
                    _extractProgress = (float)(Math.Truncate(percentage * 100) / 100.0f);

                    if (_ignoreFiles.Contains(fileEntry.FullName))
                        continue;

                    try
                    {
                        var filePath = Path.Combine(Program.ExePath, fileEntry.FullName);
                        if (File.Exists(filePath))
                            File.Move(filePath, $"{filePath}.updateremove");
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        fileEntry.ExtractToFile(filePath);
                    }
                    catch (IOException e)
                    {
                        _log.Enqueue(new LogEntry($"{fileEntry.FullName} is inaccessible.", Color.Red));

                        MessageBox.Show($"{fileEntry.FullName} is inaccessible.\r\nPlease close Aurora.\r\n\r\n {e.Message}");
                        i--;
                    }
                }

                updateFile.Dispose();
                File.Delete(Path.Combine(Program.ExePath, "update.zip"));
            }
            catch (Exception exc)
            {
                _log.Enqueue(new LogEntry(exc.Message, Color.Red));

                return false;
            }

            _log.Enqueue(new LogEntry("All files updated."));
            _log.Enqueue(new LogEntry());
            _log.Enqueue(new LogEntry("Updater will automatically restart Aurora."));
            _extractProgress = 1.0f;

            return true;
        }

        _log.Enqueue(new LogEntry("Update file not found.", Color.Red));
        return false;
    }

    private void PerformCleanup()
    {
        var messyFiles = Directory.GetFiles(Program.ExePath, "*.updateremove", SearchOption.AllDirectories);

        foreach (var file in messyFiles)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                _log.Enqueue(new LogEntry("Unable to delete file - " + file, Color.Red));
            }
        }

        if (File.Exists(Path.Combine(Program.ExePath, "update.zip")))
            File.Delete(Path.Combine(Program.ExePath, "update.zip"));
    }
}

public enum UpdateType
{
    Undefined,
    Major,
    Minor
}