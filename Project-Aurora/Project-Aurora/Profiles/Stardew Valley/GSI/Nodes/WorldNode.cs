using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.StardewValley.GSI.Nodes {

    public enum TimeRange
    {
        Sunrise, //< 750
        Morning, //< 900
        Daytime, //< 1200
        Evening, //< 1600
        Twilight,//< 1750
        Night,   //< 2100
        Midnight//>= 2100
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

        public TimeNode Time => NodeFor<TimeNode>("Time");
        public WeatherNode Weather => NodeFor<WeatherNode>("Weather");

        internal WorldNode(string json) : base(json) {}
    }

    public class TimeNode : AutoJsonNode<TimeNode>
    {
        public bool Paused;
        public bool isFestivalDay;
        public bool isWeddingToday;
        public int Hour;
        public int Minute;
        public TimeRange Range;

        internal TimeNode(string JSON) : base(JSON) { }
    }

    public class WeatherNode : AutoJsonNode<WeatherNode>
    {
        public bool IsSnowing;
        public bool IsRaining;
        public bool IsDebrisWeather;
        public bool IsLightning;

        internal WeatherNode(string JSON) : base(JSON) { }
    }
}