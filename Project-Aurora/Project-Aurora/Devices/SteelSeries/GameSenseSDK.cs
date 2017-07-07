// GameSenseSDK C# beta by brainbug89 is licensed under CC BY-NC-SA 4.0

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using Aurora;

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
            sendColor("PERIPHERYCOLOR", red, green, blue);
        }

        public void setMouseColor(byte red, byte green, byte blue)
        {
            sendColor("MOUSECOLOR", red, green, blue);
        }

        public void setMouseScrollWheelColor(byte red, byte green, byte blue)
        {
            sendColor("MOUSEWHEELCOLOR", red, green, blue);
        }

        public void setMouseLogoColor(byte red, byte green, byte blue)
        {
            sendColor("MOUSELOGOCOLOR", red, green, blue);
        }

        public void setHeadsetColor(byte red, byte green, byte blue)
        {
            sendColor("HEADSETCOLOR", red, green, blue);
        }

        public void sendColor(String deviceEvent, byte red, byte green, byte blue)
        {
            GameSensePayloadPeripheryColorEventJSON payload = new GameSensePayloadPeripheryColorEventJSON();
            payload.game = sseGameName;
            payload.Event = deviceEvent;
            payload.data = "{\"value\": [" + red + ", " + green + ", " + blue + "]}";
            // sending POST request
            String json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/game_event", json);
        }

        public void setKeyboardColors(List<byte> hids, List<Tuple<byte, byte, byte>> colors)
        {
            GameSensePayloadPeripheryColorEventJSON payload = new GameSensePayloadPeripheryColorEventJSON();
            payload.game = sseGameName;
            payload.Event = "KEYBOARDCOLORS";

            payload.data = "{";
            payload.data += "\"hids\": ";
            payload.data += JsonConvert.SerializeObject(hids);
            payload.data += ",";
            payload.data += "\"colors\": [";
            foreach (Tuple<byte, byte, byte> color in colors)
            {
                payload.data += "[" + color.Item1 + ", " + color.Item2 + ", " + color.Item3 + "], ";
            }
            payload.data += "]";
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
            payload.golisp = "(handler \"PERIPHERYCOLOR\"  (lambda (data)    (let* ((val (value: data)))      (on-device \"rgb-2-zone\" show: val) (on-device \"rgb-1-zone\" show: val))))";
            json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/load_golisp_handlers", json);

            payload.golisp = "(handler \"MOUSECOLOR\"  (lambda (data)    (let* ((val (value: data)))      (on-device \"mouse\" show: val))))";
            json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/load_golisp_handlers", json);

            payload.golisp = "(handler \"MOUSEWHEELCOLOR\"  (lambda (data)    (let* ((val (value: data)))      (on-device \"mouse\" show-on-zone: val wheel:))))";
            json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/load_golisp_handlers", json);

            payload.golisp = "(handler \"MOUSELOGOCOLOR\"  (lambda (data)    (let* ((val (value: data)))      (on-device \"mouse\" show-on-zone: val logo:))))";
            json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/load_golisp_handlers", json);

            payload.golisp = "(handler \"HEADSETCOLOR\"  (lambda (data)    (let* ((val (value: data)))      (on-device \"headset\" show: val))))";
            json = JsonConvert.SerializeObject(payload);
            sendPostRequest("http://" + sseAddress + "/load_golisp_handlers", json);

            //keyboard beta handler
            payload.golisp = "(handler \"KEYBOARDCOLORS\"  (lambda (data)    (let* ((hids (take 124 (hids: data)))    (colors (take 124 (colors: data))))      (on-device \"rgb-per-key-zones\" show-on-keys: hids colors)))) (add-event-per-key-zone-use \"KEYBOARDCOLORS\" \"all\")";
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
            //Global.logger.LogLine("GameSenseSDK debug: payload of POST: " + payload, Logging_Level.Info);

            var httpWebRequest = (HttpWebRequest) WebRequest.Create(address);
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
                var result = streamReader.ReadToEnd();
                //Global.logger.LogLine("GameSenseSDK debug: result of POST: " + result, Logging_Level.Info);
            }
        }
        
    }

}