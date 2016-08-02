import clr

clr.AddReference("Aurora")
clr.AddReference("System.Drawing")

from Aurora import Global
from Aurora.Settings import KeySequence
from Aurora.Devices import DeviceKeys
from Aurora.EffectsEngine import EffectLayer
from System.Drawing import Color
import System

array_device_keys = System.Array[DeviceKeys]

class main():
    ID = "ExamplePyEffect"
    ForegroundColour = Color.Red
    BackgroundColour = Color.Black
    DefaultKeys = KeySequence(array_device_keys((DeviceKeys.TAB, DeviceKeys.Q,  DeviceKeys.W, DeviceKeys.E, DeviceKeys.R, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.U, DeviceKeys.I, DeviceKeys.O, DeviceKeys.P)))
    def UpdateLights(self, settings, state):
        layer = EffectLayer(self.ID)
        layer.PercentEffect(self.ForegroundColour, self.BackgroundColour, settings.Keys, System.DateTime.Now.Second, 60);
        return layer