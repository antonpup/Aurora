using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public class PlayerNode : Node<PlayerNode> {

        public string Biom;
        public bool InLifePod;

        //public string type; //Base, Cyclops, Seamoth or Prawn
        public string Type;
        public bool InBase;
        public bool InCyclops;
        public bool InSeamoth;
        public bool InPrawn;

        //public int Depth;
        //public int SurfaceDepth; //always 0?
        public int DepthLevel;

        public int Health;
        public int Food;
        public int Water;

        public bool CanBreathe;
        public int OxygenCapacity;
        public int OxygenAvailable;

        public int PDAState;
        /*
        Opened = 0
        Closed = 1
        Opening = 2
        Closing = 3
        */
        public bool PDAopened;
        public bool PDAclosed;
        public bool PDAopening;
        public bool PDAclosing;

        public bool IsSwimming; //Seagliding does also count :)

        public int MotorMode;
        /*
        Walk = 0
        Dive = 1
        Seaglide = 2
        Vehicle = 3
        Mech = 4
        Run = 5
        */
        public bool IsSeagliding;

        public int Mode;
        /*
        Normal = 0
        Piloting = 1
        LockedPiloting = 2
        Sitting = 3
        */
        public bool IsPiloting;

        internal PlayerNode(string json) : base(json) {
            Biom = GetString("biom");

            InLifePod = Biom == "Lifepod";

            //Base, Cyclops, Seamoth or Prawn
            Type = GetString("type");

            InBase = Type == "Base";
            InCyclops = Type == "Cyclops";
            InSeamoth = Type == "Seamoth";
            InPrawn = Type == "Prawn";

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

            PDAState = GetInt("pda_state");

            PDAopened = PDAState == 0;
            PDAclosed = PDAState == 1;
            PDAopening = PDAState == 2;
            PDAclosing = PDAState == 3;

            IsSwimming = GetBool("is_in_water_for_swimming");

            MotorMode = GetInt("motor_mode");

            IsSeagliding = MotorMode == 2;

            Mode = GetInt("mode");
            IsPiloting = Mode == 1 || Mode == 2; // Mode 1 = Piloting/Mode 2 = locked Piloting
        }

    }
}
