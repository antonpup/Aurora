using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes {
    public enum TimeRange
    {
        Sunrise,
        Morning,
        Daytime,
        Evening,
        Twilight,
        Night,
        Midnight
    }

    public class WorldNode : AutoJsonNode<WorldNode> {
        public string CurrentSeason;
        public string CurrentLocation;
        public bool IsRaining;
        public bool IsSnowing;
        public bool TimePaused;
        public int CurrentHour;
        public int CurrentMin;
        public TimeRange TimeRange;

        internal WorldNode(string json) : base(json) {}
    }
}