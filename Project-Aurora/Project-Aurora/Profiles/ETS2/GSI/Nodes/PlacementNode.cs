namespace Aurora.Profiles.ETS2.GSI.Nodes {
    public class PlacementNode : Node<PlacementNode> {

        /// <summary>X coordinate of the object.</summary>
        public float x;

        /// <summary>Y coordinate of the object.</summary>
        public float y;

        /// <summary>Z coordinate of the object.</summary>
        public float z;

        /// <summary>Heading of the object in range (0,1). 0 is North, 0.25 is West, 0.5 is South and 0.75 is East.</summary>
        public float heading;

        /// <summary>Pitch angle of the object in range (-0.25,0.25). 0 is flat relative to the surface, 0.25 is pointing directly upwards (away from the surface) and -0.25 is pointing directly downwards (into the surface).</summary>
        public float pitch;

        /// <summary>The roll of the object in range (-0.5,0.5). If the object is the truck: 0 is level, 0.25 is rolling 90 degrees to the left, -0.25 is rolling 90 degrees to the right.</summary>
        public float roll;

        internal PlacementNode(string JSON) : base (JSON) { }
        internal PlacementNode() : base() { }
    }
}
