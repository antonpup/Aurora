
#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <ArduinoJson.h>
#define FASTLED_ESP8266_RAW_PIN_ORDER

#include "FastLED.h"

#if FASTLED_VERSION < 3001000
#error "Requires FastLED 3.1 or later"
#endif

// !!!!!!!!!! SETTINGS !!!!!!!!!!
#define LED_PIN D2				// Digital pin connected to the LED strip
#define NUM_LEDS 150			// Number of LEDs connected to your device
#define MAX_LED_BRIGHTNESS 128	// Maximum brightness of the LEDs (0-255)
#define USED_PORT 8032			// Port to be used

//!!!!!!!!!!! IP SETTIGNS !!!!!!!!!!
#define USE_STATIC_IP 1		// If you don't want to use a static IP, comment out this line

// Set static IP parameters
IPAddress ip(192, 168, 2, 6);
IPAddress gateway(192, 168, 2, 1);
IPAddress subnet(255, 255, 255, 0);
IPAddress dns(192, 168, 2, 1);

//Default settings for WS2812B LED-Strip
#define COLOR_ORDER GRB
#define LED_TYPE WS2812B
#define UPDATES_PER_SECOND 1000

#define PI_URL_SUFFIX_START "/start"
#define PI_URL_SUFFIX_STOP "/stop"
#define PI_URL_SUFFIX_QUIT "/quit"
#define PI_URL_SUFFIX_SETLIGHTS "/lights"

// WiFi Parameters
const char* ssid = "XXXXXXXXXXXSSIDXXXXXXXXXXXXXXX";
const char* password = "XXXXXXXPASSWDXXXXXXXX";

//Define a webserver on defined port
ESP8266WebServer server(USED_PORT);
//Declare pointer to JSON  memory buffer
DynamicJsonBuffer * jsonBuffer;
//Define a struct of LED color values
struct CRGB leds[NUM_LEDS];

bool isStarted = false;

void setup()
{
	//Initialize serial console
	Serial.begin(115200);
#if USE_STATIC_IP == 1
	Serial.println("Using static IP");
	WiFi.config(ip, dns, gateway, subnet);
#endif // 

	//Initialize wifi connection
	//WiFi.mode(WIFI_STA);
	WiFi.begin(ssid, password);
	while (WiFi.waitForConnectResult() != WL_CONNECTED) {
		Serial.println("Connection Failed! Rebooting...");
		delay(5000);
		ESP.restart();
	}

	Serial.println("Ready");
	Serial.print("IP address: ");
	Serial.println(WiFi.localIP());

	//Define REST callback methods
	server.on(PI_URL_SUFFIX_START, HTTP_POST, OnStart);
	server.on(PI_URL_SUFFIX_STOP,  HTTP_POST, OnStop);
	server.on(PI_URL_SUFFIX_QUIT,  HTTP_POST, OnQuit);
	server.on(PI_URL_SUFFIX_SETLIGHTS, HTTP_POST, OnSetLights);

	//Start webserver
	server.begin();

	//Initializing LEDs
	Serial.println("Initializing LEDs");
	FastLED.addLeds<LED_TYPE, LED_PIN, COLOR_ORDER>(leds, NUM_LEDS).setCorrection(TypicalLEDStrip);
	Serial.print("Setting brightness to: ");
	Serial.println(MAX_LED_BRIGHTNESS);
	FastLED.setBrightness(MAX_LED_BRIGHTNESS);

	//Calculate the necessary size for the json object in memeory
	const size_t capacity = JSON_ARRAY_SIZE(NUM_LEDS) + JSON_OBJECT_SIZE(1) + 1010;
	//Allocate memory on heap for json objects
	jsonBuffer = new DynamicJsonBuffer(capacity);

}
void OnStart()
{
	Serial.println("Starting");
	server.send(200, "text/plain", "Starting");
	isStarted = true;
}
void OnStop()
{
	Serial.println("Stopping");
	server.send(200, "text/plain", "Stopping");

	isStarted = false;
}
void OnQuit()
{
	Serial.println("Quitting");
	server.send(200, "text/plain", "Quitting");

	isStarted = false;
}
void OnSetLights()
{
	if (server.hasArg("plain") == false)
	{
		server.send(200, "text/plain", "Body not received");
		return;
	}
	server.send(200, "text/plain", "Body received");
	String message = server.arg("plain");
	
	JsonObject& root = jsonBuffer->parseObject(message);
	
	if (!root.success())
	{
		Serial.println("Parsing failed");
		return;
	}
	//Serial.println("Parsing succeeded");
	JsonArray& col = root["col"];
	//Serial.print("Number of elements : ");
	for (uint32_t i = 0; i < NUM_LEDS; ++i)
	{
		uint8_t r = (uint32_t)col[i] >> 16 & 0xFF;
		uint8_t g = (uint32_t)col[i] >> 8 & 0xFF;
		uint8_t b = (uint32_t)col[i] & 0xFF;

		leds[i].r = r;
		leds[i].g = g;
		leds[i].b = b;
	}
	jsonBuffer->clear();
	
}

void loop()
{
	server.handleClient();
	if (isStarted)
	{
		FastLED.show();
	}
	else
	{
		FastLED.clear();
	}
	FastLED.delay(1000 / UPDATES_PER_SECOND);

}
