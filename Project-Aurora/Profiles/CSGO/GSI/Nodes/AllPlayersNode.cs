using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class AllPlayersNode : Node
    {
        private readonly List<PlayerNode> _Players = new List<PlayerNode>();
        
        public PlayerNode GetByName(string Name)
        {
            PlayerNode pn = _Players.Find(x => x.Name == Name);
            if (pn != null)
                return pn;
            
            return new PlayerNode("");
        }
        
        public PlayerNode GetBySteamID(string SteamID)
        {
            PlayerNode pn = _Players.Find(x => x.SteamID == SteamID);
            if (pn != null)
                return pn;

            return new PlayerNode("");
        }

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

        public IEnumerator<PlayerNode> GetEnumerator()
        {
            return _Players.GetEnumerator();
        }

        public List<PlayerNode> GetTeam(PlayerTeam Team)
        {
            return _Players.FindAll(x => x.Team == Team);
        }
    }
}
