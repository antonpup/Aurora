using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    /// <summary>
    /// Information about players in the lobby
    /// </summary>
    public class PlayersNode : Node, IEnumerable<PlayerNode>
    {
        private List<PlayerNode> _Players = new List<PlayerNode>();
        private PlayerNode _LocalPlayer = new PlayerNode("");

        /// <summary>
        /// Amount of players in the lobby
        /// </summary>
        public int Count { get { return _Players.Count; } }


        /// <summary>
        /// The local player
        /// </summary>
        public PlayerNode LocalPlayer
        {
            get
            {
                foreach (PlayerNode player in _Players)
                {
                    if (player.IsLocalPlayer)
                        _LocalPlayer = player;
                }

                return _LocalPlayer;
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
        /// Gets the player at a selected index
        /// </summary>
        /// <param name="index">The index</param>
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

        public IEnumerator<PlayerNode> GetEnumerator()
        {
            return _Players.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Players.GetEnumerator();
        }
    }
}
