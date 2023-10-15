using System.Collections.Generic;
using System.IO;
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

    private static readonly string ConflictingProcessLocalCache = Path.Combine("'", ConflictingProcesses);
    private static readonly string DeviceTooltipsLocalCache = Path.Combine(".", DeviceTooltips);

    public static async Task<ConflictingProcesses> GetConflictingProcesses()
    {
        var stream = GetJsonStream(ConflictingProcessLocalCache);
        await using var jsonTextReader = new JsonTextReader(stream);

        var serializer = JsonSerializer.CreateDefault();
        return serializer.Deserialize<ConflictingProcesses>(jsonTextReader) ?? new ConflictingProcesses();
    }

    public static async Task<Dictionary<string, DeviceTooltips>> GetDeviceTooltips()
    {
        var stream = GetJsonStream(DeviceTooltipsLocalCache);
        await using var jsonTextReader = new JsonTextReader(stream);

        var serializer = JsonSerializer.CreateDefault();
        return serializer.Deserialize<Dictionary<string, DeviceTooltips>>(jsonTextReader) ?? new Dictionary<string, DeviceTooltips>();
    }

    private static StreamReader GetJsonStream(string cachePath)
    {
        return File.Exists(cachePath) ? File.OpenText(cachePath) : new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)));
    }
}