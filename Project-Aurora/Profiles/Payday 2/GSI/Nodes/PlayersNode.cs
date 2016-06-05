using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class PlayersNode : Node
    {
        private List<PlayerNode> _Players = new List<PlayerNode>();

        public int Count { get { return _Players.Count; } }
        public PlayerNode LocalPlayer
        {
            get
            {
                foreach (PlayerNode player in _Players)
                {
                    if (player.IsLocalPlayer)
                        return player;
                }

                return new PlayerNode("");
            }
        }

        internal PlayersNode(string JSON) : base(JSON)
        {
            foreach (JToken jt in _ParsedData.Children())
            {
                _Players.Add(new PlayerNode(jt.First.ToString()));
            }
        }

        /// <summary>
        /// Gets the player with index &lt;index&gt;
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PlayerNode this[int index]
        {
            get
            {
                if (index > _Players.Count - 1)
                {
                    return new PlayerNode("");
                }

                return _Players[index];
            }
        }
    }
}
