namespace Aurora.Profiles.ETS2.GSI.Nodes {
    public class VectorNode : Node {

        public float x;
        public float y;
        public float z;

        internal VectorNode(string JSON) : base (JSON) { }
        internal VectorNode() : base() { }
    }
}
