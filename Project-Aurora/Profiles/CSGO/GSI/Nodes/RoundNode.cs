using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    public class RoundNode : Node
    {
        public readonly RoundPhase Phase;
        public readonly BombState Bomb;
        public readonly RoundWinTeam WinTeam;

        internal RoundNode(string JSON)
            : base(JSON)
        {
            Phase = GetEnum<RoundPhase>("phase");
            Bomb = GetEnum<BombState>("bomb");
            WinTeam = GetEnum<RoundWinTeam>("win_team");
        }
    }

    public enum RoundPhase
    {
        Undefined,
        Live,
        Over,
        FreezeTime
    }

    public enum BombState
    {
        Undefined,
        Planted,
        Exploded,
        Defused
    }

    public enum RoundWinTeam
    {
        Undefined,
        T,
        CT
    }
}
