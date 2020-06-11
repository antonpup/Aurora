using Aurora.EffectsEngine;
using Aurora.EffectsEngine.Animations;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace Aurora.Profiles.Desktop
{
    public class Event_Desktop : LightEvent
    {
        private long internalcounter;

        public Event_Desktop() : base()
        {
            internalcounter = 0;
        }

        public override void UpdateLights(EffectFrame frame)
        {
            var layers = new Queue<EffectLayer>(Application.Profile.Layers.Where(l => l.Enabled).Reverse().Select(l => l.Render(_game_state)));
            
            //Scripts before interactive and shortcut assistant layers
            //ProfilesManager.DesktopProfile.UpdateEffectScripts(layers);

            if (Global.Configuration.time_based_dimming_enabled)
            {
                if (Utils.Time.IsCurrentTimeBetween(Global.Configuration.time_based_dimming_start_hour, Global.Configuration.time_based_dimming_end_hour)) {
                    layers.Clear();

                    EffectLayer time_based_dim_layer = new EffectLayer("Time Based Dim");
                    time_based_dim_layer.Fill(Color.Black);

                    layers.Enqueue(time_based_dim_layer);
                }
            }

            frame.AddLayers(layers.ToArray());
        }

        public override void SetGameState(IGameState new_game_state)
        {
            
        }

        public new bool IsEnabled
        {
            get { return true; }
        }
    }
}
