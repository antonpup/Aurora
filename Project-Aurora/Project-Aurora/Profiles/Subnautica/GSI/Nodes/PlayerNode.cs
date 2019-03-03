using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public class PlayerNode : Node<PlayerNode> {

        public string Biom;

        public bool InLifePod;

        public bool InSafeShallows;
        public bool InLostRiver;
        public bool InKelpForest;
        public bool InGrassyPlateus;
        public bool InUnderwaterIslands;
        public bool InFloatingIsland;
        public bool InLavaZone;
        public bool InInactiveLavaZone;
        public bool InMushroomForest;
        public bool InBloodKelp;
        public bool InSandDunes;
        public bool InGrandReef;
        public bool InBulbZone;
        public bool InMountains;
        public bool InSparseReef;
        public bool InJellyshroom;
        public bool InCrashZone;
        public bool InCragField;

        //public int SurfaceDepth; //always 0?
        public int Depth;
        public int DepthLevel;

        public int Health;
        public int Food;
        public int Water;

        public int GameMode;
        public bool InSurvivalMode;
        public bool InCreativeMode;
        public bool InFreedomMode;
        public bool InHardcoreMode;
        /*
        Survival = 0,
        Freedom = 1,
        Hardcore = 2,
        Creative = 3,
        None = 4
        */

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
        //public bool IsWalking; //when walking? when walking in Base or Cyclops!
        //public bool IsDiving; //Seagliding does not count
        public bool IsSeagliding;
        //public bool IsInVehicle;
        //public bool IsInMech;
        //public bool IsRunning; //is also when in Water?

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

            InSafeShallows = Biom == "safe" || Biom == "Lifepod";
            InLostRiver = Biom == "lostriver";
            InKelpForest = Biom == "kelpforest";
            InGrassyPlateus = Biom == "grassy";
            InUnderwaterIslands = Biom == "underwaterislands";
            InFloatingIsland = Biom == "floatingisland";
            InLavaZone = Biom == "lava";
            InInactiveLavaZone = Biom == "ilz";
            InMushroomForest = Biom == "mushroom";
            InBloodKelp = Biom == "bloodkelp";
            InSandDunes = Biom == "dunes";
            InGrandReef = Biom == "grandreef";
            InBulbZone = Biom == "koosh";
            InMountains = Biom == "mountains";
            InSparseReef = Biom == "sparse";
            InJellyshroom = Biom == "jellyshroom";
            InCrashZone = Biom == "crash";
            InCragField = Biom == "crag";

            //SurfaceDepth = GetInt("surface_depth");
            DepthLevel = GetInt("depth_level");
            Depth = DepthLevel >= 0 ? 0 : Math.Abs(DepthLevel);

            Health = GetInt("health");
            Food = GetInt("food");
            Water = GetInt("water");

            GameMode = GetInt("game_mode");
            InSurvivalMode = GameMode == 0;
            InFreedomMode = GameMode == 1;
            InHardcoreMode = GameMode == 2;
            InCreativeMode = GameMode == 3;

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
