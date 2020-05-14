using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// A class representing player information for all players
    /// </summary>
    public class AllPlayersNode : Node
    {
        private readonly List<PlayerNode> _Players = new List<PlayerNode>();
        
        /// <summary>
        /// Retrieves a PlayerNode from all players based on player's name
        /// </summary>
        /// <param name="Name">The name of the player to find</param>
        /// <returns>A PlayerNode pertaining to the specified player</returns>
        public PlayerNode GetByName(string Name)
        {
            PlayerNode pn = _Players.Find(x => x.Name == Name);
            if (pn != null)
                return pn;
            
            return new PlayerNode("");
        }

        /// <summary>
        /// Retrieves a PlayerNode from all players based on player's steam ID
        /// </summary>
        /// <param name="SteamID">The steam ID of a player to find</param>
        /// <returns>A PlayerNode pertaining to the specified player</returns>
        public PlayerNode GetBySteamID(string SteamID)
        {
            PlayerNode pn = _Players.Find(x => x.SteamID == SteamID);
            if (pn != null)
                return pn;

            return new PlayerNode("");
        }

        /// <summary>
        /// The number of players
        /// </summary>
        public int Count { get { return _Players.Count; } }

        internal AllPlayersNode(string JSON)
            : base(JSON)
        {
            foreach (JToken jt in _ParsedData.Children())
            {
                PlayerNode pn = new PlayerNode(jt.First.ToString());
                pn._SteamID = jt.Value<JProperty>()?.Name ?? "";
                _Players.Add(pn);
            }
        }

        /// <summary>
        /// Gets the player at a specified index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>A PlayerNode pertaining to the specified index</returns>
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

        /// <summary>
        /// Retrieves the enumerator for all players list
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<PlayerNode> GetEnumerator()
        {
            return _Players.GetEnumerator();
        }

        /// <summary>
        /// Retrieves a list of players who are on a specified team
        /// </summary>
        /// <param name="Team">The team to lookup</param>
        /// <returns>A list of players</returns>
        //[Range(0, 16)]
        public List<PlayerNode> GetTeam(PlayerTeam Team)
        {
            return _Players.FindAll(x => x.Team == Team);
        }
    }
}
