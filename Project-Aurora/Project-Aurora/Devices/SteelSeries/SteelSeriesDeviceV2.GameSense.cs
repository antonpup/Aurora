using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Devices.SteelSeries
{
    public partial class SteelSeriesDeviceV2
    {
        private HttpClient client = new HttpClient {Timeout = TimeSpan.FromSeconds(30)};
        private JObject baseObject = new JObject();
        private Task pingTask;

        private void sendLispCode()
        {
            var core = (JObject) baseObject.DeepClone();
            core.Add("game_display_name", "Project Aurora");
            core.Add("icon_color_id", 0);
            sendJson("/game_metadata", core);
            core.Remove("game_display_name");
            core.Remove("icon_color_id");
            using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Aurora.Devices.SteelSeries.GoCode.lsp")))
            {
                core.Add("golisp", reader.ReadToEnd());
            }
            sendJson("/load_golisp_handlers", core);
            pingTask = Task.Run(sendPing);
        }

        private void loadCoreProps()
        {
            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SteelSeries/SteelSeries Engine 3/coreProps.json"));
            if (!file.Exists)
                throw new FileNotFoundException($"Core Props file could not be found.");

            var reader = file.OpenText();
            var coreProps = JObject.Parse(reader.ReadToEnd());
            reader.Dispose();
            client.BaseAddress = new Uri(coreProps["address"].ToString());
            sendLispCode();
        }

        private async Task sendPing()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await sendJsonAsync("/game_heartbeat", baseObject);
                }
                catch(Exception e)
                {
                    Global.logger.Error(e, "Error while sending heartbeat to SteelSeries Engine");
                }
            }
        }

        private void sendJson(string endpoint, object obj)
        {
            sendJsonAsync(endpoint, obj).GetAwaiter().GetResult();
        }

        private Task<HttpResponseMessage> sendJsonAsync(string endpoint, object obj)
        {
            return client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
        }
    }
}