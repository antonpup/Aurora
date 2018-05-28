
// Taken from: https://github.com/Funbit/ets2-telemetry-server/blob/master/source/Funbit.Ets.Telemetry.Server/Data/Reader/Ets2TelemetryStructure.cs

using System.Runtime.InteropServices;

namespace Aurora.Profiles.ETS2 {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct ETS2MemoryStruct {
        const int GeneralStringSize = 64;

        public uint time;
        public uint paused;

        public uint ets2_telemetry_plugin_revision;
        public uint ets2_version_major;
        public uint ets2_version_minor;

        // ***** REVISION 1 ****** //

        readonly byte padding1;
        public byte trailer_attached;
        public byte padding2;
        public byte padding3;

        public float speed;
        public float accelerationX;
        public float accelerationY;
        public float accelerationZ;

        public float coordinateX;
        public float coordinateY;
        public float coordinateZ;

        public float rotationX;
        public float rotationY;
        public float rotationZ;

        public int gear;
        public int gearsForward;
        public int gearRanges;
        public int gearRangeActive;

        public float engineRpm;
        public float engineRpmMax;

        public float fuel;
        public float fuelCapacity;
        public float fuelRate;
        public float fuelAvgConsumption;

        public float userSteer;
        public float userThrottle;
        public float userBrake;
        public float userClutch;

        public float gameSteer;
        public float gameThrottle;
        public float gameBrake;
        public float gameClutch;

        public float truckWeight;
        public float trailerWeight;

        public int modelOffset;
        public int modelLength;

        public int trailerOffset;
        public int trailerLength;

        public int timeAbsolute;
        public int gearsReverse;

        public float trailerMass;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] trailerId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] trailerName;

        public int jobIncome;
        public int jobDeadline;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] jobCitySource;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] jobCityDestination;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] jobCompanySource;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] jobCompanyDestination;

        // ***** REVISION 3 ****** //

        public int retarderBrake;
        public int shifterSlot;
        public int shifterToggle;
        public int padding4;

        public byte cruiseControl;
        public byte wipers;

        public byte parkBrake;
        public byte motorBrake;

        public byte electricEnabled;
        public byte engineEnabled;

        public byte blinkerLeftActive;
        public byte blinkerRightActive;
        public byte blinkerLeftOn;
        public byte blinkerRightOn;

        public byte lightsParking;
        public byte lightsBeamLow;
        public byte lightsBeamHigh;
        public uint lightsAuxFront;
        public uint lightsAuxRoof;
        public byte lightsBeacon;
        public byte lightsBrake;
        public byte lightsReverse;

        public byte batteryVoltageWarning;
        public byte airPressureWarning;
        public byte airPressureEmergency;
        public byte adblueWarning;
        public byte oilPressureWarning;
        public byte waterTemperatureWarning;

        public float airPressure;
        public float brakeTemperature;
        public int fuelWarning;
        public float adblue;
        public float adblueConsumption;
        public float oilPressure;
        public float oilTemperature;
        public float waterTemperature;
        public float batteryVoltage;
        public float lightsDashboard;
        public float wearEngine;
        public float wearTransmission;
        public float wearCabin;
        public float wearChassis;
        public float wearWheels;
        public float wearTrailer;
        public float truckOdometer;
        public float cruiseControlSpeed;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] truckMake;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] truckMakeId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GeneralStringSize)]
        public byte[] truckModel;

        // ***** REVISION 4 ****** //

        public float fuelWarningFactor;
        public float adblueCapacity;
        public float airPressureWarningValue;
        public float airPressureEmergencyValue;
        public float oilPressureWarningValue;
        public float waterTemperatureWarningValue;
        public float batteryVoltageWarningValue;

        public uint retarderStepCount;

        public float cabinPositionX;
        public float cabinPositionY;
        public float cabinPositionZ;
        public float headPositionX;
        public float headPositionY;
        public float headPositionZ;
        public float hookPositionX;
        public float hookPositionY;
        public float hookPositionZ;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] shifterType;

        public float localScale;
        public int nextRestStop;
        public float trailerCoordinateX;
        public float trailerCoordinateY;
        public float trailerCoordinateZ;
        public float trailerRotationX;
        public float trailerRotationY;
        public float trailerRotationZ;

        public int displayedGear;
        public float navigationDistance;
        public float navigationTime;
        public float navigationSpeedLimit;

        /*
        const int MaxSlotCount = 32; // TODO: need to fix.
        const int MaxWheelCount = 20;
        public uint wheelCount; 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
        public float[] wheelPositionX;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
        public float[] wheelPositionY;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
        public float[] wheelPositionZ;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
        public byte[] wheelSteerable;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
        public byte[] wheelSimulated;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
        public float[] wheelRadius;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
        public byte[] wheelPowered;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxWheelCount)]
        public byte[] wheelLiftable;        
        public uint selectorCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSlotCount)]
		public int[] slotGear;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSlotCount)]
		public uint[] slotHandlePosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSlotCount)]
		public uint[] slotSelectors;         
        */
    }
}
