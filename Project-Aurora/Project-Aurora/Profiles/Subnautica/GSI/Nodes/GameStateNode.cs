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
        */
        public bool InGame;
        public bool InMenu;
        public bool loading;

        internal GameStateNode(string json) : base(json) {

            GameState = GetInt("game_state");
            InGame = GameState == 2;
            InMenu = GameState == 0;
            loading = GameState == 1;
        }
    }
}
