using System;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// Class representing information about the round
    /// </summary>
    public class RoundNode : Node
    {
        /// <summary>
        /// Current round phase
        /// </summary>
        public RoundPhase Phase;

        /// <summary>
        /// Current bomb state
        /// </summary>
        public BombState Bomb;

        /// <summary>
        /// Round winning team
        /// </summary>
        public RoundWinTeam WinTeam;

        internal RoundNode(string JSON)
            : base(JSON)
        {
            Phase = GetEnum<RoundPhase>("phase");
            Bomb = GetEnum<BombState>("bomb");
            WinTeam = GetEnum<RoundWinTeam>("win_team");
        }
    }

    /// <summary>
    /// Enum list for each round phase
    /// </summary>
    public enum RoundPhase
    {
        /// <summary>
        /// Undefined phase
        /// </summary>
        Undefined,

        /// <summary>
        /// Live round phase
        /// </summary>
        Live,

        /// <summary>
        /// Round over phase
        /// </summary>
        Over,

        /// <summary>
        /// Round paused phase
        /// </summary>
        FreezeTime
    }

    /// <summary>
    /// Enum list for each bomb state
    /// </summary>
    public enum BombState
    {
        /// <summary>
        /// Undefined state
        /// </summary>
        Undefined,

        /// <summary>
        /// Bomb plated state
        /// </summary>
        Planted,

        /// <summary>
        /// Bomb exploded state
        /// </summary>
        Exploded,

        /// <summary>
        /// Bomb defused state
        /// </summary>
        Defused
    }

    /// <summary>
    /// Enum lust for each winning team
    /// </summary>
    public enum RoundWinTeam
    {
        /// <summary>
        /// Undefined team
        /// </summary>
        Undefined,

        /// <summary>
        /// Terrorist team
        /// </summary>
        T,

        /// <summary>
        /// Counter-Terrorist team
        /// </summary>
        CT
    }
}
