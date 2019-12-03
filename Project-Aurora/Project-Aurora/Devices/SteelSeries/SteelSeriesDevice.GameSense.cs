using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Devices.SteelSeries
{
    public partial class SteelSeriesDevice
    {
        private HttpClient client = new HttpClient {Timeout = TimeSpan.FromSeconds(30)};
        private JObject baseObject = new JObject();
        private JObject baseColorObject = new JObject {{"Event", "AURORA"}, {"data", new JObject()}};
        private Task pingTask;
        private CancellationTokenSource pingTaskTokenSource = new CancellationTokenSource();
        private bool loadedLisp;
        private JToken dataColorObject => baseColorObject["data"];

        private void sendLispCode()
        {
            try
            {
                var core = (JObject)baseObject.DeepClone();
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
                pingTask = Task.Run(async () => await sendPing(pingTaskTokenSource.Token), pingTaskTokenSource.Token);
                loadedLisp = true;
            }
            catch (Exception e)
            {
                Global.logger.Error("SteelSeries Lisp Code failed: " + e);
                throw;
            }
        }

        private void loadCoreProps()
        {
            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SteelSeries/SteelSeries Engine 3/coreProps.json"));
            if (!file.Exists)
                throw new FileNotFoundException($"Core Props file could not be found.");

            var reader = file.OpenText();
            var coreProps = JObject.Parse(reader.ReadToEnd());
            reader.Dispose();
            client.BaseAddress = new Uri("http://" + coreProps["address"]);
            sendLispCode();
        }

        private void setKeyboardLed(byte led, Color color)
        {
            if(!((JObject)dataColorObject).ContainsKey("keyboard"))
                ((JObject) dataColorObject).Add("keyboard", new JObject {{"hids", new JArray()}, {"colors", new JArray()}});
            ((JArray) dataColorObject["keyboard"]["hids"]).Add(led);
            ((JArray) dataColorObject["keyboard"]["colors"]).Add(new JArray{color.R, color.G, color.B});
        }

        private void setOneZone(Color color)
        {
            ((JObject)dataColorObject).Add("onezone", new JObject{{"color",new JArray {color.R, color.G, color.B}}});
    }

        private void setTwoZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("twozone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setThreeZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("threezone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setFourZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("fourzone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setFiveZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("fivezone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setEightZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("eightzone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setTwelveZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("twelvezone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setSeventeenZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("seventeenzone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setTwentyFourZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("twentyfourzone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setHundredThreeZone(Color[] colors)
        {
            ((JObject) dataColorObject).Add("hundredthreezone", new JObject{{"colors", colorToJson(colors)}});
        }

        private void setLogo(Color color)
        {
            ((JObject) dataColorObject).Add("logo", new JObject{{"color",new JArray {color.R, color.G, color.B}}});
        }

        private void setWheel(Color color)
        {
            ((JObject) dataColorObject).Add("wheel", new JObject{{"color",new JArray {color.R, color.G, color.B}}});
        }

        private void setMouse(Color color)
        {
            ((JObject) dataColorObject).Add("mouse", new JObject{{"color",new JArray {color.R, color.G, color.B}}});
        }

        private void setGeneric(Color color)
        {
            ((JObject) dataColorObject).Add("periph", new JObject{{"color",new JArray {color.R, color.G, color.B}}});
        }

        private async Task sendPing(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await sendJsonAsync("/game_heartbeat", baseObject);
                }
                catch (Exception e)
                {
                    Global.logger.Error(e, "Error while sending heartbeat to SteelSeries Engine");
                    //To stop all other events from erroring
                    Shutdown();
                }
            }
        }

        private void sendLighting()
        {
            sendJson("/game_event", baseColorObject);
            ((JObject) dataColorObject).RemoveAll();
        }

        private void sendJson(string endpoint, object obj)
        {
            var t = sendJsonAsync(endpoint, obj).GetAwaiter().GetResult();
        }

        private Task<HttpResponseMessage> sendJsonAsync(string endpoint, object obj)
        {
            return client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json"));
        }

        private static JArray colorToJson(Color[] colors)
        {
            var arr = new JArray();
            foreach (var color in colors)
            {
                arr.Add(new JArray{color.R,color.G,color.B});
            }
            return arr;
        }
    }
}