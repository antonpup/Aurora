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
        public bool InMenu;
        public bool loading;
        public bool Playing;
        public bool Paused;

        public bool InGame;

        internal GameStateNode(string json) : base(json) {

            GameState = GetInt("game_state");
            InMenu = GameState == 0;
            loading = GameState == 1;
            Playing = GameState == 2;
            Paused = GameState == 3;

            InGame = Playing || Paused;
        }
    }
}
