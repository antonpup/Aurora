using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes {
    public enum GameStateEnum
    {
        Menu,
        Loading,
        InGame
    }

    public class GameStateNode : AutoJsonNode<GameStateNode>
    {
        [AutoJsonPropertyName("game_state")]
        public GameStateEnum State;

        public bool InMenu => State == GameStateEnum.Menu; //legacy
        public bool loading => State == GameStateEnum.Loading; //legacy
        public bool InGame => State == GameStateEnum.InGame; //legacy

        [AutoJsonPropertyName("pause_menu")]
        public bool IsPaused;

        internal GameStateNode(string json) : base(json) { }
    }
}
