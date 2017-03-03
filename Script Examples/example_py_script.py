import clr

clr.AddReference("Aurora")
clr.AddReference("System.Drawing")

from Aurora import Global
from Aurora.Settings import KeySequence, IEffectScript, VariableRegistry
from Aurora.Devices import DeviceKeys
from Aurora.EffectsEngine import EffectLayer
from Aurora.Utils import RealColor
from System.Drawing import Color
import System

array_device_keys = System.Array[DeviceKeys]

class ExamplePyEffect(IEffectScript):
    ID = "ExamplePyEffect"
    prop = None
    ForegroundColour = Color.Red
    BackgroundColour = Color.Black
    DefaultKeys = KeySequence(array_device_keys((DeviceKeys.TAB, DeviceKeys.Q,  DeviceKeys.W, DeviceKeys.E, DeviceKeys.R, DeviceKeys.T, DeviceKeys.Y, DeviceKeys.U, DeviceKeys.I, DeviceKeys.O, DeviceKeys.P)))
    def __init__(self):
        self.ForegroundColour = Color.Green
        
    def get_Properties(self):
        if self.prop == None:
            self.prop = VariableRegistry()
            self.prop.Register("keys", KeySequence(self.DefaultKeys), "Main Keys")
            self.prop.Register("foregroundColour", RealColor(Color.Red), "Foreground Colour");
            self.prop.Register("backgroundColour", RealColor(Color.Black), "Background Colour");
        
        return self.prop
        
    def UpdateLights(self, settings, state):
        layer = EffectLayer(self.ID)
        layer.PercentEffect(settings.GetVariable[RealColor]("foregroundColour").GetDrawingColor(), settings.GetVariable[RealColor]("backgroundColour").GetDrawingColor(), settings.GetVariable[KeySequence]("keys"), System.DateTime.Now.Second, 60)
        return layer