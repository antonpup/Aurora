// GameSenseSDK C# beta by brainbug89 is licensed under CC BY-NC-SA 4.0

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SteelSeries.GameSenseSDK
{

    public class SSECorePropsJSON
    {
        public String address { get; set; }
        public String encrypted_address { get; set; }
    }

    public class GameSensePayloadLISPHandlerJSON
    {
        public String game { get; set; }
        public String golisp { get; set; }
    }

    public class GameSensePayloadHeartbeatJSON
    {
        public String game { get; set; }
    }

    public class GameSensePayloadGameDataJSON
    {
        public String game { get; set; }
        public String game_display_name { get; set; }
        public byte icon_color_id { get; set; }
    }

    public class GameSensePayloadPeripheryColorEventJSON
    {
        public String game { get; set; }
        public String Event { get; set; }
        public String data { get; set; }
    }

    public class GameSenseSDK
    {
        private String COREPROPS_JSON_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/SteelSeries/SteelSeries Engine 3/coreProps.json";
        private String sseGameName = "";
        private String sseGameDisplayname = "";

        private static String sseAddress = "";

        public void init(String sseGameName, String sseGameDisplayname, byte iconColorID)
        {

            if (!File.Exists(COREPROPS_JSON_PATH))
                throw new FileNotFoundException($"Core Props file could not be found at \"{COREPROPS_JSON_PATH}\"");

            // read %PROGRAMDATA%/SteelSeries/SteelSeries Engine 3/coreProps.json
            SSECorePropsJSON coreProps = JsonConvert.DeserializeObject<SSECorePropsJSON>(File.ReadAllText(@COREPROPS_JSON_PATH));
            sseAddress = coreProps.address;

            // setup "game" meda data
            this.sseGameName = sseGameName;
            this.sseGameDisplayname = sseGameDisplayname;
            setupGame(iconColorID);

            // setup golisp handler
            setupLISPHandlers();
        }

        public void setPeripheryColor(byte red, byte green, byte blue)
        {
            sendColor("periph", red, green, blue);
        }

        public void setMouseColor(byte red, byte green, byte blue)
        {
            sendColor("mouse", red, green, blue);
        }

        public void setMouseScrollWheelColor(byte red, byte green, byte blue)
        {
            sendColor("mousewheel", red, green, blue);
        }

        public void setMouseLogoColor(byte red, byte green, byte blue)
        {
            sendColor("mouselogo", red, green, blue);
        }

        public void setHeadsetColor(byte red, byte green, byte blue)
        {
            sendColor("headset", red, green, blue);
        }

        public void sendColor(String deviceType, byte red, byte green, byte blue)
        {
            GameSensePayloadPeripheryColorEventJSON payload = new GameSensePayloadPeripheryColorEventJSON();
            payload.game = sseGameName;
            payload.Event = "COLOR";
            payload.data = "{\""+ deviceType + "\":{\"color\": [" + red + ", " + green + ", " + blue + "]}}";
            // sending POST request
            String json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/game_event", json);
        }

        public void setKeyboardColors(List<byte> hids, List<Tuple<byte, byte, byte>> colors)
        {
            GameSensePayloadPeripheryColorEventJSON payload = new GameSensePayloadPeripheryColorEventJSON();
            payload.game = sseGameName;
            payload.Event = "COLOR";

            payload.data = "{";
            payload.data += "\"keyboard\":{";
            payload.data += "\"hids\":";
            payload.data += JsonConvert.SerializeObject(hids);
            payload.data += ",";
            payload.data += "\"colors\":[";
            foreach (Tuple<byte, byte, byte> color in colors)
            {
                payload.data += "[" + color.Item1 + ", " + color.Item2 + ", " + color.Item3 + "],";
            }
            // JSON doesn't allow trailing commas
            payload.data = payload.data.TrimEnd(',');
            payload.data += "]";
            payload.data += "}";
            payload.data += "}";

            // sending POST request
            String json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/game_event", json);
        }

        public void sendHeartbeat()
        {
            GameSensePayloadHeartbeatJSON payload = new GameSensePayloadHeartbeatJSON();
            payload.game = sseGameName;
            // sending POST request
            String json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/game_heartbeat", json);
        }

        public void sendStop()
        {
            GameSensePayloadPeripheryColorEventJSON payload = new GameSensePayloadPeripheryColorEventJSON();
            payload.game = sseGameName;
            payload.Event = "STOP";
            // sending POST request
            String json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/game_event", json);
        }

        private void setupLISPHandlers()
        {
            String json = "";
            GameSensePayloadLISPHandlerJSON payload = new GameSensePayloadLISPHandlerJSON();
            payload.game = sseGameName;

            // sending POST requests with golisp handler
            payload.golisp = @"
(handler ""COLOR""
    (lambda (data)
        (when (keyboard:? data)
            (let* ((keyboard (keyboard: data))
                   (hids (hids: keyboard))
                   (colors (colors: keyboard)))
                (on-device ""rgb-per-key-zones"" show-on-keys: hids colors)))

        (when (periph:? data)
            (let* ((periph (periph: data))
                   (color (color: periph)))
                (on-device ""rgb-1-zone"" show: color)
                (on-device ""rgb-2-zone"" show: color)
                (on-device ""rgb-3-zone"" show: color)
                (on-device ""rgb-4-zone"" show: color)
                (on-device ""rgb-5-zone"" show: color)
                (on-device ""rgb-12-zone"" show: color)))

        (when (mouse:? data)
            (let* ((mouse (mouse: data))
                   (color (color: mouse)))
                (on-device ""mouse"" show: color)))

        (when (mousewheel:? data)
            (let* ((mousewheel (mousewheel: data))
                   (color (color: mousewheel)))
                (on-device ""mouse"" show-on-zone: color wheel:)))

        (when (mouselogo:? data)
            (let* ((mouselogo (mouselogo: data))
                   (color (color: mouselogo)))
                (on-device ""mouse"" show-on-zone: color logo:)))

        (when (headset:? data)
            (let* ((headset (headset: data))
                   (color (color: headset)))
                (on-device ""headset"" show: color)))
                (on-device ""earcups"" show: color)))
    )
)

(add-event-zone-use-with-specifier ""COLOR"" ""all"" ""rgb-1-zone"")
(add-event-zone-use-with-specifier ""COLOR"" ""all"" ""rgb-2-zone"")
(add-event-zone-use-with-specifier ""COLOR"" ""all"" ""rgb-3-zone"")
(add-event-zone-use-with-specifier ""COLOR"" ""all"" ""rgb-4-zone"")
(add-event-zone-use-with-specifier ""COLOR"" ""all"" ""rgb-5-zone"")
(add-event-zone-use-with-specifier ""COLOR"" ""all"" ""rgb-12-zone"")
(add-event-per-key-zone-use ""COLOR"" ""all"")
";
            json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/load_golisp_handlers", json);

            /*payload.golisp = "(handler \"STOP\" (lambda (data)    (send Generic-Initializer deinitialize:)))";
            // sending POST request
            json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/load_golisp_handlers", json);*/
        }

        private void setupGame(byte iconColorID)
        {
            GameSensePayloadGameDataJSON payload = new GameSensePayloadGameDataJSON();
            payload.game = sseGameName;
            payload.game_display_name = sseGameDisplayname;
            payload.icon_color_id = iconColorID;
            // sending POST request
            String json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/game_metadata", json);   
        }

        private void sendPostRequest(String address, String payload)
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(address);
            httpWebRequest.ReadWriteTimeout = 30;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(payload);
            }
            // sending POST request
            var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                streamReader.ReadToEnd();
            }
        }
        
    }

}