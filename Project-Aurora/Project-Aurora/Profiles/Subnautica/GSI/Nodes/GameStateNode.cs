using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public class GameStateNode : Node<GameStateNode> {

        public int GameState;
        /*
        0 = Menu
        1 = Loading
        2 = Playing
        3 = Paused
        */

        public bool InMenu;
        public bool loading;
        public bool InGame;
        public bool Paused;

        internal GameStateNode(string json) : base(json) {

            GameState = GetInt("game_state");
            InMenu = GameState == 0;
            loading = GameState == 1;
            InGame = GameState == 2 || GameState == 3;
            Paused = GameState == 3;
        }
    }
}
