namespace Aurora.Profiles.ETS2.GSI.Nodes {
    public class GameNode : Node<GameNode> {

        private Box<ETS2MemoryStruct> _memdat;

        /// <summary>Whether or not the telemetry server is connected to the game.</summary>
        public bool connected => _memdat.value.ets2_telemetry_plugin_revision != 0 && _memdat.value.timeAbsolute != 0;

        /// <summary>The name of the game (e.g. "ETS2").</summary>
        public string gameName;

        /// <summary>Whether the game has been paused by the player.</summary>
        public bool paused => _memdat.value.paused != 0;

        /// <summary>The time scale in the game.</summary>
        public float timeScale => _memdat.value.localScale;

        /// <summary>The game major version.</summary>
        public int versionMajor => (int)_memdat.value.ets2_version_major;
        /// <summary>The game minor version.</summary>
        public int versionMinor => (int)_memdat.value.ets2_version_minor;

        /// <summary>The telemetry server major version.</summary>
        public int telemetryPluginVersion => (int)_memdat.value.ets2_telemetry_plugin_revision;

        internal GameNode(string JSON) : base (JSON) { }
        internal GameNode() : base() { }

        /// <summary>
        /// Creates an instance of GameNode and populates the fields with the given memory data structure.
        /// </summary>
        /// <param name="memdat">Data to populate fields with.</param>
        internal GameNode(Box<ETS2MemoryStruct> memdat) {
            _memdat = memdat;
        }
    }
}
