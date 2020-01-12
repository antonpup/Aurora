using Aurora.Profiles.Minecraft.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify
{

    public class Spotify : Application
    {
        public Spotify() : base(new LightEventConfig
        {
            Name = "Spotify",
            ID = "spotify",
            ProcessNames = new[] { "Spotify.exe" },
            ProfileType = typeof(SpotifyProfile),
            OverviewControlType = typeof(Control_Spotify),
            GameStateType = typeof(GSI.GameState_Spotify),
            Event = new GameEvent_Generic(),
            IconURI = "Resources/spotify.png",
            EnableByDefault = false,
            EnableOverlaysByDefault = true
        })
        { }
    }
}