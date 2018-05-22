namespace Aurora.Profiles.ETS2.GSI.Nodes {
    public class TruckNode : Node<TruckNode> {

        private ETS2MemoryStruct _memdat;

        /// <summary>Brand Id of the current truck. Standard values are: "daf", "iveco", "man", "mercedes", "renault", "scania", "volvo".</summary>
        public string id;
        /// <summary>Localized brand name of the current truck for display purposes.</summary>
        public string make;
        /// <summary>Localized model name of the current truck.</summary>
        public string model;

        /// <summary>Current truck speed in km/h. If truck is moving backwards the value is negative.</summary>
        public float speed => _memdat.speed * 3.6f;
        /// <summary>Speed selected for the cruise control in km/h.</summary>
        public float cruiseControlSpeed => _memdat.cruiseControlSpeed * 3.6f;
        /// <summary>Whether or not cruise control is active.</summary>
        public bool cruiseControlOn => _memdat.cruiseControlSpeed > 0;
        /// <summary>The value of the truck's odometer in km.</summary>
        public float odometer => _memdat.truckOdometer;

        /// <summary>Gear that is currently selected in the engine (physical gear). Positive values reflect forward gears, negative - reverse.</summary>
        public int gear => _memdat.gear;
        /// <summary>Gear that is currently displayed on the main dashboard inside the game. Positive values reflect forward gears, negative - reverse.</summary>
        public int displayedGear => _memdat.displayedGear;
        /// <summary>Total number of forward gears fitted to the truck.</summary>
        public int forwardGears => _memdat.gearsForward;
        /// <summary>Total number of reverse gears fitted to the truck.</summary>
        public int reverseGears => _memdat.gearsReverse;
        /// <summary>Type of the shifter selected in the game's settings. One of the following values: "arcade", "automatic", "manual", "hshifter".</summary>
        public string shifterType;

        /// <summary>Current RPM value of the truck's engine (revolutions per minute).</summary>
        public float engineRpm => _memdat.engineRpm;
        /// <summary>Maximum RPM value of the truck's engine.</summary>
        public float engineRpmMax => _memdat.engineRpmMax;

        /// <summary>Current amount of fuel in litres.</summary>
        public float fuel => _memdat.fuel;
        /// <summary>Fuel tank capacity in litres.</summary>
        public float fuelCapacity => _memdat.fuelCapacity;
        /// <summary>Average consumption of the fuel in litres/km.</summary>
        public float fuelAverageConsumption => _memdat.fuelAvgConsumption;
        /// <summary>When total fuel is less than this amount of the capacity, the warning light activates (between 0 and 1).</summary>
        public float fuelWarningFactor => _memdat.fuelWarningFactor;
        /// <summary>Whether the low fuel warning light is on or not.</summary>
        public bool fuelWarningOn => _memdat.fuelWarning != 0;

        /// <summary>Current level of truck's engine wear/damage between 0 (min) and 1 (max).</summary>
        public float wearEngine => _memdat.wearEngine;
        /// <summary>Current level of truck's transmission wear/damage between 0 (min) and 1 (max).</summary>
        public float wearTransmission => _memdat.wearTransmission;
        /// <summary>Current level of truck's cabin wear/damage between 0 (min) and 1 (max).</summary>
        public float wearCabin => _memdat.wearCabin;
        /// <summary>Current level of truck's chassis wear/damage between 0 (min) and 1 (max).</summary>
        public float wearChassis => _memdat.wearChassis;
        /// <summary>Current level of truck's wheel wear/damage between 0 (min) and 1 (max).</summary>
        public float wearWheels => _memdat.wearWheels;

        /// <summary>Steering received from input (-1;1). Note that it is interpreted counterclockwise. If the user presses the steer right button on digital input (e.g. keyboard) this value goes immediatelly to -1.0.</summary>
        public float userSteer => _memdat.userSteer;
        /// <summary>Throttle received from input (-1;1). If the user presses the forward button on digital input (e.g. keyboard) this value goes immediately to 1.0.</summary>
        public float userThrottle => _memdat.userThrottle;
        /// <summary>Brake received from input (-1;1). If the user presses the brake button on digital input (e.g. keyboard) this value goes immediately to 1.0.</summary>
        public float userBrake => _memdat.userBrake;
        /// <summary>Clutch received from input (-1;1). If the user presses the clutch button on digital input (e.g. keyboard) this value goes immediately to 1.0.</summary>
        public float userClutch => _memdat.userClutch;
        /// <summary>Steering as used by the simulation (-1;1). Note that it is interpreted counterclockwise. Accounts for interpolation speeds and simulated counterfoces for digital inputs.</summary>
        public float gameSteer => _memdat.gameSteer;
        /// <summary>Throttle pedal input as used by the simulation (0;1). Accounts for the press attack curve for digital inputs or cruise-control input.</summary>
        public float gameThrottle => _memdat.gameThrottle;
        /// <summary>Brake pedal input as used by the simulation (0;1). Accounts for the press attack curve for digital inputs. Does not contain retarder, parking or motor brake.</summary>
        public float gameBrake => _memdat.gameBrake;
        /// <summary>Clutch pedal input as used by the simulation (0;1). Accounts for the automatic shifting or interpolation of player input.</summary>
        public float gameClutch => _memdat.gameClutch;

        /// <summary>Gearbox slot the h-shifter handle is currently in. 0 means that no slot is selected.</summary>
        public int shifterSlot => _memdat.shifterSlot;

        /// <summary>Indicates whether the engine is currently turned on or off.</summary>
        public bool engineOn => _memdat.engineEnabled != 0;
        /// <summary>Indicates whether the electric is enabled or not.</summary>
        public bool electricOn => _memdat.electricEnabled != 0;

        /// <summary>Whether the windscreen wipers are currently active.</summary>
        public bool wipersOn => _memdat.wipers != 0;

        /// <summary>Current level of the retarder brake. Ranges from 0 to RetarderStepCount.</summary>
        public int retarderBrake => _memdat.retarderBrake;
        /// <summary>Number of steps in the retarder. 0 if retarder is not mounted to the truck.</summary>
        public int retarderStepCount => (int)_memdat.retarderStepCount;

        /// <summary>Whether the parking brake is enabled or not.</summary>
        public bool parkBrakeOn => _memdat.parkBrake != 0;

        /// <summary>Whether the motor brake is enabled or not.</summary>
        public bool motorBrakeOn => _memdat.motorBrake != 0;

        /// <summary>Temperature of the brakes in degrees celsius.</summary>
        public float brakeTemperature => _memdat.brakeTemperature;

        /// <summary>Current amount of AdBlue in liters.</summary>
        public float adblue => _memdat.adblue;
        /// <summary>AdBlue tank capacity in liters.</summary>
        public float adblueCapacity => _memdat.adblueCapacity;
        /// <summary>Average consumption of the AdBlue in litres/km.</summary>
        public float adblueAverageConsumption => _memdat.adblueConsumption;
        /// <summary>Whether the low AdBlue warning is active or not.</summary>
        public bool adblueWarningOn => _memdat.adblueWarning != 0;

        /// <summary>Pressure in the brake air tank in psi.</summary>
        public float airPressure => _memdat.airPressure;
        /// <summary>Is the air pressure warning active or not.</summary>
        public bool airPressureWarningOn => _memdat.airPressureWarning != 0;
        /// <summary>Pressure of the air in the tank below which the warning activates.</summary>
        public float airPressureWarningValue => _memdat.airPressureWarningValue;
        /// <summary>Whether the emergency brakes are active as result of low air pressure.</summary>
        public bool airPressureEmergencyOn => _memdat.airPressureEmergency != 0;
        /// <summary>Pressure of the air in the tank below which the emergency brakes activate.</summary>
        public float airPressureEmergencyValue => _memdat.airPressureEmergencyValue;

        /// <summary>Temperature of the oil in degrees celsius.</summary>
        public float oilTemperature => _memdat.oilTemperature;
        /// <summary>Pressure of the oil in psi.</summary>
        public float oilPressure => _memdat.oilPressure;
        /// <summary>Whether the oil pressure warning is active or not.</summary>
        public bool oilPressureWarningOn => _memdat.oilPressureWarning != 0;
        /// <summary>Pressure of the oil bellow which the warning activates.</summary>
        public float oilPressureWarningValue => _memdat.oilPressureWarningValue;

        /// <summary>Temperature of the water in degrees celsius.</summary>
        public float waterTemperature => _memdat.waterTemperature;
        /// <summary>Is the water temperature warning active or not.</summary>
        public bool waterTemperatureWarningOn => _memdat.waterTemperatureWarning != 0;
        /// <summary>Temperature of the water above which the warning activates.</summary>
        public float waterTemperatureWarningValue => _memdat.waterTemperatureWarningValue;

        /// <summary>Voltage of the battery.</summary>
        public float batteryVoltage => _memdat.batteryVoltage;
        /// <summary>Is the battery voltage/not charging warning active or not.</summary>
        public bool batteryVoltageWarningOn => _memdat.batteryVoltageWarning != 0;
        /// <summary>Voltage of the battery below which the warning activates.</summary>
        public float batteryVoltageWarningValue => _memdat.batteryVoltageWarningValue;

        /// <summary>Intensity of the dashboard backlight between 0 (off) and 1 (max).</summary>
        public float lightsDashboardValue => _memdat.lightsDashboard;
        /// <summary>Whether the dashboard backlight is on or off.</summary>
        public bool lightsDashboardOn => _memdat.lightsDashboard != 0;

        /// <summary>Whether the left blinker is currently emitting light (will be true if the hazard lights are on).</summary>
        public bool blinkerLeftActive => _memdat.blinkerLeftActive != 0;
        /// <summary>Whether the right blinker is currently emitting light (will be true if the hazard lights are on).</summary>
        public bool blinkerRightActive => _memdat.blinkerRightActive != 0;
        /// <summary>Whether the left blinker is currently on or off (will not be true if the hazard light are on).</summary>
        public bool blinkerLeftOn => _memdat.blinkerLeftOn != 0;
        /// <summary>Whether the right blinker is currently on or off (will not be true if the hazard light are on).</summary>
        public bool blinkerRightOn => _memdat.blinkerRightOn != 0;

        /// <summary>Whether the parking lights are active.</summary>
        public bool lightsParkingOn => _memdat.lightsParking != 0;

        /// <summary>Whether the low beam lights are active.</summary>
        public bool lightsBeamLowOn => _memdat.lightsBeamLow != 0;
        /// <summary>Whether the high beam lights are active.</summary>
        public bool lightsBeamHighOn => _memdat.lightsBeamHigh != 0;

        /// <summary>Whether the front auxiliary lights are active.</summary>
        public bool lightsAuxFrontOn => _memdat.lightsAuxFront != 0;
        /// <summary>Whether the roof auxiliary lights are active.</summary>
        public bool lightsAuxRoofOn => _memdat.lightsAuxRoof != 0;
        /// <summary>Whether the beacon lights are active.</summary>
        public bool lightsBeaconOn => _memdat.lightsBeacon != 0;

        /// <summary>Whether the brake light is active.</summary>
        public bool lightsBrakeOn => _memdat.lightsBrake != 0;
        /// <summary>Whether the reverse light is active.</summary>
        public bool lightsReverseOn => _memdat.lightsReverse != 0;

        /// <summary>Current truck placement in the game world.</summary>
        public PlacementNode placement => new PlacementNode {
            x = _memdat.coordinateX,
            y = _memdat.coordinateY,
            z = _memdat.coordinateZ,
            heading = _memdat.rotationX,
            pitch = _memdat.rotationY,
            roll = _memdat.rotationZ
        };
        /// <summary>Represents vehicle space linear acceleration of the truck measured in metres/second^2</summary>
        public VectorNode acceleration => new VectorNode {
            x = _memdat.accelerationX,
            y = _memdat.accelerationY,
            z = _memdat.accelerationZ
        };

        /// <summary>Default position of the head in the cabin space.</summary>
        public VectorNode head => new VectorNode {
            x = _memdat.headPositionX,
            y = _memdat.headPositionY,
            z = _memdat.headPositionZ
        };
        /// <summary>Position of the cabin in the vehicle space. This is position of the joint around which the cabin rotates. This attribute might be not present if the vehicle does not have a separate cabin.</summary>
        public VectorNode cabin => new VectorNode {
            x = _memdat.cabinPositionX,
            y = _memdat.cabinPositionY,
            z = _memdat.cabinPositionZ
        };
        /// <summary>Position of the trailer connection hook in vehicle space.</summary>
        public VectorNode hook => new VectorNode {
            x = _memdat.hookPositionX,
            y = _memdat.hookPositionY,
            z = _memdat.hookPositionZ
        };

        internal TruckNode(string JSON) : base (JSON) { }
        internal TruckNode() : base() { }

        /// <summary>
        /// Creates an instance of TruckNode and populates the fields with the given memory data structure.
        /// </summary>
        /// <param name="memdat">Data to populate fields with.</param>
        internal TruckNode(ETS2MemoryStruct memdat) {
            _memdat = memdat;
        }
    }
}
