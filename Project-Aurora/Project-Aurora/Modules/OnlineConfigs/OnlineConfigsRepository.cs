using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aurora.Modules.Blacklist.Model;
using Aurora.Modules.OnlineConfigs.Model;
using Newtonsoft.Json;

namespace Aurora.Modules.OnlineConfigs;

public static class OnlineConfigsRepository
{
    private const string ConflictingProcesses = "ConflictingProcesses.json";
    private const string DeviceTooltips = "DeviceInformations.json";
    private const string OnlineSettings = "OnlineSettings.json";

    private static readonly string ConflictingProcessLocalCache = Path.Combine("'", ConflictingProcesses);
    private static readonly string DeviceTooltipsLocalCache = Path.Combine(".", DeviceTooltips);
    private static readonly string OnlineSettingsLocalCache = Path.Combine(".", OnlineSettings);

    private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();

    public static async Task<ConflictingProcesses> GetConflictingProcesses()
    {
        return await ParseLocalJson<ConflictingProcesses>(ConflictingProcessLocalCache);
    }

    public static async Task<Dictionary<string, DeviceTooltips>> GetDeviceTooltips()
    {
        return await ParseLocalJson<Dictionary<string, DeviceTooltips>>(DeviceTooltipsLocalCache);
    }

    public static async Task<OnlineSettingsMeta> GetOnlineSettingsLocal()
    {
        return await ParseLocalJson<OnlineSettingsMeta>(OnlineSettingsLocalCache);
    }

    public static async Task<OnlineSettingsMeta> GetOnlineSettingsOnline()
    {
        var stream = await ReadOnlineJson(OnlineSettings);
        await using var jsonTextReader = new JsonTextReader(stream);

        return Serializer.Deserialize<OnlineSettingsMeta>(jsonTextReader) ?? new OnlineSettingsMeta();
    }

    private static async Task<T> ParseLocalJson<T>(string cachePath) where T : new()
    {
        var stream = GetJsonStream(cachePath);
        await using var jsonTextReader = new JsonTextReader(stream);

        return Serializer.Deserialize<T>(jsonTextReader) ?? new T();
    }

    private static StreamReader GetJsonStream(string cachePath)
    {
        return File.Exists(cachePath) ? File.OpenText(cachePath) : new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)));
    }

    private static async Task<StreamReader> ReadOnlineJson(string file)
    {
        var url = "https://raw.githubusercontent.com/Aurora-RGB/Online-Settings/master/" + file;
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return new StreamReader(await response.Content.ReadAsStreamAsync());
        }

        //internet check is done before but...
        throw new ApplicationException("Internet access is lost during update");
    }
}