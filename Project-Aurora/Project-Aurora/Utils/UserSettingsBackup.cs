using System.Diagnostics;
using System.IO;
using System.Reflection;
using SemanticVersioning;

namespace Aurora.Utils;

public class UserSettingsBackup
{
    private const string LastVersionTxt = "lastversion.txt";
    
    private string _previousVersion = "v95";

    public void BackupIfNew()
    {
        ReadPreviousVersion();
        var prevVersion = Version.Parse(_previousVersion + ".0.0", true);
        var currentVersion = Version.Parse(CurrentVersion() + ".0.0", true);
        if (currentVersion <= prevVersion) return;
        MakeBackup();
        WriteCurrentVersion();
    }

    private void ReadPreviousVersion()
    {
        var lastVersionFilePath = Path.Join(Global.AppDataDirectory, LastVersionTxt);
        if (File.Exists(lastVersionFilePath))
        {
            _previousVersion = File.ReadAllText(lastVersionFilePath);
        }
    }

    private void WriteCurrentVersion()
    {
        var lastVersionFilePath = Path.Combine(Global.AppDataDirectory, LastVersionTxt);
        
        File.WriteAllText(lastVersionFilePath, CurrentVersion());
    }

    private void MakeBackup()
    {
        var backupTarget = Path.GetFullPath(Path.Join(Global.AppDataDirectory, "..", "AuroraBackups", _previousVersion));
        CopyFilesRecursively(Global.AppDataDirectory, backupTarget);
    }
    
    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }

    private static string CurrentVersion()
    {
        return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
    }
}