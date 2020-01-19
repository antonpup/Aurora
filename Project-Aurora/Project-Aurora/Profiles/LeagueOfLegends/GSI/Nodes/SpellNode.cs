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
        Exhaust,
        Flash,
        Ghost,
        Heal,
        Smite,
        Teleport,
        Clarity,
        Ignite,
        Barrier,
        Mark
    }

    public class SpellNode : Node<SpellNode>
    {
        public SummonerSpell Spell;
        public float Cooldown;
    }
}
