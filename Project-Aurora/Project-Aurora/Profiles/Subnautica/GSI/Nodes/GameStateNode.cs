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

            if (GameState == 2)
            {
                InGame = true;
                InMenu = false;
                loading = false;
            }
            else if (GameState == 1)
            {
                InGame = false;
                InMenu = false;
                loading = true;
            }
            else
            {
                InGame = false;
                InMenu = true;
                loading = false;
            }
        }
    }
}
