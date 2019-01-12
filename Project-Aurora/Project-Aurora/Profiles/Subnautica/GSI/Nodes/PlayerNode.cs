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
            if (Biom == "Lifepod")
                InLifePod = true;
            else
                InLifePod = false;

            //Base, Cyclops, Seamoth or Prawn
            Type = GetString("type");
            switch (Type) 
            { 
                case "Base":
                    InBase = true;
                    InCyclops = false;
                    InSeamoth = false;
                    InPrawn = false;
                    break;
                case "Cyclops":
                    InBase = false;
                    InCyclops = true;
                    InSeamoth = false;
                    InPrawn = false;
                    break;
                case "Seamoth":
                    InBase = false;
                    InCyclops = false;
                    InSeamoth = true;
                    InPrawn = false;
                    break;
                case "Prawn":
                    InBase = false;
                    InCyclops = false;
                    InSeamoth = false;
                    InPrawn = true;
                    break;
                default: //In Menu
                    InBase = false;
                    InCyclops = false;
                    InSeamoth = false;
                    InPrawn = false;
                    break;
            }


            //SurfaceDepth = GetInt("surface_depth");
            DepthLevel = GetInt("depth_level");

            Health = GetInt("health");
            Food = GetInt("food");
            Water = GetInt("water");

            CanBreathe = GetBool("can_breathe");

            Global.logger.Info(GetInt("oxygen_capacity"));
            if (GetInt("oxygen_capacity") >= 1)
            {
                OxygenCapacity = GetInt("oxygen_capacity");
            }
            else
                OxygenCapacity = 45;

            OxygenAvailable = GetInt("oxygen_available");

            switch (GetInt("pda_state"))
            {
                case 0: //PDA opened
                    PDAopened = true;
                    PDAclosed = false;
                    PDAopening = false;
                    PDAclosing = false;
                    break;

                case 1: //PDA closed
                    PDAopened = false;
                    PDAclosed = true;
                    PDAopening = false;
                    PDAclosing = false;
                    break;

                case 2: //PDA opening
                    PDAopened = false;
                    PDAclosed = false;
                    PDAopening = true;
                    PDAclosing = false;
                    break;

                case 3: //PDA closing
                    PDAopened = false;
                    PDAclosed = false;
                    PDAopening = false;
                    PDAclosing = true;
                    break;

                default: //In Menu
                    PDAopened = false;
                    PDAclosed = false;
                    PDAopening = false;
                    PDAclosing = false;
                    break;
            }

            IsSwimming = GetBool("is_in_water_for_swimming");

            MotorMode = GetInt("motor_mode");
            if (MotorMode == 2)
                IsSeagliding = true;
            else
                IsSeagliding = false;

            Mode = GetInt("mode");
            if (Mode == 1 || Mode == 2) // Mode 1 = Piloting/Mode 2 = locked Piloting
                IsPiloting = true;
            else
                IsPiloting = false;

        }

    }
}
