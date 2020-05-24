using Aurora.Profiles.RocketLeague.GSI.Nodes;
using System;
using System.ComponentModel;

namespace Aurora.Profiles.RocketLeague.GSI
{
    public enum RLStatus
    {
        [Description("Menu")]
        Undefined = -1,
        [Description("Replay")]
        Replay,
        [Description("In Game")]
        InGame,
        [Description("Freeplay")]
        Freeplay,
        [Description("Training")]
        Training,
        [Description("Spectate")]
        Spectate,
        [Description("Local Match")]
        Local
    };

    public class Game_RocketLeague : AutoJsonNode<Game_RocketLeague>
    {
        public RLStatus Status;

        internal Game_RocketLeague(string json_data) : base(json_data) { }
    }

    /// <summary>
    /// A class representing various information relating to Rocket League
    /// </summary>
    public class GameState_RocketLeague : GameState
    {
        public Match_RocketLeague Match => NodeFor<Match_RocketLeague>("match");
        public Player_RocketLeague Player => NodeFor<Player_RocketLeague>("player");
        public Game_RocketLeague Game => NodeFor<Game_RocketLeague>("game");

        /// <summary>
        /// Creates a default GameState_RocketLeague instance.
        /// </summary>
        public GameState_RocketLeague() : base() { }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_RocketLeague(string json_data) : base(json_data) { }

        /// <summary>
        /// Returns true if all the color values for both teams are between zero and one.
        /// </summary>
        /// <returns></returns>
        public bool ColorsValid()
        {
            return (this.Match.Orange.Red >= 0 && this.Match.Blue.Red <= 1) &&
                   (this.Match.Orange.Green >= 0 && this.Match.Blue.Green <= 1) &&
                   (this.Match.Orange.Blue >= 0 && this.Match.Blue.Blue <= 1) &&
                   (this.Match.Orange.Red >= 0 && this.Match.Blue.Red <= 1) &&
                   (this.Match.Orange.Green >= 0 && this.Match.Blue.Green <= 1) &&
                   (this.Match.Orange.Blue >= 0 && this.Match.Blue.Blue <= 1);
        }

        public Team_RocketLeague OpponentTeam => Player.Team == 0 ? Match.Orange : Match.Blue;

        public Team_RocketLeague YourTeam => Player.Team == 0 ? Match.Blue : Match.Orange;
    }
}
