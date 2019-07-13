using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Slime_Rancher.GSI.Nodes {
    public class GameStateNode : Node<GameStateNode> {

        public bool InMenu;
        public bool loading;
        public bool InGame;
        public bool IsPaused;

        internal GameStateNode(string json) : base(json) {

            GameStateEnum GameState = (GameStateEnum)GetInt("game_state");
            InMenu = GameState == GameStateEnum.Menu;
            loading = GameState == GameStateEnum.Loading;
            InGame = GameState == GameStateEnum.InGame;
            IsPaused = GetBool("pause_menu");
        }

        public enum GameStateEnum
        {
            Menu,
            Loading,
            InGame
        }
    }
}
