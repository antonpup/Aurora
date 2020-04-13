using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.RocketLeague.GSI.Nodes
{
    public class Team_RocketLeague : AutoJsonNode<Team_RocketLeague>
    {
        /// <summary>
        /// Name of the team. Usually Blue or Orange, but can be different in custom games and for clan teams
        /// </summary>
        public string Name;

        /// <summary>
        /// Number of goals the team scored
        /// </summary>
        public int Goals;

        /// <summary>
        /// Red value of the teams color (0-1)
        /// </summary>
        public float Red;

        /// <summary>
        /// Green value of the teams color (0-1)
        /// </summary>
        public float Green;

        /// <summary>
        /// Blue value of the teams color (0-1)
        /// </summary>
        public float Blue;

        internal Team_RocketLeague(string JSON) : base(JSON) { }

        public Color TeamColor
        {
            get
            {
                return Color.FromArgb((int)(Red * 255.0f),
                                       (int)(Green * 255.0f),
                                       (int)(Blue * 255.0f));
            }
            set
            {
                Red = value.R / 255.0f;
                Green = value.G / 255.0f;
                Blue = value.B / 255.0f;
            }
        }
    }
}