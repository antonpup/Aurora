# Aurora Raspberry Pi LED Script

This fork includes a script for Raspberry PI and Aurora to transmit lighting information to LED lights connected to the Raspberry PI.

# Requirements
* [Aurora](https://github.com/antonpup/Aurora)
* A Raspberry Pi
* LED strips connected to the Raspberry Pi
* Pi connected to your home network
* Fast local network connection

## Video demonstration
 
[![](http://img.youtube.com/vi/p0WiaQwmSYQ/0.jpg)](https://www.youtube.com/watch?v=p0WiaQwmSYQ)

# How to Install & Run on a NeoPixel LED Strip
1. Make sure your Raspberry Pi is configured to work with Python. Run the following commands:
    ```
	
    sudo apt-get update
    sudo apt-get install build-essential python-dev git scons swig
	
    ```
    - It is also recommended to configure your Pi to have a static IP address.
2. Connect your NeoPixel LED Strips to the Raspberry Pi ( https://learn.adafruit.com/neopixels-on-raspberry-pi/wiring )
3. Compile & Install rpi_ws281x Library on your Raspberry Pi:
    Run the following commands to clone the rpi_ws281x repo and build it.
    ```
	
    git clone https://github.com/jgarff/rpi_ws281x.git
    cd rpi_ws281x
    scons
	
    ```
4. Verify that your NeoPixels work by going into python directory and running one of the example scripts.
    ```
	
    cd python
    sudo python setup.py install
	
    ```
5. Copy file "aurora_neopixel_pi.py" to your Raspberry Pi
6. Open it and change these lines according to your configuration:
    ``` Python
    
    NUMBER_OF_LEDS = 120 #Number of LEDs connected to your Raspberry pi
    USED_PORT = 8032 #Port to be used
    
	# (Neopixel Library config) LED strip configuration:
	LED_PIN	= 18  # GPIO pin connected to the pixels (must support PWM!).
	LED_FREQ_HZ	= 800000  # LED signal frequency in hertz (usually 800khz)
	LED_DMA	= 5	  # DMA channel to use for generating signal (try 5)
	LED_BRIGHTNESS = 128  # Set to 0 for darkest and 255 for brightest
	LED_INVERT = False  # True to invert the signal (when using NPN transistor level shift)
	LED_CHANNEL    = 0
	LED_STRIP      = ws.SK6812_STRIP_RGBW	
	#LED_STRIP      = ws.SK6812W_STRIP
	
    ```
7. Run "sudo python aurora_neopixel_pi.py". Raspberry Pi portion is now complete.

8. On your PC, copy file "Scripts/Devices/rpi_script.cs" to "*Aurora install location*/Scripts/Devices/rpi_script.cs"
9. Open the "rpi_script.cs" file to configure the script. Adjust following lines:
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
10. Run Aurora and lighting should now work.


# How to Install & Run on a LPD8806 LED Strip
1. Make sure your Raspberry Pi is configured to work with Java. Has JRE and JDK.
   - It is also recommended to configure your Pi to have a static IP address.
2. Connect your LED Strips to the Raspberry Pi ( http://learn.adafruit.com/assets/1589 )
3. Install following dependencies to work on your Raspberry Pi:
 - Maven: http://maven.apache.org/
 - Pi4j
	- Raspian: http://pi4j.com/
	- Arch: https://github.com/glnds/pi4j-arch
4. Copy directory "AuroraPiLighting" to your Raspberry Pi
5. Open "src/main/java/aurora/lights/AuroraPiLighting.java" and change these lines according to your configuration:
    ``` Java
    
    private final static int NUMBER_OF_LEDS = 32; //Number of LEDs connected to your Raspberry pi
    private final static int USED_PORT = 8032; //Port to be used
    
    ```
6. Build the java project on your Pi by going into "AuroraPiLighting" directory and running "mvn clean install".
7. Afterwards go into newly created "target" directory and run "java -jar AuroraPiLighting-full.jar". Raspberry Pi portion is now complete.

8. On your PC, copy file "Scripts/Devices/rpi_script.cs" to "*Aurora install location*/Scripts/Devices/rpi_script.cs"
9. Open the "rpi_script.cs" file to configure the script. Adjust following lines:
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
10. Run Aurora and lighting should now work.

# Credit
* [rpi_ws281x](https://github.com/jgarff/rpi_ws281x) - Used for NeoPixel LED lighting
* [LedStrip](https://github.com/glnds/LedStrip) - Used for LED lighting
* [Gson](https://github.com/google/gson) - Used for Json parsing
* [FastLED](https://github.com/FastLED/FastLED) - Used for LED lighting on ESP8266
* [ArduinoJson](https://github.com/bblanchon/ArduinoJson) - Used for Json parsing on ESP8266
* [Arduino](https://github.com/esp8266/Arduino) - Arduino core for ESP8266 WiFi chip
