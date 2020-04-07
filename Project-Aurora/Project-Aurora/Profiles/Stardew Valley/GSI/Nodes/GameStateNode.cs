using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes {
    public enum GameStates
    {
        Unknown = -1,
        TitleMenu,
        Loading,
        InGame
    }

    public enum GameModes
    {
        Unknown = -1,
        TitleScreen,
        LoadScreen,
        NewGame,
        PlayingGame,
        LogoScreen,
        Loading = 6,
        Save,
        SaveComplete,
        SelectGame,
        Credits,
        ErrorLog
    }

    public class GameStateNode : AutoJsonNode<GameStateNode>
    {
        public GameStates GameState;
        public string CurrMenu;
        public string CurrMinigameCutscene;
        public GameModes GameMode;
        public bool HasLoadedGame;
        public bool IsMultiplayer;
        public bool ChatOpened;

        internal GameStateNode(string json) : base(json) { }
    }
}