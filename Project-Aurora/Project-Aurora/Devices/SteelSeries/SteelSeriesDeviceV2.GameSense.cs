using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Devices.SteelSeries
{
    public partial class SteelSeriesDeviceV2
    {
        private HttpClient client = new HttpClient();
        private JObject baseObject;

        private void sendLispCode()
        {

        }

        private void loadCoreProps()
        {
            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SteelSeries/SteelSeries Engine 3/coreProps.json"));
            if(!file.Exists)
                throw new FileNotFoundException($"Core Props file could not be found.");

            var reader = file.OpenText();
            var coreProps = JObject.Parse(reader.ReadToEnd());
            reader.Dispose();
            client.BaseAddress = new Uri(coreProps["address"].ToString());
            sendLispCode();
        }

        private void sendJson(string endpoint, object obj)
        {
            client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
        }
    }

    static class GoLisp
    {
        public static string Code = @"";
    }
}
