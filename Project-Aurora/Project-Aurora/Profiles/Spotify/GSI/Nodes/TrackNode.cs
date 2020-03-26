namespace Aurora.Profiles.Spotify.GSI
{
    public class TrackNode : AutoJsonNode<TrackNode>
    {
        public string Title;
        public string Artist;
        public string Album;

        public TrackNode(string json) : base(json) { }
    }
}