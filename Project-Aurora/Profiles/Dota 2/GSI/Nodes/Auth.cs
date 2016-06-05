namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    public class Auth_Dota2 : Node
    {
        public readonly string Token;

        internal Auth_Dota2(string json_data) : base(json_data)
        {
            Token = GetString("token");
        }
    }
}
