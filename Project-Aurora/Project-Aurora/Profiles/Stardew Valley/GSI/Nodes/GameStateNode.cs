using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes {
    public enum GameState
    {
        Unknown = -1,
        TitleMenu,
        Loading,
        InGame
    }

    public class GameStateNode : AutoJsonNode<GameStateNode>
    {
        public GameState GameState;
        public string CurrMenu;
        public string CurrMinigameCutscene;
        public byte GameMode;
        public bool HasLoadedGame;
        public bool IsMultiplayer;
        public bool ChatOpened;

        internal GameStateNode(string json) : base(json) { }
    }
}