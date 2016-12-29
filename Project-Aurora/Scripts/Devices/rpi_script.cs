using Aurora;
using Aurora.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class RPiDeviceScript
{
    //!!!!!!!!!! SCRIPT SETTINGS !!!!!!!!!!//
    public bool enabled = true; //Switch to True, to enable it in Aurora
    private static readonly int NUMBER_OF_LEDS = 32; //Number of LEDs connected to your Raspberry pi
    private static readonly string PI_URL = "http://10.0.0.150:8032/"; //The URL to which requests will be sent to
    private static readonly string PI_URL_SUFFIX_START = "start";
    private static readonly string PI_URL_SUFFIX_STOP = "stop";
    private static readonly string PI_URL_SUFFIX_SETLIGHTS = "lights";
    private static readonly bool WAIT_FOR_RESPONSE = true; //Should this script wait for a response from Raspberry pi

    private static readonly Dictionary<DeviceKeys, int[]> PI_LED_MAPPING = new Dictionary<DeviceKeys, int[]>()
        {
            { DeviceKeys.ESC, new int[] { 0, 1 } }, //Aurora's ESC key maps to PI's LED lights 0 and 1
            { DeviceKeys.F1, new int[] { 2, 3 } }, //Aurora's F1 key maps to PI's LED lights 2 and 1
            { DeviceKeys.F2, new int[] { 4, 5 } }, // etc...
            { DeviceKeys.F3, new int[] { 6, 7 } },
            { DeviceKeys.F4, new int[] { 8, 9 } },
            { DeviceKeys.F5, new int[] { 10, 11 } },
            { DeviceKeys.F6, new int[] { 12, 13 } },
            { DeviceKeys.F7, new int[] { 14, 15 } },
            { DeviceKeys.F8, new int[] { 16, 17 } },
            { DeviceKeys.F9, new int[] { 18, 19 } },
            { DeviceKeys.F10, new int[] { 20, 21 } },
            { DeviceKeys.F11, new int[] { 22, 23 } },
            { DeviceKeys.F12, new int[] { 24, 25 } }
        };

    public string devicename = "Raspberry Pi Device Script";
    private bool initialized = false;

    private Color[] device_colors;

    private enum ActionCodes
    {
        None = 0,
        Initialize = 1,
        Stop = 2,
        SetLightis = 3
    }

    public bool Initialize()
    {
        try
        {
            if (!initialized)
            {
                //Perform necessary actions to initialize your device
                Reset();

                initialized = SendJsonToRPI(ActionCodes.Initialize) == HttpStatusCode.OK;
            }
            return initialized;
        }
        catch (Exception exc)
        {
            Global.logger.LogLine("[" + devicename + "] Exception: " + exc.ToString());

            return false;
        }
    }

    public void Reset()
    {
        //Perform necessary actions to reset your device
        device_colors = new Color[NUMBER_OF_LEDS];

        for (int col_i = 0; col_i < NUMBER_OF_LEDS; col_i++)
            device_colors[col_i] = Color.FromArgb(0, 0, 0);
    }

    public void Shutdown()
    {
        if (initialized)
        {
            //Perform necessary actions to shutdown your device
            SendJsonToRPI(ActionCodes.Stop);

            initialized = false;
        }
    }

    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
    {
        try
        {
            //Gather colors
            Color[] newColors = new Color[NUMBER_OF_LEDS];

            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                //Iterate over each key and color and prepare to send them to the device
                if (PI_LED_MAPPING.ContainsKey(key.Key))
                {
                    foreach (int id in PI_LED_MAPPING[key.Key])
                    {
                        newColors[id] = key.Value;
                    }
                }
            }

            System.Threading.Tasks.Task.Run(() => SendColorsToDevice(newColors, forced));

            return true;
        }
        catch (Exception exc)
        {
            Global.logger.LogLine("[" + devicename + "] Exception: " + exc.ToString());

            return false;
        }
    }

    //Custom method to send the color to the device
    private void SendColorsToDevice(Color[] colors, bool forced)
    {
        //Check if device's current color is the same, no need to update if they are the same
        if (!Enumerable.SequenceEqual(colors, device_colors) || forced)
        {
            RPI_Packet packet = new RPI_Packet(colors);

            if (!WAIT_FOR_RESPONSE || SendJsonToRPI(ActionCodes.SetLightis, packet) == HttpStatusCode.OK)
            {
                // Pi responded! Colors must've been set
                device_colors = colors;
            }
        }
    }

    private HttpStatusCode SendJsonToRPI(ActionCodes action, RPI_Packet packet = null)
    {
        if (!initialized && action != ActionCodes.Initialize)
            return HttpStatusCode.NotFound;

        string request_url = PI_URL;

        switch (action)
        {
            case ActionCodes.Initialize:
                request_url += PI_URL_SUFFIX_START;
                break;
            case ActionCodes.Stop:
                request_url += PI_URL_SUFFIX_STOP;
                break;
            case ActionCodes.SetLightis:
                request_url += PI_URL_SUFFIX_SETLIGHTS;
                break;
        }

        var httpWebRequest = (HttpWebRequest)WebRequest.Create(request_url);
        ServicePointManager.DefaultConnectionLimit = 15;
        httpWebRequest.Proxy = null;
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";
        httpWebRequest.KeepAlive = false;

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            if (packet != null)
                streamWriter.Write(packet.ToJson());
            streamWriter.Flush();
            streamWriter.Close();
        }

        HttpStatusCode code = HttpStatusCode.NotFound;
        using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
        {
            code = httpResponse.StatusCode;

            httpResponse.Close();
        }

        return code;
    }
}

public class RPI_Packet
{
    public int[] col = null;

    public RPI_Packet()
    {
    }

    public RPI_Packet(Color[] colors)
    {
        col = new int[colors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            Color c = colors[i];

            if (c == null)
                col[i] = 0; //Completely black
            else
                col[i] = (c.R << 16) | (c.G << 8) | (c.B);
        }
    }

    public string ToJson()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("{");

        if (col == null)
            sb.Append("\"col\": null");
        else
        {
            sb.Append("\"col\": [");
            for (int i = 0; i < col.Length; i++)
            {
                sb.Append(col[i]);
                if (i < col.Length - 1)
                    sb.Append(",");
            }
            sb.Append("]");
        }
        sb.Append("}");

        return sb.ToString();
    }
}