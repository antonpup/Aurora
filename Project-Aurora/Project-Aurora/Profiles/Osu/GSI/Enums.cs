namespace Aurora.Profiles.Osu.GSI {
    public enum OsuStatus {
        NoFoundProcess = 1,
        Unkonwn = 2,
        SelectSong = 4,
        Playing = 8,
        Editing = 16,
        Rank = 32,
        MatchSetup = 64,
        Lobby = 128,
        Idle = 256
    }

    public enum OsuPlayMode {
        Unknown = -1,
        Osu = 0,
        Taiko = 1,
        CatchTheBeat = 2,
        Mania = 3
    }
}
