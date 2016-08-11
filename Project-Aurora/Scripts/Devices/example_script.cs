using Aurora;
using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class ExampleDeviceScript
{
    public string devicename = "Example CS Device Script";
    public bool enabled = false; //Switch to True, to enable it in Aurora
    
    private Color device_color = Color.Black;

    public bool Initialize()
    {
        try
        {
            //Perform necessary actions to initialize your device
            return true;
        }
        catch(Exception exc)
        {
            return false;
        }
    }
    
    public void Reset()
    {
        //Perform necessary actions to reset your device
    }
    
    public void Shutdown()
    {
        //Perform necessary actions to shutdown your device
    }
    
    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
    {
        try
        {
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                //Iterate over each key and color and send them to your device
                
                if(key.Key == DeviceKeys.Peripheral)
                {
                    //For example if we're basing our device color on Peripheral colors
                    SendColorToDevice(key.Value, forced);
                }
            }
            
            return true;
        }
        catch(Exception exc)
        {
            return false;
        }
    }
    
    //Custom method to send the color to the device
    private void SendColorToDevice(Color color, bool forced)
    {
        //Check if device's current color is the same, no need to update if they are the same
        if (!device_color.Equals(color) || forced)
        {
            //NOTE: Do not have any logging during color set for performance reasons. Only use logging for debugging
            Global.logger.LogLine(string.Format("[C# Script] Sent a color, {0} to the device", color));
            
            //Update device color locally
            device_color = color;
        }
    }
}