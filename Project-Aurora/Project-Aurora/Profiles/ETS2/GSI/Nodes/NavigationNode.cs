namespace Aurora.Profiles.ETS2.GSI.Nodes {
    public class NavigationNode : Node<NavigationNode> {

        private Box<ETS2MemoryStruct> _memdat;

        /// <summary>Estimated time remaining in seconds.</summary>
        public int estimatedTime => (int)_memdat.value.navigationTime;

        /// <summary>Estimated distance to the destination in meters.</summary>
        public int estimatedDistance => (int)_memdat.value.navigationDistance;

        /// <summary>Current value of the "Route Advisor speed limit" in km/h.</summary>
        public int speedLimit => (int)(_memdat.value.navigationSpeedLimit * 3.6f);

        internal NavigationNode(string JSON) : base (JSON) { }
        internal NavigationNode() : base() { }

        /// <summary>
        /// Creates an instance of NavigationNode and populates the fields with the given memory data structure.
        /// </summary>
        /// <param name="memdat">Data to populate fields with.</param>
        internal NavigationNode(Box<ETS2MemoryStruct> memdat) {
            _memdat = memdat;
        }
    }
}
