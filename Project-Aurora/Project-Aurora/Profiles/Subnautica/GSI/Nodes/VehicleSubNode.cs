using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public enum LightingStates
    {                       //Meaning in Game:
        OnNoDanger = 0,    //Operational = 0
        OnDanger = 1,         //Danger = 1
        Off = 2         //Damaged = 2
    }
    public enum CyclopsMotorModes
    {
        Slow = 0,
        Standard = 1,
        Flank = 2
    }
    public class VehicleSubNode : Node<VehicleSubNode> {
        //Base, Cyclops, Seamoth or Prawn //Change to Enum
        public string Type;
        public bool InBase;
        public bool InCyclops;
        public bool InSeamoth;
        public bool InPrawn;

        public int Power;
        public int MaxPower;

        public bool FloodlightEnabled;

        public int VehicleHealth;
        public int VehicleMaxHealth;

        public int CrushDepth;

        public LightingStates LightState;

        public bool CyclopsWarning;
        public bool CyclopsFireSuppression;
        public bool CyclopsSilentRunning;
        
        public CyclopsMotorModes CyclopsMotorMode;

        public bool CyclopsEngineOn;
        public float CyclopsNoice;

        internal VehicleSubNode(string json) : base(json) {
            //Base, Cyclops, Seamoth or Prawn
            Type = GetString("type");

            InBase = Type == "Base";
            InCyclops = Type == "Cyclops";
            InSeamoth = Type == "Seamoth";
            InPrawn = Type == "Prawn";

            Power = GetInt("power");
            MaxPower = GetInt("max_power");

            FloodlightEnabled = GetBool("floodlight");

            LightState = (LightingStates)GetInt("lightstate");

            VehicleHealth = GetInt("vehicle_health");
            VehicleMaxHealth = GetInt("vehicle_max_health");
            CrushDepth = GetInt("crushDepth");

            CyclopsWarning = GetBool("cyclops_warning");
            CyclopsFireSuppression = GetBool("cyclops_fire_suppression_state");
            CyclopsSilentRunning = GetBool("cyclops_silent_running");
            
            CyclopsMotorMode = (CyclopsMotorModes)GetInt("cyclops_motor_mode");

            CyclopsEngineOn = GetBool("cyclops_engine_on");
            CyclopsNoice = GetFloat("cyclops_noice_percent");

        }
    }
}
