using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Aurora.Utils.IpApi;

public static class IpApiClient
{
    public static async Task<IpData> GetIpData()
    {
        using HttpClient client = new();
        var response = await client.GetFromJsonAsync<IpData>("http://ip-api.com/json?fields=status,message,lat,lon");

        if (response == null)
        {
            throw new ApplicationException("IpApi returned empty response");
        }
        if (response.Status != "success")
        {
            throw new ApplicationException("IpApi returned error: " + response.Message);
        }
        
        return response;
    }
}