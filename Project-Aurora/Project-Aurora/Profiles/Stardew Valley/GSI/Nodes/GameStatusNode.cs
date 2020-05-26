using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes {
    public enum GameStatus
    {
        Unknown = -1,
        TitleScreen = 0,
        PlayingGame = 3,
        //LogoScreen = 4,
        Loading = 6,
    }

    public class GameStatusNode : AutoJsonNode<GameStatusNode>
    {
        public GameStatus Status;
        public string CurrentMenu;
        public string CurrentMinigameCutscene;
        public bool IsMultiplayer;
        public bool IsChatOpened;

        internal GameStatusNode(string json) : base(json) { }
    }
}