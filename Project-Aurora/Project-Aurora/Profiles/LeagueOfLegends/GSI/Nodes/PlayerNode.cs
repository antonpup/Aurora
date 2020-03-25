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
        Undefined = -1,
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
        Undefined = -1,
        Order,
        Chaos
    }

    public enum SummonerSpell
    {
        Undefined = -1,
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
        Undefined,
        Top,
        Jungle,
        Middle,
        Bot,
        [JsonProperty("UTILITY")]
        Support
    }

    public class PlayerNode : Node<PlayerNode>
    {
        public StatsNode ChampionStats = new StatsNode();
        public AbilitiesNode Abilities = new AbilitiesNode();
        public InventoryNode Items = new InventoryNode();
        public SummonerSpell SpellD = SummonerSpell.Undefined;
        public SummonerSpell SpellF = SummonerSpell.Undefined;
        public Champion Champion = Champion.Undefined;
        public Team Team = Team.Undefined;
        public Position Position = Position.Undefined;
        public string SummonerName;
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
