using System;
using System.IO;
using System.Text;
using Aurora;
using Newtonsoft.Json;

namespace Aurora_Updater;

public class UpdaterConfiguration
{
    private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", "Config");
    private const string ConfigExtension = ".json";
    
    public bool GetDevReleases { get; set; }

    public static UpdaterConfiguration Load()
    {
        UpdaterConfiguration config;
        try
        {
            config = TryLoad();
        }
        catch (Exception)
        {
            config = new UpdaterConfiguration();
        }

        return config;
    }
    
    private static UpdaterConfiguration TryLoad()
    {
        UpdaterConfiguration config;
        var configPath = ConfigPath + ConfigExtension;

        if (!File.Exists(configPath))
            config = new UpdaterConfiguration();
        else
        {
            var content = File.ReadAllText(configPath, Encoding.UTF8);
            config = string.IsNullOrWhiteSpace(content)
                ? new UpdaterConfiguration()
                : JsonConvert.DeserializeObject<UpdaterConfiguration>(content)!;
        }

        return config;
    }
}