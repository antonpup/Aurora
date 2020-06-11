namespace Aurora.Profiles.Dota_2.GSI.Nodes
{
    /// <summary>
    /// A class representing the authentication information for GSI
    /// </summary>
    public class Auth_Dota2 : Node
    {
        /// <summary>
        /// The auth token sent by this GSI
        /// </summary>
        public string Token;

        internal Auth_Dota2(string json_data) : base(json_data)
        {
            Token = GetString("token");
        }
    }
}
