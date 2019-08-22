namespace Aurora.Profiles.EliteDangerous.GSI.Nodes
{
    /// <summary>
    /// Class representing player status
    /// </summary>
    public class Status : Node<Status>
    {
        public string timestamp;
        public string @event;
        public long Flags;
        public int[] Pips;
        public int FireGroup;
        public int GuiFocus;
        public double Fuel;
        public double Cargo;
    }
}