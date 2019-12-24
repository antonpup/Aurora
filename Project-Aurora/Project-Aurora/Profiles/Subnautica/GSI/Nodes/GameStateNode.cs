using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public enum GameState
    {
        Menu = 0,
        Loading = 1,
        Playing = 2,
        Paused = 3
    }

    public class GameStateNode : Node<GameStateNode> {

        public GameState State;

        internal GameStateNode(string json) : base(json) {
            State = (GameState)GetInt("game_state");
        }
    }
}
