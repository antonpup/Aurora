namespace Aurora.Profiles.ETS2.GSI.Nodes {
    public class JobNode : Node<JobNode> {

        private Box<ETS2MemoryStruct> _memdat;

        /// <summary>Reward in internal game-specific currency.</summary>
        public int income => _memdat.value.jobIncome;

        /// <summary>Localized name of the source city for display purposes.</summary>
        public string sourceCity;

        /// <summary>Localized name of the source company for display purposes.</summary>
        public string sourceCompany;

        /// <summary>Localized name of the destination city for display purposes.</summary>
        public string destinationCity;

        /// <summary>Localized name of the destination company for display purposes.</summary>
        public string destinationCompany;

        internal JobNode(string JSON) : base (JSON) { }
        internal JobNode() : base() { }

        /// <summary>
        /// Creates an instance of JobNode and populates the fields with the given memory data structure.
        /// </summary>
        /// <param name="memdat">Data to populate fields with.</param>
        internal JobNode(Box<ETS2MemoryStruct> memdat) {
            _memdat = memdat;
        }
    }
}
