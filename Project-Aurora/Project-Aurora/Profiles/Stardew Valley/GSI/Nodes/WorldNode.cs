using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes {
    public enum TimeRange
    {
        sunrise,
        morning,
        daytime,
        evening,
        twilight,
        night,
        midnight
    }

    public class WorldNode : AutoJsonNode<WorldNode> {
        [AutoJsonPropertyName("current_season")]
        public string CurrentSeason;
        [AutoJsonPropertyName("current_location")]
        public string CurrentLocation;
        [AutoJsonPropertyName("is_raining")]
        public bool IsRaining;
        [AutoJsonPropertyName("is_snowing")]
        public bool IsSnowing;
        [AutoJsonPropertyName("time_paused")]
        public bool TimePaused;
        [AutoJsonPropertyName("curr_hour")]
        public int CurrentHour;
        [AutoJsonPropertyName("curr_min")]
        public int CurrentMin;
        [AutoJsonPropertyName("time_range")]
        public TimeRange TimeRange;

        internal WorldNode(string json) : base(json) {}
    }
}