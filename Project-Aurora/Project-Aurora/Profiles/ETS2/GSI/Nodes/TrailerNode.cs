namespace Aurora.Profiles.ETS2.GSI.Nodes {
    public class TrailerNode : Node<TrailerNode> {

        private ETS2MemoryStruct _memdat;

        /// <summary>Whether the trailer is attached to the truck or not.</summary>
        public bool attached => _memdat.trailer_attached != 0;

        /// <summary>ID of the cargo (internal).</summary>
        public string id;

        /// <summary>Localized name of the current trailer for display purposes.</summary>
        public string name;

        /// <summary>Mass of the cargo in kilograms.</summary>
        public float mass => _memdat.trailerMass;

        /// <summary>Current level of trailer wear/damage between 0 (min) and 1 (max).</summary>
        public float wear => _memdat.wearTrailer;

        /// <summary>Current trailer placement in the game world.</summary>
        public PlacementNode placement => new PlacementNode {
            x = _memdat.trailerCoordinateX,
            y = _memdat.trailerCoordinateY,
            z = _memdat.trailerCoordinateZ,
            heading = _memdat.trailerRotationX,
            pitch = _memdat.trailerRotationY,
            roll = _memdat.trailerRotationZ
        };

        internal TrailerNode(string JSON) : base (JSON) { }
        internal TrailerNode() : base() { }

        /// <summary>
        /// Creates an instance of TrailerNode and populates the fields with the given memory data structure.
        /// </summary>
        /// <param name="memdat">Data to populate fields with.</param>
        internal TrailerNode(ETS2MemoryStruct memdat) {
            _memdat = memdat;
        }
    }
}
