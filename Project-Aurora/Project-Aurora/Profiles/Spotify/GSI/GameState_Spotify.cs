using Aurora.Profiles.Generic.GSI.Nodes;
using Aurora.Profiles.Spotify.GSI.Nodes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Spotify.GSI
{
    public class GameState_Spotify : GameState
    {
        public ProviderNode Provider => NodeFor<ProviderNode>("provider");

        public PlayerNode Player => NodeFor<PlayerNode>("player");

        public ColorsNode Colors => NodeFor<ColorsNode>("colors");

        public TrackNode Track => NodeFor<TrackNode>("track");

        public GameState_Spotify() : base() { }

        public GameState_Spotify(string JSONstring) : base(JSONstring) { }
    }
}