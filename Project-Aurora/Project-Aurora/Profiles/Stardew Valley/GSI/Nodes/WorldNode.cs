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

    public enum Seasons
    {
        Unknown = -1,
        Spring,
        Summer,
        Fall,
        Winter
    }

    public class WorldNode : AutoJsonNode<WorldNode> {
        public Seasons Season;

        public bool TimePaused;
        public int CurrentHour;
        public int CurrentMin;
        public TimeRange TimeRange;
        public WeatherNode Weather => NodeFor<WeatherNode>("Weather");


        internal WorldNode(string json) : base(json) {}
    }
    public class WeatherNode : AutoJsonNode<WeatherNode>
    {
        public bool IsSnowing;
        public bool IsRaining;
        public bool IsDebrisWeather;
        public bool WeddingToday;
        public bool IsLightning;

        internal WeatherNode(string JSON) : base(JSON) { }
    }
}