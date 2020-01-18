using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Aurora.Profiles;
using System.Net;
using Aurora.Profiles.LeagueOfLegends.GSI;
using Aurora.Profiles.LeagueOfLegends.GSI.Nodes;
using Newtonsoft.Json.Linq;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class GameEvent_LoL : LightEvent
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string URI = "https://localhost:2999/liveclientdata/activeplayer";
        public override void UpdateTick()
        {
            string data;
            try
            {
                var res = client.GetAsync(URI).Result;
                data = res.Content.ReadAsStringAsync().Result;
            }
            catch(Exception e)
            {
                data = "";
            }

            if (string.IsNullOrEmpty(data))
                return;

            var jsonData = JsonConvert.DeserializeObject(data) as JObject;
            (_game_state as GameState_LoL).Player.Stats = jsonData["championStats"].ToObject<StatsNode>();
            (_game_state as GameState_LoL).Player.Gold = jsonData["currentGold"].ToObject<float>();
            (_game_state as GameState_LoL).Player.Level = jsonData["level"].ToObject<int>();
        }

        public override void ResetGameState()
        {
            _game_state = new GameState_LoL();
        }

        public GameEvent_LoL() : base()
        {
            //ignore ssl errors
            //client.Timeout = TimeSpan.FromMilliseconds(100);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
    }
}
