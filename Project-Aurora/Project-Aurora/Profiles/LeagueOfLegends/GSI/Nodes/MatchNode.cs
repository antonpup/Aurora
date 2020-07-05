using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public class MatchNode : Node
    {
        public MapTerrain MapTerrain;

        //public GameMode GameMode;
        //TODO: Find the rest of the enum values. meanwhile i'll leave this as a string
        public string GameMode = "";

        public float GameTime;

        public bool InGame;

        public int InfernalDragonsKilled;

        public int OceanDragonsKilled;

        public int MountainDragonsKilled;

        public int CloudDragonsKilled;

        public int EarthDragonsKilled;

        public int ElderDragonsKilled;

        public int DragonsKilled;

        public int TurretsKilled;

        public int InhibsKilled;

        public int BaronsKilled;

        public int HeraldsKilled;
    }

    public enum MapTerrain
    {
        Unknown,
        Default,
        Infernal,
        Cloud,
        Mountain,
        Ocean
    }

    //TODO: Find the rest of these
    public enum GameMode 
    {     
        Unknown = -1,
        None = 0,
        PracticeTool
    }
}
