namespace Aurora.Profiles.Payday_2.GSI.Nodes
{
    public class LobbyNode : Node
    {
        public readonly LobbyDifficulty Difficulty;
        public readonly LobbyPermissions Permissions;
        public readonly bool IsTeamAIEnabled;
        public readonly int RequiredLevel;
        public readonly bool DropInEnabled;
        public readonly LobbyKickSetting KickSetting;
        public readonly LobbyJobPlan JobPlan;
        public readonly bool CheaterAutoKick;
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

    public enum LobbyDifficulty
    {
        Undefined,
        Normal,
        Hard,
        Overkill_145,
        Overkill_290
    }

    public enum LobbyPermissions
    {
        Undefined,
        Public,
        Friends_only,
        Private
    }

    public enum LobbyKickSetting
    {
        Undefined = -1,
        NoKick = 0,
        HostKick = 1,
        VoteKick = 2
    }

    public enum LobbyJobPlan
    {
        Undefined = -1,
        Loud = 1,
        Stealth = 2
    }
}
