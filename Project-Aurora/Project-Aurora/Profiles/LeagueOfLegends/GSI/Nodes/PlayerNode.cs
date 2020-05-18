using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public enum Champion
    {
        Unknown = -1,
        None = 0,
        Aatrox,
        Ahri,
        Akali,
        Alistar,
        Amumu,
        Anivia,
        Annie,
        Aphelios,
        Ashe,
        AurelionSol,
        Azir,
        Bard,
        Blitzcrank,
        Brand,
        Braum,
        Caitlyn,
        Camille,
        Cassiopeia,
        Chogath,
        Corki,
        Darius,
        Diana,
        Draven,
        DrMundo,
        Ekko,
        Elise,
        Evelynn,
        Ezreal,
        Fiddlesticks,
        Fiora,
        Fizz,
        Galio,
        Gangplank,
        Garen,
        Gnar,
        Gragas,
        Graves,
        Hecarim,
        Heimerdinger,
        Illaoi,
        Irelia,
        Ivern,
        Janna,
        JarvanIV,
        Jax,
        Jayce,
        Jhin,
        Jinx,
        Kaisa,
        Kalista,
        Karma,
        Karthus,
        Kassadin,
        Katarina,
        Kayle,
        Kayn,
        Kennen,
        Khazix,
        Kindred,
        Kled,
        KogMaw,
        Leblanc,
        LeeSin,
        Leona,
        Lissandra,
        Lucian,
        Lulu,
        Lux,
        Malphite,
        Malzahar,
        Maokai,
        MasterYi,
        MissFortune,
        Mordekaiser,
        Morgana,
        Nami,
        Nasus,
        Nautilus,
        Neeko,
        Nidalee,
        Nocturne,
        Nunu,
        Olaf,
        Orianna,
        Ornn,
        Pantheon,
        Poppy,
        Pyke,
        Qiyana,
        Quinn,
        Rakan,
        Rammus,
        RekSai,
        Renekton,
        Rengar,
        Riven,
        Rumble,
        Ryze,
        Sejuani,
        Senna,
        Sett,
        Shaco,
        Shen,
        Shyvana,
        Singed,
        Sion,
        Sivir,
        Skarner,
        Sona,
        Soraka,
        Swain,
        Sylas,
        Syndra,
        TahmKench,
        Taliyah,
        Talon,
        Taric,
        Teemo,
        Thresh,
        Tristana,
        Trundle,
        Tryndamere,
        TwistedFate,
        Twitch,
        Udyr,
        Urgot,
        Varus,
        Vayne,
        Veigar,
        Velkoz,
        Vi,
        Viktor,
        Vladimir,
        Volibear,
        Warwick,
        Xayah,
        Xerath,
        XinZhao,
        Wukong,
        Yasuo,
        Yorick,
        Yuumi,
        Zac,
        Zed,
        Ziggs,
        Zilean,
        Zoe,
        Zyra
    }

    public enum Team
    {
        Unknown = -1,
        None = 0,
        Order,
        Chaos
    }

    public enum SummonerSpell
    {
        Unknown = -1,
        None = 0,
        Cleanse,//210
        Exhaust,//210
        Flash,//300
        Ghost,//180
        Heal,//240
        Smite,//oof
        Teleport,//260
        Clarity,//240
        Ignite,//180
        Barrier,//180
        Mark,//80
        Dash//0
    }

    public enum Position
    {
        Unknown = -1,
        None = 0,
        Top,
        Jungle,
        Middle,
        Bot,
        [JsonProperty("UTILITY")]
        Support
    }

    public class PlayerNode : Node
    {
        public StatsNode ChampionStats = new StatsNode();
        public AbilitiesNode Abilities = new AbilitiesNode();
        public InventoryNode Items = new InventoryNode();
        public SummonerSpell SpellD = SummonerSpell.None;
        public SummonerSpell SpellF = SummonerSpell.None;
        public Champion Champion = Champion.None;
        public Team Team = Team.None;
        public Position Position = Position.None;
        public string SummonerName = "";
        public int Level;
        public float Gold;       
        public bool IsDead;   
        public int Kills;
        public int Deaths;
        public int Assists;
        public int CreepScore;
        public float WardScore;
        public float RespawnTimer;
    }
}
