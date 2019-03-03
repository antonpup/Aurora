using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public class VehicleSubNode : Node<VehicleSubNode> {

        //public string type; //Base, Cyclops, Seamoth or Prawn
        public string Type;

        //General Vehicle/Sub Variables:
        public bool InBase;
        public bool InCyclops;
        public bool InSeamoth;
        public bool InPrawn;
        public float Power;
        public float MaxPower;
        public bool FloodlightOn;
        public float VehicleHealth;
        public float VehicleMaxHealth;
        public float CrushDepth;

        //General Sub Variables:
        public int LightState;
        public bool LightOn;
        public bool LightOnDanger;
        public bool LightOff;

        //Cyclops Variables:
        public bool CyclopsWarning;
        public bool CyclopsFireSuppressionActive;
        public bool CyclopsSilentRunning;
        public int CyclopsMotorMode;

        public bool CyclopsMotorSlow { get; private set; }
        public bool CyclopsMotorNormal { get; private set; }
        public bool CyclopsMotorFlank { get; private set; }

        public bool CyclopsEngineOn;

        public float CyclopsNoice;

        //Base Variables:

        //Vehicle Variables:
        public float VehicleTemperatur;

        internal VehicleSubNode(string json) : base(json) {

            //Base, Cyclops, Seamoth or Prawn
            Type = GetString("type");

            //General Vehicle/Sub Variables:
            InBase = Type == "Base";
            InCyclops = Type == "Cyclops";
            InSeamoth = Type == "Seamoth";
            InPrawn = Type == "Prawn";

            Power = GetFloat("power");
            MaxPower = GetFloat("max_power");

            FloodlightOn = GetBool("floodlight");

            VehicleHealth = GetFloat("vehicle_health");
            VehicleMaxHealth = GetFloat("vehicle_max_health");
            CrushDepth = GetFloat("crushDepth");

            //General Sub Variables:
            LightState = GetInt("lightstate"); // On = 0, On with Danger = 1, Off = 2
            LightOn = LightState == 0;
            LightOnDanger = LightState == 1;
            LightOff = LightState == 2;


            //Cyclops Variables:
            CyclopsWarning = GetBool("cyclops_warning");
            CyclopsFireSuppressionActive = GetBool("cyclops_fire_suppression_state");
            CyclopsSilentRunning = GetBool("cyclops_silent_running");

            CyclopsMotorMode = GetInt("cyclops_motor_mode");
            CyclopsMotorSlow = CyclopsMotorMode == 0;
            CyclopsMotorNormal = CyclopsMotorMode == 1;
            CyclopsMotorFlank = CyclopsMotorMode == 2;

            CyclopsEngineOn = GetBool("cyclops_engine_on");

            CyclopsNoice = GetFloat("cyclops_noice_percent");

            //Base Variables:

            //Vehicle Variables:
            VehicleTemperatur = GetFloat("temperatur");

    }
    }
}
