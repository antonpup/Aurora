using System;

namespace Aurora.Profiles.EliteDangerous.GSI.Nodes
{
    public class StatusState
    {
        long flagsSet = -1;
        long flagsNotSet = -1;
        int guiFocus = -1;
        private Func<bool> conditionCallback = null;

        public StatusState(long flagsSet, long flagsNotSet = -1) : this(flagsSet, -1, flagsNotSet, null)
        {
        }

        public StatusState(long flagsSet, long flagsNotSet, Func<bool> conditionCallback) : this(flagsSet, -1,
            flagsNotSet, conditionCallback)
        {
        }

        public StatusState(long flagsSet, int guiFocus, long flagsNotSet, Func<bool> conditionCallback)
        {
            this.flagsSet = flagsSet;
            this.guiFocus = guiFocus;
            this.flagsNotSet = flagsNotSet;
            this.conditionCallback = conditionCallback;
        }

        public StatusState(Func<bool> conditionCallback)
        {
            this.conditionCallback = conditionCallback;
        }

        public bool ConditionSatisfied(Status status)
        {
            return ConditionSatisfied(status.Flags, status.GuiFocus);
        }

        public bool ConditionSatisfied(long flags, int guiFocus)
        {
            if (conditionCallback != null && !conditionCallback())
            {
                return false;
            }

            if (this.guiFocus != -1 && this.guiFocus != guiFocus)
            {
                return false;
            }

            if (this.flagsSet != -1 && !Flag.IsFlagSet(flags, this.flagsSet))
            {
                return false;
            }

            if (this.flagsNotSet != -1 && Flag.IsFlagSet(flags, this.flagsNotSet))
            {
                return false;
            }

            return true;
        }
    }

    public static class Flag
    {
        public static readonly int NONE = -1;
        public static readonly int DOCKED = 1;
        public static readonly int LANDED_PLANET = 1 << 1;
        public static readonly int LANDING_GEAR = 1 << 2;
        public static readonly int SHIELDS_UP = 1 << 3;
        public static readonly int SUPERCRUISE = 1 << 4;
        public static readonly int FA_OFF = 1 << 5;
        public static readonly int HARDPOINTS = 1 << 6;
        public static readonly int IN_WING = 1 << 7;
        public static readonly int SHIP_LIGHTS = 1 << 8;
        public static readonly int CARGO_SCOOP = 1 << 9;
        public static readonly int SILENT_RUNNING = 1 << 10;
        public static readonly int SCOOPING = 1 << 11;
        public static readonly int SRV_HANDBRAKE = 1 << 12;
        public static readonly int SRV_TURRET = 1 << 13;
        public static readonly int SRV_UNDER_SHIP = 1 << 14;
        public static readonly int SRV_DRIVE_ASSIST = 1 << 15;
        public static readonly int MASS_LOCK = 1 << 16;
        public static readonly int FSD_CHARGING = 1 << 17;
        public static readonly int FSD_COOLDOWN = 1 << 18;
        public static readonly int LOW_FUEL = 1 << 19; // Less than 25%
        public static readonly int OVERHEATING = 1 << 20; // More than 100%
        public static readonly int HAS_LAT_LONG = 1 << 21;
        public static readonly int IN_DANGER = 1 << 22;
        public static readonly int INTERDICTION = 1 << 23;
        public static readonly int IN_SHIP = 1 << 24;
        public static readonly int IN_FIGHTER = 1 << 25;
        public static readonly int IN_SRV = 1 << 26;
        public static readonly int HUD_DISCOVERY_MODE = 1 << 27;
        public static readonly int NIGHT_VISION = 1 << 28;

        public static bool IsFlagSet(long bitmask, long flag)
        {
            return (bitmask & flag) == flag;
        }
    }

    /// <summary>
    /// Class representing player status
    /// </summary>
    public class Status : Node<Status>
    {
        public DateTime timestamp;
        public string @event;
        public long Flags;
        public int[] Pips;
        public int FireGroup;
        public int GuiFocus;
        public FuelClass Fuel;
        public double Cargo;

        public class FuelClass
        {
            public double FuelMain;
            public double FuelReservoir;
        }
    }
}