import time
import json
import sys, os
import cgi

from neopixel import *
from BaseHTTPServer import BaseHTTPRequestHandler, HTTPServer
from threading import Thread
import subprocess

#!!!!!!!!!! SETTINGS !!!!!!!!!!
NUMBER_OF_LEDS = 120 #Number of LEDs connected to your Raspberry pi
USED_PORT = 8032 #Port to be used
PI_URL_SUFFIX_START = "start"
PI_URL_SUFFIX_STOP = "stop"
PI_URL_SUFFIX_QUIT = "quit"
PI_URL_SUFFIX_SETLIGHTS = "lights"

# (Neopixel Library config) LED strip configuration:
LED_PIN	= 18  # GPIO pin connected to the pixels (must support PWM!).
LED_FREQ_HZ	= 800000  # LED signal frequency in hertz (usually 800khz)
LED_DMA	= 5	  # DMA channel to use for generating signal (try 5)
LED_BRIGHTNESS = 128  # Set to 0 for darkest and 255 for brightest
LED_INVERT = False  # True to invert the signal (when using NPN transistor level shift)
LED_CHANNEL    = 0
LED_STRIP      = ws.SK6812_STRIP_RGBW	
#LED_STRIP      = ws.SK6812W_STRIP

strip = None
isInitialized = False


def allOff(strip):
	"""Wipe color across display a pixel at a time."""
	for i in range(strip.numPixels()):
		strip.setPixelColor(i, Color(0, 0, 0))
	
def handle_incoming_lighting(json_string):
	json_data = json.loads(json_string)
	
	for (led_index, color) in enumerate(json_data['col']):
		if led_index < NUMBER_OF_LEDS:
			led_col = int(color)
			R = led_col >> 16
			G = (led_col >> 8) & 255
			B = led_col & 255
			
			strip.setPixelColor(led_index, Color(G, R, B))
	strip.show()
	isInitialized = False

	
				
#Create custom HTTPRequestHandler class
class AuroraNPLHTTPRequestHandler(BaseHTTPRequestHandler):
	
	#handle POST command
	def do_POST(self):
		try:
			response = bytes("") #create response
			if self.path.endswith(PI_URL_SUFFIX_START):
				self.send_response(200) #create header
				self.send_header("Content-Length", str(len(response)))
				self.end_headers()
				self.wfile.write(response) #send response
				
				isInitialized = True
				return
			elif self.path.endswith(PI_URL_SUFFIX_STOP):
				self.send_response(200) #create header
				self.send_header("Content-Length", str(len(response)))
				self.end_headers()
				self.wfile.write(response) #send response
				
				allOff(strip)
				strip.show()
				isInitialized = False
				return
			elif self.path.endswith(PI_URL_SUFFIX_QUIT):
				self.send_response(200) #create header
				self.send_header("Content-Length", str(len(response)))
				self.end_headers()
				self.wfile.write(response) #send response
				
				allOff(strip)
				strip.show()
				isInitialized = False
				
				sys.exit(1)
				#os.exit(1)
				return
			elif self.path.endswith(PI_URL_SUFFIX_SETLIGHTS):
				self.send_response(200) #create header
				self.send_header("Content-Length", str(len(response)))
				self.end_headers()
				self.wfile.write(response) #send response
				
				ctype, pdict = cgi.parse_header(self.headers.getheader('content-type'))
				if ctype == 'application/json':
					content_len = int(self.headers.getheader('content-length'))
					post_body = self.rfile.read(content_len)
					
					t1 = Thread(target = handle_incoming_lighting, args = (post_body,))
					t1.start()
				return
			
		except IOError:
			self.send_error(404, 'file not found')
			
	def log_message(self, format, *args):
		return
	
				
# Main program logic follows:
if __name__ == '__main__':
	# Create NeoPixel object with appropriate configuration.
	strip = Adafruit_NeoPixel(NUMBER_OF_LEDS, LED_PIN, LED_FREQ_HZ, LED_DMA, LED_INVERT, LED_BRIGHTNESS, LED_CHANNEL, LED_STRIP)
	# Intialize the library (must be called once before other functions).
	strip.begin()
	
	print ('Strip Initialized')
	
	allOff(strip)
	strip.show()
	
	#Set up HTTP Handlers
	
	httpd = HTTPServer(("", USED_PORT), AuroraNPLHTTPRequestHandler)
	print('http server is running...')
	httpd.serve_forever()