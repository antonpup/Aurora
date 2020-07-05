using Aurora.EffectsEngine;
using Aurora.Profiles.CSGO.GSI;
using Aurora.Profiles.CSGO.GSI.Nodes;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Profiles.CSGO.Layers
{
    public class CSGOKillIndicatorLayerHandlerProperties : LayerHandlerProperties2Color<CSGOKillIndicatorLayerHandlerProperties>
    {
        public Color? _RegularKillColor { get; set; }

        [JsonIgnore]
        public Color RegularKillColor { get { return Logic._RegularKillColor ?? _RegularKillColor ?? Color.Empty; } }

        public Color? _HeadshotKillColor { get; set; }

        [JsonIgnore]
        public Color HeadshotKillColor { get { return Logic._HeadshotKillColor ?? _HeadshotKillColor ?? Color.Empty; } }

        public CSGOKillIndicatorLayerHandlerProperties() : base() { }

        public CSGOKillIndicatorLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();

            this._Sequence = new KeySequence(new Devices.DeviceKeys[] { Devices.DeviceKeys.G1, Devices.DeviceKeys.G2, Devices.DeviceKeys.G3, Devices.DeviceKeys.G4, Devices.DeviceKeys.G5 });
            this._RegularKillColor = Color.FromArgb(255, 204, 0);
            this._HeadshotKillColor = Color.FromArgb(255, 0, 0);
        }

    }

    public class CSGOKillIndicatorLayerHandler : LayerHandler<CSGOKillIndicatorLayerHandlerProperties>
    {
        enum RoundKillType
        {
            None,
            Regular,
            Headshot
        };

        private List<RoundKillType> roundKills = new List<RoundKillType>();
        private int lastCountedKill = 0;

        protected override UserControl CreateControl()
        {
            return new Control_CSGOKillIndicatorLayer(this);
        }

        public override EffectLayer Render(IGameState state)
        {
            EffectLayer kills_indicator_layer = new EffectLayer("CSGO - Kills Indicator");

            if (state is GameState_CSGO)
            {
                GameState_CSGO csgostate = state as GameState_CSGO;

                if (lastCountedKill != csgostate.Player.State.RoundKills)
                {
                    if (csgostate.Player.State.RoundKills == 0 ||
                        (csgostate.Round.WinTeam == RoundWinTeam.Undefined && csgostate.Previously.Round.WinTeam != RoundWinTeam.Undefined) ||
                        (csgostate.Player.State.Health == 100 && ((csgostate.Previously.Player.State.Health > -1 && csgostate.Previously.Player.State.Health < 100) || (csgostate.Round.WinTeam == RoundWinTeam.Undefined && csgostate.Previously.Round.WinTeam != RoundWinTeam.Undefined)) && csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID))
                    )
                        roundKills.Clear();
                    if (csgostate.Previously.Player.State.RoundKills != -1 && csgostate.Player.State.RoundKills != -1 && csgostate.Previously.Player.State.RoundKills < csgostate.Player.State.RoundKills && csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID))
                    {
                        if (csgostate.Previously.Player.State.RoundKillHS != -1 && csgostate.Player.State.RoundKillHS != -1 && csgostate.Previously.Player.State.RoundKillHS < csgostate.Player.State.RoundKillHS)
                            roundKills.Add(RoundKillType.Headshot);
                        else
                            roundKills.Add(RoundKillType.Regular);
                    }

                    lastCountedKill = csgostate.Player.State.RoundKills;
                }

                if (csgostate.Provider.SteamID.Equals(csgostate.Player.SteamID))
                {
                    for (int pos = 0; pos < Properties.Sequence.keys.Count(); pos++)
                    {
                        if (pos < roundKills.Count)
                        {
                            switch (roundKills[pos])
                            {
                                case (RoundKillType.Regular):
                                    kills_indicator_layer.Set(Properties.Sequence.keys[pos], Properties.RegularKillColor);
                                    break;
                                case (RoundKillType.Headshot):
                                    kills_indicator_layer.Set(Properties.Sequence.keys[pos], Properties.HeadshotKillColor);
                                    break;
                            }
                        }
                    }
                }
            }

            return kills_indicator_layer;
        }

        public override void SetApplication(Application profile)
        {
            (Control as Control_CSGOKillIndicatorLayer).SetProfile(profile);
            base.SetApplication(profile);
        }
    }
}