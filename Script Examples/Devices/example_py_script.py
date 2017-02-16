import clr

clr.AddReference("Aurora")
clr.AddReference("System.Drawing")

from Aurora import Global
from Aurora.Devices import DeviceKeys
from System.Drawing import Color
from System.Collections.Generic import Dictionary
import System

import sys

class main():
    devicename = "Example Python Device Script"
    enabled = False #Switch to True, to enable it in Aurora
    device_color = Color.Black
    
    def Initialize(self):
        try:
            #Perform necessary actions to initialize your device
            return True
        except:
            return False
    
    def Reset(self):
        #Perform necessary actions to reset your device
        return
        
    def Shutdown(self):
        #Perform necessary actions to shutdown your device
        return
    
    def UpdateDevice(self, keyColors, forced):
        try:
            for kvp in keyColors:
                #Iterate over each key and color and send them to your device
                
                if kvp.Key == DeviceKeys.Peripheral:
                    #For example if we're basing our device color on Peripheral colors
                    self.SendColorToDevice(kvp.Value, forced)
                    
            return True
        except Exception as e:
            Global.logger.LogLine("[PY Script] Exception: "+str(e))
            
            return False
    
    def CompareColors(self, color1, color2):
        if color1.A == color2.A and color1.R == color2.R and color1.G == color2.G and color1.B == color2.B:
            return True
        else:
            return False
    
    #Custom method to send the color to the device
    def SendColorToDevice(self, color, forced):
        #Check if device's current color is the same, no need to update if they are the same
        if not self.CompareColors(color, self.device_color) or forced:
            #NOTE: Do not have any logging during color set for performance reasons. Only use logging for debugging
            Global.logger.LogLine("[PY Script] Sent a color, " + str(color) + " to the device")
            
            #Update device color locally
            self.device_color = color
        return
