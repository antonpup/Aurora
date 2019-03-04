namespace Aurora.Profiles.Osu.GSI {

    public class GameState_Osu : GameState<GameState_Osu> {

        private ProviderNode provider;
        public ProviderNode Provider => provider ?? (provider = new ProviderNode(GetNode("provider")));

        private GameNode game;
        public GameNode Game => game ?? (game = new GameNode(GetNode("game")));

        public GameState_Osu() : base() { }
        public GameState_Osu(string JSONstring) : base(JSONstring) { }
        public GameState_Osu(IGameState other) : base(other) { }
    }

    public class GameNode : AutoNode<GameNode> {

        public string Status;
        public float HP;
        public float Accuracy;
        public int Combo;
        public int Count50;
        public int Count100;
        public int Count200;
        public int Count300;
        public int CountKatu;
        public int CountGeki;
        public int CountMiss;

        internal GameNode() : base() { }
        internal GameNode(string json) : base(json) { }
    }
}
