using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.LeagueOfLegends.GSI.Nodes
{
    public enum SummonerSpell
    {
        Undefined = -1,
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

    public class SpellNode : Node<SpellNode>
    {
        public SummonerSpell Spell;
        public float Cooldown;
    }
}
