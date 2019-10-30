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

    public class Game_RocketLeague : Node<Game_RocketLeague>
    {
        public RLStatus Status;

        internal Game_RocketLeague(string json_data) : base(json_data)
        {
            Status = (RLStatus)GetInt("status");
        }
    }

    /// <summary>
    /// A class representing various information relating to Rocket League
    /// </summary>
    public class GameState_RocketLeague : GameState<GameState_RocketLeague>
    {
        private Player_RocketLeague _Player;
        private Match_RocketLeague _Match;
        private Game_RocketLeague _Game;

        /// <summary>
        /// Contains information referring to the Player
        /// </summary>
        public Player_RocketLeague Player => _Player ?? (_Player = new Player_RocketLeague(_ParsedData["player"]?.ToString() ?? ""));

        /// <summary>
        /// Contains information referring to the match the player is in
        /// </summary>
        public Match_RocketLeague Match => _Match ?? (_Match = new Match_RocketLeague(_ParsedData["match"]?.ToString() ?? ""));

        /// <summary>
        ///  Contains information referring to the general state of the game
        /// </summary>
        public Game_RocketLeague Game => _Game ?? (_Game = new Game_RocketLeague(_ParsedData["game"]?.ToString() ?? ""));

        /// <summary>
        /// Creates a default GameState_RocketLeague instance.
        /// </summary>
        public GameState_RocketLeague() : base()
        {
        }

        /// <summary>
        /// Creates a GameState instance based on the passed json data.
        /// </summary>
        /// <param name="json_data">The passed json data</param>
        public GameState_RocketLeague(string json_data) : base(json_data)
        {
        }

        /// <summary>
        /// A copy constructor, creates a GameState_RocketLeague instance based on the data from the passed GameState instance.
        /// </summary>
        /// <param name="other_state">The passed GameState</param>
        public GameState_RocketLeague(IGameState other_state) : base(other_state)
        {
        }

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
