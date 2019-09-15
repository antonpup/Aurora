using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.RocketLeague.GSI.Nodes
{
    public class Team_RocketLeague : Node<Team_RocketLeague>
    {
        public string Name;

        public int Goals;

        public float Red;

        public float Green;

        public float Blue;
        internal Team_RocketLeague(string JSON) : base(JSON)
        {
            Name = GetString("name");
            Goals = GetInt("goals");
            Red = GetFloat("red");
            Green = GetFloat("green");
            Blue = GetFloat("blue");
        }
    }
}