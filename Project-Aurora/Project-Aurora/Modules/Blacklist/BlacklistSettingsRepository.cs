using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aurora.Modules.Blacklist.Model;
using Newtonsoft.Json;

namespace Aurora.Modules.Blacklist;

public static class BlacklistSettingsRepository
{
    private const string Owner = "Aurora-RGB";
    private const string Repo = "Online-Settings";
    private const string RepositoryUrl = $"https://raw.githubusercontent.com/{Owner}/{Repo}/master/";

    private const string ConflictingProcesses = "ConflictingProcesses.json";

    private static readonly string ConflictingProcessLocalCache = Path.Combine(Global.AppDataDirectory, ConflictingProcesses);

    public static async Task<ConflictingProcesses> GetConflictingProcesses()
    {
        var stream = await GetConflictingProcessStream();
        await using var jsonTextReader = new JsonTextReader(stream);

        var serializer = JsonSerializer.CreateDefault();
        var result = serializer.Deserialize<ConflictingProcesses>(jsonTextReader) ?? new ConflictingProcesses();

        //Save to local cache
        await using var jsonTextWriter = new JsonTextWriter(new StreamWriter(File.Create(ConflictingProcessLocalCache)));
        serializer.Serialize(jsonTextWriter, result);
        
        return result;
    }

    private static async Task<StreamReader> GetConflictingProcessStream()
    {
        const string url = RepositoryUrl + ConflictingProcesses;
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return new StreamReader(await response.Content.ReadAsStreamAsync());
        }

        if (!File.Exists(ConflictingProcessLocalCache))
        {
            return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)));
        }
        return File.OpenText(ConflictingProcessLocalCache);

    }
}