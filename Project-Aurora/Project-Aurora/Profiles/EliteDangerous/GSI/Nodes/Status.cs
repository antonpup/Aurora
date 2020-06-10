using System;

namespace Aurora.Profiles.EliteDangerous.GSI.Nodes
{
    public static class Flag
    {
        public const long UNSPECIFIED = -1;
        public const long DOCKED = 1;
        public const long LANDED_PLANET = 1 << 1;
        public const long LANDING_GEAR = 1 << 2;
        public const long SHIELDS_UP = 1 << 3;
        public const long SUPERCRUISE = 1 << 4;
        public const long FA_OFF = 1 << 5;
        public const long HARDPOINTS = 1 << 6;
        public const long IN_WING = 1 << 7;
        public const long SHIP_LIGHTS = 1 << 8;
        public const long CARGO_SCOOP = 1 << 9;
        public const long SILENT_RUNNING = 1 << 10;
        public const long SCOOPING = 1 << 11;
        public const long SRV_HANDBRAKE = 1 << 12;
        public const long SRV_TURRET = 1 << 13;
        public const long SRV_UNDER_SHIP = 1 << 14;
        public const long SRV_DRIVE_ASSIST = 1 << 15;
        public const long MASS_LOCK = 1 << 16;
        public const long FSD_CHARGING = 1 << 17;
        public const long FSD_COOLDOWN = 1 << 18;
        public const long LOW_FUEL = 1 << 19; // Less than 25%
        public const long OVERHEATING = 1 << 20; // More than 100%
        public const long HAS_LAT_LONG = 1 << 21;
        public const long IN_DANGER = 1 << 22;
        public const long INTERDICTION = 1 << 23;
        public const long IN_SHIP = 1 << 24;
        public const long IN_FIGHTER = 1 << 25;
        public const long IN_SRV = 1 << 26;
        public const long HUD_DISCOVERY_MODE = 1 << 27;
        public const long NIGHT_VISION = 1 << 28;
        public const long ALTITUDE_FROM_AVERAGE_RADIUS = 1 << 29;

        public static bool IsFlagSet(long bitmask, long flag)
        {
            return (bitmask & flag) == flag;
        }
        
        public static bool AtLeastOneFlagSet(long bitmask, long flag)
        {
            return (bitmask & flag) != 0;
        }
    }

    public enum GuiFocus
    {
        NONE = 0,
        PANEL_SYSTEMS = 1,
        PANEL_NAV = 2,
        PANEL_COMS = 3,
        PANEL_ROLE = 4,
        STATION_SERVICES = 5,

        MAP_GALAXY = 6,
        MAP_SYSTEM = 7,
        MAP_ORRERY = 8,
        MODE_FSS = 9,
        MODE_ADS = 10,
        CODEX = 11,
    }
    
    public class Fuel
    {
        public double FuelMain;
        public double FuelReservoir;
    }

    /// <summary>
    /// Class representing player status
    /// </summary>
    public class Status : Node
    {
        public DateTime timestamp;
        public string @event;
        public long Flags;
        public int[] Pips = new [] {8,8,8};
        public int FireGroup;
        public GuiFocus GuiFocus;
        public Fuel Fuel;
        public double Cargo;

        public bool IsFlagSet(long flag)
        {
            return Flag.IsFlagSet(Flags, flag);
        }
    }
}