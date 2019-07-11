using Aurora.Devices;
using Aurora.Devices.Razer;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using RazerSdkWrapper;
using RazerSdkWrapper.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class RazerLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        private static RzManager _manager;
        private static int _instances;

        private Color[] _keyboardColors;
        private Color[] _mousepadColors;
        private Color _mouseColor;


        public RazerLayerHandler()
        {
            _ID = "Razer";
            _keyboardColors = new Color[22 * 6];
            _mousepadColors = new Color[16];

            _instances++;
            if (_instances > 0 && _manager == null)
            {
                _manager = new RzManager()
                {
                    KeyboardEnabled = true,
                    MouseEnabled = true,
                    MousepadEnabled = true,
                };
            }

            if(_manager != null)
                _manager.DataUpdated += OnDataUpdated;
        }

        protected override UserControl CreateControl()
        {
            return new Control_RazerLayer(this);
        }

        private void OnDataUpdated(object s, EventArgs e)
        {
            if (!(s is AbstractDataProvider provider))
                return;

            provider.Update();
            if (provider is RzKeyboardDataProvider keyboard)
            {
                for (var i = 0; i < keyboard.ZoneGrid.Height * keyboard.ZoneGrid.Width; i++)
                    _keyboardColors[i] = keyboard.GetZoneColor(i);
            }
            else if (provider is RzMouseDataProvider mouse)
            {
                _mouseColor = mouse.GetZoneColor(55);
            }
            else if (provider is RzMousepadDataProvider mousePad)
            {
                for (var i = 0; i < mousePad.ZoneGrid.Height * mousePad.ZoneGrid.Width; i++)
                    _mousepadColors[i] = mousePad.GetZoneColor(i);
            }
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            var layer = new EffectLayer();

            foreach (var key in (DeviceKeys[])Enum.GetValues(typeof(DeviceKeys)))
            {
                if( RazerLayoutMap.GenericKeyboard.TryGetValue(key, out var position)){
                    var index = position[1] + position[0] * 22;
                    layer.Set(key, _keyboardColors[index]);
                }
            }

            return layer;
        }

        public override void Dispose()
        {
            _instances--;
            if (_manager != null)
            {
                _manager.DataUpdated -= OnDataUpdated;
                if (_instances == 0)
                {
                    _manager.Dispose();
                    _manager = null;
                }

                if (_instances < 0)
                    _instances = 0;
            }

            base.Dispose();
        }
    }
}
