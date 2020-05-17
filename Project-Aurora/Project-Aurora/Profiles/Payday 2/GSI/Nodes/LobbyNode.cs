namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    /// <summary>
    /// Information about the game lobby
    /// </summary>
    public class LobbyNode : AutoJsonNode<LobbyNode>
    {
        /// <summary>
        /// Lobby difficulty
        /// </summary>
        public LobbyDifficulty Difficulty;

        /// <summary>
        /// Lobby visibility permissions
        /// </summary>
        [AutoJsonPropertyName("permission")]
        public LobbyPermissions Permissions;

        /// <summary>
        /// A boolean representing if team AI is enabled
        /// </summary>
        [AutoJsonPropertyName("team_ai")]
        public bool IsTeamAIEnabled;

        /// <summary>
        /// Required level to join this lobby
        /// </summary>
        [AutoJsonPropertyName("minimum_level")]
        public int RequiredLevel;

        /// <summary>
        /// A boolean representing if dropping in is enabled
        /// </summary>
        [AutoJsonPropertyName("drop_in")]
        public bool DropInEnabled;

        /// <summary>
        /// Lobby kick option
        /// </summary>
        [AutoJsonPropertyName("kick_option")]
        public LobbyKickSetting KickSetting;

        /// <summary>
        /// Lobby job plan
        /// </summary>
        [AutoJsonPropertyName("job_plan")]
        public LobbyJobPlan JobPlan;

        /// <summary>
        /// A boolean representing if cheaters are automatically kicked
        /// </summary>
        [AutoJsonPropertyName("cheater_auto_kick")]
        public bool CheaterAutoKick;

        /// <summary>
        /// A boolean representing if lobby is singleplayer
        /// </summary>
        [AutoJsonPropertyName("singleplayer")]
        public bool IsSingleplayer;

        internal LobbyNode(string JSON) : base(JSON) { }
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
