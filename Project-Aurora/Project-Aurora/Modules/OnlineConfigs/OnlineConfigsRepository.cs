using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aurora.Devices;
using Aurora.Modules.Blacklist.Model;
using Aurora.Modules.OnlineConfigs.Model;
using Newtonsoft.Json;

namespace Aurora.Modules.OnlineConfigs;

public static class OnlineConfigsRepository
{
    private const string Owner = "Aurora-RGB";
    private const string Repo = "Online-Settings";
    private const string RepositoryUrl = $"https://raw.githubusercontent.com/{Owner}/{Repo}/master/";

    private const string ConflictingProcesses = "ConflictingProcesses.json";
    private const string DeviceTooltips = "DeviceInformations.json";

    private static readonly string ConflictingProcessLocalCache = Path.Combine(Global.AppDataDirectory, ConflictingProcesses);
    private static readonly string DeviceTooltipsLocalCache = Path.Combine(Global.AppDataDirectory, DeviceTooltips);

    public static async Task<ConflictingProcesses> GetConflictingProcesses()
    {
        const string url = RepositoryUrl + ConflictingProcesses;
        var stream = await GetJsonStream(url, ConflictingProcessLocalCache);
        await using var jsonTextReader = new JsonTextReader(stream);

        var serializer = JsonSerializer.CreateDefault();
        var result = serializer.Deserialize<ConflictingProcesses>(jsonTextReader) ?? new ConflictingProcesses();

        //Save to local cache
        await using var jsonTextWriter = new JsonTextWriter(new StreamWriter(File.Create(ConflictingProcessLocalCache)));
        serializer.Serialize(jsonTextWriter, result);
        
        return result;
    }

    public static async Task<Dictionary<string, DeviceTooltips>> GetDeviceTooltips()
    {
        const string url = RepositoryUrl + DeviceTooltips;
        var stream = await GetJsonStream(url, DeviceTooltipsLocalCache);
        await using var jsonTextReader = new JsonTextReader(stream);

        var serializer = JsonSerializer.CreateDefault();
        var result = serializer.Deserialize<Dictionary<string, DeviceTooltips>>(jsonTextReader) ?? new Dictionary<string, DeviceTooltips>();

        //Save to local cache
        await using var jsonTextWriter = new JsonTextWriter(new StreamWriter(File.Create(DeviceTooltipsLocalCache)));
        serializer.Serialize(jsonTextWriter, result);
        
        return result;
    }

    private static async Task<StreamReader> GetJsonStream(string url, string cachePath)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return new StreamReader(await response.Content.ReadAsStreamAsync());
        }

        return File.Exists(cachePath) ? File.OpenText(cachePath) : new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)));
    }
}