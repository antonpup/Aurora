using System;
using Aurora.Profiles.EliteDangerous.Journal.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aurora.Profiles.EliteDangerous.Journal
{
    public enum EventType
    {
        FSDTarget,
        StartJump,
        SupercruiseEntry,
        SupercruiseExit,
        Fileheader,
        FSDJump,
        Loadout,
        Music,
        LaunchFighter,
        DockFighter,
        FighterDestroyed,
        FighterRebuilt,
    }

    public class JournalEvent
    {
        public DateTime timestamp;
        public EventType @event;
    }
    
    public class JournalEventJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(JournalEvent).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, 
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);

            switch (item["event"].Value<string>())
            {
                case "FSDTarget": return item.ToObject<FSDTarget>();
                case "StartJump": return item.ToObject<StartJump>();
                case "SupercruiseEntry": return item.ToObject<SupercruiseEntry>();
                case "SupercruiseExit": return item.ToObject<SupercruiseExit>();
                case "Fileheader": return item.ToObject<Fileheader>();
                case "FSDJump": return item.ToObject<FSDJump>();
                case "Loadout": return item.ToObject<Loadout>();
                case "Music": return item.ToObject<Music>();
                case "LaunchFighter": return item.ToObject<LaunchFighter>();
                case "DockFighter": return item.ToObject<DockFighter>();
                case "FighterDestroyed": return item.ToObject<FighterDestroyed>();
                case "FighterRebuilt": return item.ToObject<FighterRebuilt>();
            }
                
            //Do not deserialize an event we don't need since it's REALLY SLOW!
            return null;
        }

        public override void WriteJson(JsonWriter writer, 
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}