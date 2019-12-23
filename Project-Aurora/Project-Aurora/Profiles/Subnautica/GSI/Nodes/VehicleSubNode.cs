using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Subnautica.GSI.Nodes {
    public class VehicleSubNode : Node<VehicleSubNode> {

        public int Power;
        public int MaxPower;

        public bool FloodlightEnabled;

        public int VehicleHealth;
        public int VehicleMaxHealth;

        public int CrushDepth;

        public int LightState;
        public bool LightOn;
        public bool LightDanger;
        public bool LightOff;
        // On = 0, On with Danger = 1, Off = 2

        public bool CyclopsWarning;
        public bool CyclopsFireSuppression;
        public bool CyclopsSilentRunning;
        
        public int CyclopsMotorMode;
        public bool CyclopsSlowMode;
        public bool CyclopsStandardMode;
        public bool CyclopsFlankMode;

        public bool CyclopsEngineOn;
        public float CyclopsNoice;

        internal VehicleSubNode(string json) : base(json) {

            Power = GetInt("power");
            MaxPower = GetInt("max_power");

            FloodlightEnabled = GetBool("floodlight");
            // On = 0, On with Danger = 1, Off = 2
            LightState = GetInt("lightstate");
            LightOn = LightState == 0 || LightState == 1;
            LightDanger = LightState == 1;
            LightOff = LightState == 2;

            VehicleHealth = GetInt("vehicle_health");
            VehicleMaxHealth = GetInt("vehicle_max_health");
            CrushDepth = GetInt("crushDepth");

            CyclopsWarning = GetBool("cyclops_warning");
            CyclopsFireSuppression = GetBool("cyclops_fire_suppression_state");
            CyclopsSilentRunning = GetBool("cyclops_silent_running");
            CyclopsMotorMode = GetInt("cyclops_motor_mode");
            CyclopsSlowMode = CyclopsMotorMode == 0;
            CyclopsStandardMode = CyclopsMotorMode == 1;
            CyclopsFlankMode = CyclopsMotorMode == 2;
            CyclopsEngineOn = GetBool("cyclops_engine_on");
            CyclopsNoice = GetFloat("cyclops_noice_percent");

        }
    }
}
