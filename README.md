# Aurora Raspberry Pi LED Script

This fork includes a script for Raspberry PI and Aurora to transmit lighting information to LED lights connected to the Raspberry PI.

# Requirements
* [Aurora](https://github.com/antonpup/Aurora)
* A Raspberry Pi
* LED strips connected to the Raspberry Pi
* Pi connected to your home network
* Fast network connection

# How to Install & Run
1. Make sure your Raspberry Pi is configured to work with Java. Has JRE and JDK.
   - It is also recommended to configure your Pi to have a static IP address.
2. Connect your LED Strips to the Raspberry Pi ( http://learn.adafruit.com/assets/1589 )
3. Install following dependencies to work on your Raspberry Pi:
 - Maven: http://maven.apache.org/
 - Pi4j
	- Raspian: http://pi4j.com/
	- Arch: https://github.com/glnds/pi4j-arch
4. Copy directory "AuroraPiLighting" to your Raspberry Pi
5. Open "src\main\java\aurora\lights\AuroraPiLighting.java" and change these lines according to your configuration:
``` Java
private final static int NUMBER_OF_LEDS = 32; //Number of LEDs connected to your Raspberry pi
private final static int USED_PORT = 8032; //Port to be used
```
5. Build the java project on your Pi by going into "AuroraPiLighting" directory and running "mvn clean install".
6. Afterwards go into newly created "target" directory and run "java -jar AuroraPiLighting-full.jar". Raspberry Pi portion is now complete.

7. On your PC, copy file "Scripts/Devices/rpi_script.cs" to "*Aurora install location*/Scripts/Devices/rpi_script.cs"
8. Open the "rpi_script.cs" file to configure the script. Adjust following lines:
``` C#
//!!!!!!!!!! SCRIPT SETTINGS !!!!!!!!!!//
public bool enabled = true; //Switch to True, to enable it in Aurora
private static readonly int NUMBER_OF_LEDS = 32; //Number of LEDs connected to your Raspberry pi
private static readonly string PI_URL = "http://10.0.0.150:8032/"; //The URL of your Pi (to which requests will be sent to)
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
```

9. Run Aurora and lighting should now work.

# Credit
* [LedStrip](https://github.com/glnds/LedStrip) - Used for LED lighting
* [Gson](https://github.com/google/gson) - Used for Json parsing