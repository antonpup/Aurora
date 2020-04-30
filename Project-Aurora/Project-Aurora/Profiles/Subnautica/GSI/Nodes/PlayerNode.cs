using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public enum PDAState
    {
        Opened = 0,
        Closed = 1,
        Opening = 2,
        Closing = 3
    }

    public enum MotorMode
    {
        Walk = 0,
        Dive = 1,
        Seaglide = 2,
        Vehicle = 3,
        Mech = 4,
        Run = 5
    }

    public enum Modes
    {
        Normal = 0,
        Piloting = 1,
        LockedPiloting = 2,
        Sitting = 3
    }

    public enum GameModes
    {
        Survival = 0,
        Freedom = 1,
        Hardcore = 2,
        Creative = 3,
        None = 4
    }

    public class PlayerNode : Node<PlayerNode> {

        public string Biom;
        public bool InLifePod;

        //public int Depth;
        //public int SurfaceDepth; //always 0?
        public int DepthLevel;

        public int Health;
        public int Food;
        public int Water;

        public bool CanBreathe;
        public int OxygenCapacity;
        public int OxygenAvailable;

        public bool IsSwimming; //Seagliding does also count :)

        public PDAState PDAState;
        public MotorMode MotorMode;
        public Modes Mode;
        public GameModes GameMode;

        internal PlayerNode(string json) : base(json)
        {
            Biom = GetString("biom");

            InLifePod = Biom == "Lifepod";

            //SurfaceDepth = GetInt("surface_depth");
            DepthLevel = GetInt("depth_level");

            Health = GetInt("health");
            Food = GetInt("food");
            Water = GetInt("water");

            CanBreathe = GetBool("can_breathe");

            if (GetInt("oxygen_capacity") >= 1)
            {
                OxygenCapacity = GetInt("oxygen_capacity");
            }
            else
                OxygenCapacity = 45;

            OxygenAvailable = GetInt("oxygen_available");

            IsSwimming = GetBool("is_in_water_for_swimming");

            PDAState = (PDAState)GetInt("pda_state");
            MotorMode = (MotorMode)GetInt("motor_mode");
            Mode = (Modes)GetInt("mode");
            GameMode = (GameModes)GetInt("game_mode");
        }

    }
}
