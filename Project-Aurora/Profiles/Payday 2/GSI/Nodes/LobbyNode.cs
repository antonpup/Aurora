namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    /// <summary>
    /// Information about the game lobby
    /// </summary>
    public class LobbyNode : Node
    {
        /// <summary>
        /// Lobby difficulty
        /// </summary>
        public readonly LobbyDifficulty Difficulty;

        /// <summary>
        /// Lobby visibility permissions
        /// </summary>
        public readonly LobbyPermissions Permissions;

        /// <summary>
        /// A boolean representing if team AI is enabled
        /// </summary>
        public readonly bool IsTeamAIEnabled;

        /// <summary>
        /// Required level to join this lobby
        /// </summary>
        public readonly int RequiredLevel;

        /// <summary>
        /// A boolean representing if dropping in is enabled
        /// </summary>
        public readonly bool DropInEnabled;

        /// <summary>
        /// Lobby kick option
        /// </summary>
        public readonly LobbyKickSetting KickSetting;

        /// <summary>
        /// Lobby job plan
        /// </summary>
        public readonly LobbyJobPlan JobPlan;

        /// <summary>
        /// A boolean representing if cheaters are automatically kicked
        /// </summary>
        public readonly bool CheaterAutoKick;

        /// <summary>
        /// A boolean representing if lobby is singleplayer
        /// </summary>
        public readonly bool IsSingleplayer;

        internal LobbyNode(string JSON) : base(JSON)
        {
            Difficulty = GetEnum<LobbyDifficulty>("difficulty");
            Permissions = GetEnum<LobbyPermissions>("permission");
            IsTeamAIEnabled = GetBool("team_ai");
            RequiredLevel = GetInt("minimum_level");
            DropInEnabled = GetBool("drop_in");
            KickSetting = GetEnum<LobbyKickSetting>("kick_option");
            JobPlan = GetEnum<LobbyJobPlan>("job_plan");
            CheaterAutoKick = GetBool("cheater_auto_kick");
            IsSingleplayer = GetBool("singleplayer");
        }
    }

    /// <summary>
    /// Enum for every difficulty level
    /// </summary>
    public enum LobbyDifficulty
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,

        /// <summary>
        /// Normal difficluty
        /// </summary>
        Normal,

        /// <summary>
        /// Hard difficulty
        /// </summary>
        Hard,

        /// <summary>
        /// Very Hard difficulty
        /// </summary>
        Overkill,

        /// <summary>
        /// Overkill difficulty
        /// </summary>
        Overkill_145,

        /// <summary>
        /// Deathwish difficulty
        /// </summary>
        Overkill_290
    }

    /// <summary>
    /// Enum for each lobby permission
    /// </summary>
    public enum LobbyPermissions
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined,

        /// <summary>
        /// Public lobby
        /// </summary>
        Public,

        /// <summary>
        /// Friends only lobby
        /// </summary>
        Friends_only,

        /// <summary>
        /// Private lobby
        /// </summary>
        Private
    }

    /// <summary>
    /// Enum for lobby kick settings
    /// </summary>
    public enum LobbyKickSetting
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// No kick
        /// </summary>
        NoKick = 0,

        /// <summary>
        /// Host can kick
        /// </summary>
        HostKick = 1,

        /// <summary>
        /// Vote kicking
        /// </summary>
        VoteKick = 2
    }

    /// <summary>
    /// Enum for lobby job plan
    /// </summary>
    public enum LobbyJobPlan
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// Loud
        /// </summary>
        Loud = 1,

        /// <summary>
        /// Stealth
        /// </summary>
        Stealth = 2
    }
}
