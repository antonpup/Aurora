using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.CSGO.GSI.Nodes
{
    /// <summary>
    /// A class representing the authentication information for GSI
    /// </summary>
    public class AuthNode : Node
    {
        /// <summary>
        /// The auth token sent by GSI
        /// </summary>
        public string Token;

        internal AuthNode(string JSON)
            : base(JSON)
        {
            Token = GetString("token");
        }

    }
}
